using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TIS_3dAntiCollision.Core;
using TIS_3dAntiCollision.Core.PlcTypes;
using System.Windows.Media.Media3D;

namespace TIS_3dAntiCollision.Business
{
    public sealed class PlcManager
    {
        static readonly PlcManager pm = new PlcManager();

        private PLC plc = new PLC(CPU_Type.S7300, 
                                    ConfigFileManager.ReadPlcIp(), 
                                    short.Parse(ConfigFileManager.ReadPlcRack()), 
                                    short.Parse(ConfigFileManager.ReadPlcSlot()));

        public DBStruct OnlineDataBlock = new DBStruct();

        private bool isConnect = false;

        public bool IsConnect
        {
            get { return isConnect; }
        }

        // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
        static PlcManager(){}

        PlcManager(){}

        public static PlcManager GetInstance
        {
            get { return pm; }
        }

        public ErrorCode Connect()
        {
            ErrorCode connect_result = plc.Open();

            if (connect_result == ErrorCode.NoError)
                isConnect = true;

            return connect_result; 
        }

        public void Close()
        {
            isConnect = false;
            plc.Close();
        }

        public void Write(string var_name, object value)
        {
            plc.Write(var_name, value);
        }

        public object Read(string var_name)
        {
            return plc.Read(var_name);
        }

        public void ReadStruct()
        {
            OnlineDataBlock = (DBStruct)plc.ReadStruct(typeof(DBStruct), ConfigParameters.DATA_BLOCK_NUMBER);
        }

        public string WriteStruct()
        {
            return plc.WriteStruct(OnlineDataBlock, ConfigParameters.DATA_BLOCK_NUMBER).ToString();
        }

        public void TriggerMiniMotor()
        {
            PlcManager.GetInstance.OnlineDataBlock.Start_swivel = true;
            //PlcManager.GetInstance.OnlineDataBlock.Remote = true;
            PlcManager.GetInstance.WriteStruct();
        }

        public Point3D SensorPosition
        {
            get
            {
                return new Point3D(OnlineDataBlock.X_post + ConfigParameters.SENSOR_OFFSET_X,
                    ConfigParameters.SENSOR_TO_GROUND_DISTANCE - OnlineDataBlock.Y_post,
                    ConfigParameters.SENSOR_OFFSET_Z);
            }
        }

        // Position of spreader only, not the holding container
        // ,-----^^------,0
        public Point3D SpreaderPosition
        {
            get
            {
                return new Point3D(OnlineDataBlock.X_post + ConfigParameters.SPREADER_OFFSET_X,
                    ConfigParameters.SENSOR_TO_GROUND_DISTANCE - OnlineDataBlock.Y_post,
                    ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2);
            }
        }

        public double TrolleySpeedPercent
        {
            get { return OnlineDataBlock.X_Speed / ConfigParameters.MAX_SPEED * 100; }
        }

        public double HoistSpeedPercent
        {
            get { return OnlineDataBlock.Y_Speed / ConfigParameters.MAX_SPEED * 100; }
        }

        public double RealTrolleySpeed
        {
            get 
            { 
                return (OnlineDataBlock.X_Speed / ConfigParameters.MAX_SPEED) * 1350 / (60 * 41.2) * 90 * Math.PI;
            }
        }

        public double RealHoistSpeed
        {
            get
            {
                return (OnlineDataBlock.Y_Speed / ConfigParameters.MAX_SPEED) * 860 / (60 * 23.8 * 2) * 120 * Math.PI;
            }
        }

        public void SetSlowMode(MoveRoute move_direction)
        {
            resetSlow();
            if (move_direction != MoveRoute.NotMove)
            {
                switch (move_direction)
                {
                    case MoveRoute.Up:
                        OnlineDataBlock.H_Up_Slow = true;
                        break;
                    case MoveRoute.Down:
                        OnlineDataBlock.H_Down_Slow = true;
                        break;
                    case MoveRoute.Forward:
                        OnlineDataBlock.T_Forward_Slow = true;
                        break;
                    case MoveRoute.Reverse:
                        OnlineDataBlock.T_Reverse_Slow = true;
                        break;
                    case MoveRoute.UpForward:
                        OnlineDataBlock.H_Up_Slow = true;
                        OnlineDataBlock.T_Forward_Slow = true;
                        break;
                    case MoveRoute.UpReverse:
                        OnlineDataBlock.H_Up_Slow = true;
                        OnlineDataBlock.T_Reverse_Slow = true;
                        break;
                    case MoveRoute.DownForward:
                        OnlineDataBlock.H_Down_Slow = true;
                        OnlineDataBlock.T_Forward_Slow = true;
                        break;
                    case MoveRoute.DownReverse:
                        OnlineDataBlock.H_Down_Slow = true;
                        OnlineDataBlock.T_Reverse_Slow = true;
                        break;
                }
                WriteStruct();
            }
        }

        public void SetStopMode(MoveRoute move_direction)
        {
            resetStop();

            if (move_direction != MoveRoute.NotMove)
            {
                switch (move_direction)
                {
                    case MoveRoute.Up:
                        OnlineDataBlock.H_Up_Stop = true;
                        break;
                    case MoveRoute.Down:
                        OnlineDataBlock.H_Down_Stop = true;
                        break;
                    case MoveRoute.Forward:
                        OnlineDataBlock.T_Forward_Stop = true;
                        break;
                    case MoveRoute.Reverse:
                        OnlineDataBlock.T_Reverse_Stop = true;
                        break;
                    case MoveRoute.UpForward:
                        OnlineDataBlock.H_Up_Stop = true;
                        OnlineDataBlock.T_Forward_Stop = true;
                        break;
                    case MoveRoute.UpReverse:
                        OnlineDataBlock.H_Up_Stop = true;
                        OnlineDataBlock.T_Reverse_Stop = true;
                        break;
                    case MoveRoute.DownForward:
                        OnlineDataBlock.H_Down_Stop = true;
                        OnlineDataBlock.T_Forward_Stop = true;
                        break;
                    case MoveRoute.DownReverse:
                        OnlineDataBlock.H_Down_Stop = true;
                        OnlineDataBlock.T_Reverse_Stop = true;
                        break;
                }
                WriteStruct();
            }
        }

        public void ResetNormalMode()
        {
            resetStop();
            resetSlow();

            WriteStruct();
        }

        private void resetSlow()
        {
            OnlineDataBlock.H_Up_Slow = false;
            OnlineDataBlock.H_Down_Slow = false;
            OnlineDataBlock.T_Forward_Slow = false;
            OnlineDataBlock.T_Reverse_Slow = false;

            //WriteStruct();
        }

        private void resetStop()
        {
            OnlineDataBlock.H_Up_Stop = false;
            OnlineDataBlock.H_Down_Stop = false;
            OnlineDataBlock.T_Forward_Stop = false;
            OnlineDataBlock.T_Reverse_Stop = false;

            //WriteStruct();
        }
        
    }
}
