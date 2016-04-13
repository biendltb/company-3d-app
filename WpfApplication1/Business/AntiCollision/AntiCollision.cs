using System;
using TIS_3dAntiCollision.Core;
using TIS_3dAntiCollision.Business.Profiling;
using TIS_3dAntiCollision.Model.DAO;
using System.Windows;
using System.Windows.Media.Media3D;

namespace TIS_3dAntiCollision.Business.AntiCollision
{
    public sealed class AntiCollision
    {
        static readonly AntiCollision ac = new AntiCollision();
        static AntiCollision() { }
        AntiCollision() { }

        public static AntiCollision GetInstance
        {
            get { return ac; }
        }

        public void CheckCollision()
        {
            Stack s = ProfileController.GetInstance.Profile.GetMiddleContainerMap();

            
        }

        private MoveRoute getDirection()
        {
            double hoist_speed = PlcManager.GetInstance.HoistSpeedPercent;
            double trolley_speed = PlcManager.GetInstance.TrolleySpeedPercent;

            if (hoist_speed > 0)
                if (trolley_speed == 0)
                    return MoveRoute.Down;
                else
                    if (trolley_speed > 0)
                        return MoveRoute.DownForward;
                    else
                        return MoveRoute.DownReverse;
            else
                if (hoist_speed == 0 && trolley_speed != 0)
                    if (trolley_speed > 0)
                        return MoveRoute.Forward;
                    else
                        return MoveRoute.Reverse;
                else
                    if (hoist_speed < 0)
                        if (trolley_speed == 0)
                            return MoveRoute.Up;
                        else
                            if (trolley_speed > 0)
                                return MoveRoute.UpForward;
                            else
                                return MoveRoute.UpReverse;

            return MoveRoute.NotMove;
        }

        private Point getLimitSlowPoint()
        {
            Point3D spreader_pos = PlcManager.GetInstance.GetSpreaderPosition();
            Point holding_container_pos = new Point(spreader_pos.X, spreader_pos.Y + ConfigParameters.CONTAINER_HEIGHT);

            // find the next position of spreader in slow range follow movement vector
            double next_pos_x = holding_container_pos.X + PlcManager.GetInstance.TrolleySpeedPercent * ConfigParameters.SLOW_RANGE_SPEED_RATE;
            double next_pos_y = holding_container_pos.Y + PlcManager.GetInstance.HoistSpeedPercent * ConfigParameters.SLOW_RANGE_SPEED_RATE;

            return (new Point(next_pos_x, next_pos_y));
        }
    }
}
