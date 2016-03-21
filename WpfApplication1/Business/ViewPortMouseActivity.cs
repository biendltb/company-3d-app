
using System.Windows.Media.Media3D;
using System.Windows;
using System.Collections.Generic;
using System;

namespace TIS_3dAntiCollision.Business
{
    static class ViewPortMouseActivity
    {
        private static Point3D viewport_center_point = new Point3D(735, 1280, 0);

        private static Point mouse_start_drag_position;


        /// <summary>
        /// Calculate camera position when mouse scroll
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="current_pos"></param>
        /// <returns>Return new camera position</returns>
        public static Point3D ZoomViewPort(int delta, Point3D current_pos)
        {
            // set the t
            double t = -0.01;

            if (delta < 0)
                t = 0.01;

            return new Point3D(current_pos.X + (current_pos.X - viewport_center_point.X) * t,
                                current_pos.Y + (current_pos.Y - viewport_center_point.Y) * t,
                                current_pos.Z + (current_pos.Z - viewport_center_point.Z) * t);
        }

        public static void SetStartDragPoint(Point position)
        {
            mouse_start_drag_position = position;
        }

        public static void RotateViewPort(Point new_mouse_pos, ref Point3D camera_pos, ref Vector3D camera_look_direction)
        {
            // change rate
            double t = 1;
            Point3D new_cam_pos = new Point3D(camera_pos.X, camera_pos.Y, camera_pos.Z);

            // calculate the radius of sphere
            // R^2 = (x - a)^2 + (y - b)^2 + (z - c)^2
            double radius = Math.Sqrt((camera_pos.X - viewport_center_point.X) * (camera_pos.X - viewport_center_point.X)
                + (camera_pos.Y - viewport_center_point.Y) * (camera_pos.Y - viewport_center_point.Y)
                + (camera_pos.Z - viewport_center_point.Z) * (camera_pos.Z - viewport_center_point.Z));

            // MAKE THE CAMERA POSITION MOVE UP/DOWN (Y invert)
            double y_diff = mouse_start_drag_position.Y - new_mouse_pos.Y;
            if (y_diff != 0)
            {
                // z = R^2 - (x - a)^2 - (y - b)^2
                double new_cam_pos_x = new_cam_pos.X;
                double new_cam_pos_y = new_cam_pos.Y + y_diff * t;
                double new_cam_pos_z = Math.Sqrt(Math.Abs(radius * radius
                    - (new_cam_pos_x - viewport_center_point.X) * (new_cam_pos_x - viewport_center_point.X)
                    - (new_cam_pos_y - viewport_center_point.Y) * (new_cam_pos_y - viewport_center_point.Y)))
                    + viewport_center_point.Z;

                new_cam_pos = new Point3D(new_cam_pos_x, new_cam_pos_y, new_cam_pos_z);
            }
            
            // MAKE THE CAMERA POSITION MOVE LEFT/RIGHT
            double x_diff = new_mouse_pos.X - mouse_start_drag_position.X;
            if (x_diff != 0)
            {
                double new_cam_pos_x = new_cam_pos.X + x_diff * t;
                double new_cam_pos_y = new_cam_pos.Y;
                double new_cam_pos_z = Math.Sqrt(Math.Abs(radius * radius
                    - (new_cam_pos_x - viewport_center_point.X) * (new_cam_pos_x - viewport_center_point.X)
                    - (new_cam_pos_y - viewport_center_point.Y) * (new_cam_pos_y - viewport_center_point.Y)))
                    + viewport_center_point.Z;

                new_cam_pos = new Point3D(new_cam_pos_x, new_cam_pos_y, new_cam_pos_z);
            }
            

            if (x_diff != 0 && y_diff != 0)
            {
                camera_pos = new_cam_pos;
                camera_look_direction = new Vector3D(viewport_center_point.X - camera_pos.X,
                    viewport_center_point.Y - camera_pos.Y,
                    viewport_center_point.Z - camera_pos.Z);
            }

            Console.WriteLine("New Mouse Pos: " + new_mouse_pos.ToString());
            Console.WriteLine("Start Mouse Pos: " + mouse_start_drag_position.ToString());
            Console.WriteLine("New Cam Pos: " + camera_pos.ToString());
            Console.WriteLine("New Look Direction: " + camera_look_direction.ToString());


            // update mouse start pos
            mouse_start_drag_position = new_mouse_pos;
        }
    }
}
