using System;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Collections.Generic;

using TIS_3dAntiCollision.Core;
using TIS_3dAntiCollision.Business.Profiling;
using TIS_3dAntiCollision.Model.DAO;
using TIS_3dAntiCollision.Display;
using TIS_3dAntiCollision.Services;

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

        MoveRoute move_direction = MoveRoute.NotMove;
        bool isWarningMode = false;

        public void CheckCollision()
        {
            // current stack container map that may contains obstacle containers
            Stack current_stack_map = ProfileController.GetInstance.Profile.GetMiddleContainerMap();
            //Point3D slow_next_point = getLimitSlowPoint();
            //ViewPortManager.GetInstance.DisplayVirtualBox(slow_next_point, MoveRoute.DownForward);
            double trolley_speed = PlcManager.GetInstance.RealTrolleySpeed;
            double hoist_speed = PlcManager.GetInstance.RealHoistSpeed;

            Vector movement_vector = new Vector(trolley_speed, hoist_speed);

            //double stop_range = movement_vector.Length * ConfigParameters.STOP_RANGE_SPEED_RATE;
            double stop_range = 20;
            double slow_range = movement_vector.Length * ConfigParameters.SLOW_RANGE_SPEED_RATE;

            //Logger.Log("Trolley speed: " + trolley_speed);
            //Logger.Log("Hoist speed: " + hoist_speed);
            //Logger.Log("Stop range: " + stop_range);
            //Logger.Log("Slow range: " + slow_range);

            // TODO: Check whether spreader holds container or not
            // Default is holding container
            Point lowest_spreader_complex_point = new Point(PlcManager.GetInstance.SpreaderPosition.X,
                                                            PlcManager.GetInstance.SpreaderPosition.Y + ConfigParameters.CONTAINER_HEIGHT);

            if (current_stack_map.Columns.Count == 0)
                return;

            MoveRoute current_move_direction = getDirection();

            double shortest_collision_distance = getDistanceCollision(movement_vector, lowest_spreader_complex_point, current_stack_map);
            Logger.Log("Distance to collision: " + shortest_collision_distance + " - Slow range: " + slow_range);

            if (shortest_collision_distance != 99999)
                if (shortest_collision_distance < stop_range)
                {
                    PlcManager.GetInstance.SetStopMode(current_move_direction);
                    isWarningMode = true;
                    //Logger.Log("Stop mode for anticollison is turned on.");
                }
                else
                    if (shortest_collision_distance < slow_range)
                    {
                        PlcManager.GetInstance.SetSlowMode(current_move_direction);
                        isWarningMode = true;
                        //Logger.Log("Slow mode for anticollison is turned on.");
                    }
                    else
                    {
                        // if in slow or stop mode and not change direction
                        if (current_move_direction == move_direction)
                        {
                            if (!isWarningMode)
                            {
                                //PlcManager.GetInstance.ResetNormalMode();
                            }
                        }
                        else
                        {
                            //PlcManager.GetInstance.ResetNormalMode();
                            isWarningMode = false;
                        }
                    }
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

        private Point3D getLimitSlowPoint()
        {
            Point3D spreader_pos = PlcManager.GetInstance.SpreaderPosition;
            Point3D holding_container_pos = new Point3D(spreader_pos.X, spreader_pos.Y + ConfigParameters.CONTAINER_HEIGHT, spreader_pos.Z);

            // find the next position of spreader in slow range follow movement vector
            double next_pos_x = holding_container_pos.X + PlcManager.GetInstance.TrolleySpeedPercent * ConfigParameters.SLOW_RANGE_SPEED_RATE;
            double next_pos_y = holding_container_pos.Y + PlcManager.GetInstance.HoistSpeedPercent * ConfigParameters.SLOW_RANGE_SPEED_RATE;

            return (new Point3D(next_pos_x, next_pos_y, spreader_pos.Z));
        }
        
        /// <summary>
        /// Check is there any intersection between sections which calculated
        /// base on the vector and the array of point and the line of column 
        /// which calculated based on the top container point of column 
        /// </summary>
        /// <param name="movement_vector">Vector of movement direction (x = trolley, y = hoist)</param>
        /// <param name="holding_container_point">The sensitive point of holding container based on the movement direction</param>
        /// <param name="top_column_point">First point of top column that spreader will meet based on the movement direction</param>
        /// <param name="range_limit">Range limit (slow or stop)</param>
        /// <returns></returns>
        private bool isIntersect(Vector movement_vector, Point holding_container_point, 
                                    Point top_column_point, double range_limit)
        {
            double intersection_height = holding_container_point.Y + (movement_vector.Y / movement_vector.X) * (top_column_point.X - holding_container_point.X);

            if (intersection_height > top_column_point.Y)
            {
                // length from sensetive point of holding container to the intersection point
                double intersection_length = Math.Pow((top_column_point.X - holding_container_point.X), 2)
                                            + Math.Pow((intersection_height - holding_container_point.Y), 2);
                if (intersection_length <= range_limit)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get distance to the nearest collision point
        /// </summary>
        /// <param name="movement_vector">Vector of movement direction (x = trolley, y = hoist)</param>
        /// <param name="lowest_spreader_complex_point">Based point of spreader
        /// If spreader is holding a container, it's will be the based point of the container</param>
        /// <param name="stack_map">Map of middle range and may include some obstacle</param>
        /// <returns></returns>
        private double getDistanceCollision(Vector movement_vector, Point lowest_spreader_complex_point, Stack stack_map)
        {
            //   _______
            //  |       |
            //  |       |
            // 2|_______|1

            double shortest_distance = 99999;
            double col_width = ConfigParameters.CONTAINER_WIDTH + ConfigParameters.DEFAULT_SPACE_BETWEEN_COLUMN;
            // CHECK THE COLUMN SIDE COLLISION
            // move forward
            if (movement_vector.X > 0)
            {
                Point sensitive_point = new Point(lowest_spreader_complex_point.X + ConfigParameters.CONTAINER_HEIGHT,
                                                    lowest_spreader_complex_point.Y);
                // current column
                int current_col_index = (int)(sensitive_point.X / col_width);

                for (int i = current_col_index + 1; i < stack_map.Columns.Count; i++)
                {
                    double collision_point_y = sensitive_point.Y + (movement_vector.Y / movement_vector.X)
                        * ((i * col_width) - sensitive_point.X);

                    // if collision point height is lower than column height. Note: invert
                    if (collision_point_y > (ConfigParameters.SENSOR_TO_GROUND_DISTANCE - stack_map.Columns[i].Quantity * ConfigParameters.CONTAINER_HEIGHT))
                    {
                        double shortest_distance_tmp = Math.Sqrt(Math.Pow((i * col_width) - sensitive_point.X, 2)
                                            + Math.Pow((collision_point_y - sensitive_point.Y), 2));

                        if (shortest_distance_tmp < shortest_distance)
                            shortest_distance = shortest_distance_tmp;
                        break;
                    }
                }
            }
            else
                // move revert
                if (movement_vector.X < 0)
                {
                    Point sensitive_point = lowest_spreader_complex_point;
                    // current column
                    int current_col_index = (int)(sensitive_point.X / col_width);

                    for (int i = current_col_index - 1; i >= 0; i--)
                    {
                        double collision_point_y = sensitive_point.Y + (movement_vector.Y / movement_vector.X)
                            * ((i * col_width + ConfigParameters.CONTAINER_WIDTH) - sensitive_point.X);

                        if (collision_point_y > (ConfigParameters.SENSOR_TO_GROUND_DISTANCE - stack_map.Columns[i].Quantity * ConfigParameters.CONTAINER_HEIGHT))
                        {
                            double shortest_distance_tmp = Math.Sqrt(Math.Pow((i * col_width + ConfigParameters.CONTAINER_HEIGHT - sensitive_point.X), 2)
                                + Math.Pow((collision_point_y - sensitive_point.Y), 2));

                            if (shortest_distance_tmp < shortest_distance)
                                shortest_distance = shortest_distance_tmp;
                            break;
                        }
                    }
                }

            // CHECK THE COLUMN TOP COLLISION
            // only happen when go down
            if (movement_vector.Y > 0)
            {
                // check the first sensitive point
                Point first_sensitive_point = lowest_spreader_complex_point;
                Point second_sensitive_point = new Point(lowest_spreader_complex_point.X + ConfigParameters.CONTAINER_WIDTH,
                                                            lowest_spreader_complex_point.Y);

                // check every top column which is lower than sensetive point
                for (int i = 0; i < stack_map.Columns.Count; i++)
                {
                    double col_height_tmp = ConfigParameters.SENSOR_TO_GROUND_DISTANCE - stack_map.Columns[i].Quantity * ConfigParameters.CONTAINER_HEIGHT;
                    if (first_sensitive_point.Y < col_height_tmp)
                    {
                        double first_collision_pos_x = first_sensitive_point.X
                            + (movement_vector.X / movement_vector.Y) * (col_height_tmp - first_sensitive_point.Y);

                        double second_collision_pos_x = second_sensitive_point.X
                            + (movement_vector.X / movement_vector.Y) * (col_height_tmp - second_sensitive_point.Y);

                        double start_col_x = i * col_width;
                        double end_col_x = start_col_x + ConfigParameters.CONTAINER_WIDTH;
                        if ((first_collision_pos_x >= start_col_x && first_collision_pos_x <= end_col_x))
                        {
                            double first_shortest_distance_tmp = Math.Sqrt(Math.Pow((first_collision_pos_x - first_sensitive_point.X), 2)
                                                                + Math.Pow((col_height_tmp - first_sensitive_point.Y), 2));
                            if (first_shortest_distance_tmp < shortest_distance)
                                shortest_distance = first_shortest_distance_tmp;
                        }


                        if (second_collision_pos_x >= start_col_x && second_collision_pos_x <= end_col_x)
                        {
                            double second_shortest_distance_tmp = Math.Sqrt(Math.Pow((second_collision_pos_x - second_sensitive_point.X), 2)
                                                                + Math.Pow((col_height_tmp - second_sensitive_point.Y), 2));
                            if (second_shortest_distance_tmp < shortest_distance)
                                shortest_distance = second_shortest_distance_tmp;
                        }
                    }
                }
            }
            return shortest_distance;
        }
    }
}
