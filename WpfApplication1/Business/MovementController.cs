using System.Collections.Generic;
using TIS_3dAntiCollision.Core;
using System;
using TIS_3dAntiCollision.Services;

namespace TIS_3dAntiCollision.Business
{
    static class MovementController
    {
        private static bool isOnMove = false;

        public static bool IsOnMove
        {
            get { return MovementController.isOnMove; }
        }

        // list contains all stops
        private static List<double> list_trolley_stop = new List<double>();

        // list contains speed according to the path to each stop
        private static List<short> list_speed = new List<short>();

        public static void Execute()
        {
            if (list_trolley_stop.Count > 0)
            {
                // first in first out excute. Move one by one stop with a specific speed
                if (move(list_trolley_stop[0], list_speed[0]))
                {
                    list_trolley_stop.RemoveAt(0);
                    list_speed.RemoveAt(0);
                }
            }
        }

        public static void AddMove(double des_pos)
        {
            AddMove(des_pos, ConfigParameters.NORMAL_SPEED);
        }

        public static void AddMove(double des_pos, short trolley_speed)
        {
            list_trolley_stop.Add(des_pos);
            list_speed.Add(trolley_speed);
        }

        private static bool move(double des_pos, short speed)
        {
            double current_pos = PlcManager.GetInstance.OnlineDataBlock.X_post;

            if (Math.Abs(current_pos - des_pos) <= ConfigParameters.MIN_TROLLEY_STOP_RANGE)
            {
                // finish task
                stopTrolley();
                //Logger.Log("Stopped at: " + current_pos);
                isOnMove = false;
                return true;
            }
            else
                // move forward to destination
                if (current_pos < des_pos)
                {
                    moveForward(speed);
                    isOnMove = true;
                    return false;
                }
                else
                    // move revert to destination
                {
                    moveRevert(speed);
                    isOnMove = true;
                    return false;
                }
        }

        private static void stopTrolley()
        {
            PlcManager.GetInstance.OnlineDataBlock.Remote = false;
            PlcManager.GetInstance.OnlineDataBlock.Run_Bit = false;
            PlcManager.GetInstance.OnlineDataBlock.T_SetPoint = 0;
            PlcManager.GetInstance.WriteStruct();
        }

        private static void moveForward(short trolley_speed)
        {
            PlcManager.GetInstance.OnlineDataBlock.Remote = true;
            PlcManager.GetInstance.OnlineDataBlock.Run_Bit = true;
            PlcManager.GetInstance.OnlineDataBlock.T_SetPoint = getRealSpeed(trolley_speed);
            PlcManager.GetInstance.WriteStruct();
        }

        private static void moveRevert(short trolley_speed)
        {
            PlcManager.GetInstance.OnlineDataBlock.Remote = true;
            PlcManager.GetInstance.OnlineDataBlock.Run_Bit = true;
            PlcManager.GetInstance.OnlineDataBlock.T_SetPoint = getRealSpeed((short)(-1 * trolley_speed));
            PlcManager.GetInstance.WriteStruct();
        }

        private static short getRealSpeed(short speed_percent)
        {
            return (short)(speed_percent * ConfigParameters.MAX_TROLLEY_SPEED / 100);
        }
    }
}
