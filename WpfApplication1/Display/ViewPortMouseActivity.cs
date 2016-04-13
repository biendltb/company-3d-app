﻿
using System.Windows.Media.Media3D;
using System.Windows;
using System.Collections.Generic;
using System;

namespace TIS_3dAntiCollision.Display
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
        public static Point3D ZoomViewPort(int delta)
        {
            Point3D current_pos = ViewPortManager.GetInstance.Camera.Position;
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
            double rotate_angle_unit_rad = 0.1 * Math.PI / 180;
            Point3D new_cam_pos = new Point3D(camera_pos.X, camera_pos.Y, camera_pos.Z);

            // MAKE THE CAMERA POSITION MOVE LEFT/RIGHT (ROTATE AROUND AXIS Y)
            double x_diff = new_mouse_pos.X - mouse_start_drag_position.X;
            if (x_diff != 0)
            {
                double y_rotate_angle_rad = -1 * x_diff * rotate_angle_unit_rad;
                // y not change
                double new_cam_pos_y = new_cam_pos.Y;
                
                // move the axis to view port center point
                double x_tmp = new_cam_pos.X - viewport_center_point.X;
                double z_tmp = new_cam_pos.Z - viewport_center_point.Z;

                double new_cam_pos_x = x_tmp * Math.Cos(y_rotate_angle_rad) - z_tmp * Math.Sin(y_rotate_angle_rad) + viewport_center_point.X;
                double new_cam_pos_z = x_tmp * Math.Sin(y_rotate_angle_rad) + z_tmp * Math.Cos(y_rotate_angle_rad) + viewport_center_point.Z;

                new_cam_pos = new Point3D(new_cam_pos_x, new_cam_pos_y, new_cam_pos_z);

                camera_pos = new_cam_pos;
                camera_look_direction = new Vector3D(viewport_center_point.X - camera_pos.X,
                    viewport_center_point.Y - camera_pos.Y,
                    viewport_center_point.Z - camera_pos.Z);
            }

            // MAKE THE CAMERA POSITION MOVE UP/DOWN 
            double y_diff = mouse_start_drag_position.Y - new_mouse_pos.Y;
            if (y_diff != 0)
            {
                double rotate_angle_rad = y_diff * rotate_angle_unit_rad;

                // move the axis to view port center point
                double x_tmp = new_cam_pos.X - viewport_center_point.X;
                double y_tmp = new_cam_pos.Y - viewport_center_point.Y;
                double z_tmp = new_cam_pos.Z - viewport_center_point.Z;

                // angle to x axis
                double rotate_x_plane_angle_rad = Math.Atan(z_tmp / x_tmp);
                if ((x_tmp < 0 && z_tmp > 0) || (x_tmp < 0 && z_tmp < 0))
                    rotate_x_plane_angle_rad += Math.PI;

                rotate_x_plane_angle_rad *= -1;
                
                // reflect camera position to x plane (rotate around y)
                double new_cam_pos_x_1 = x_tmp * Math.Cos(rotate_x_plane_angle_rad) - z_tmp * Math.Sin(rotate_x_plane_angle_rad);
                double new_cam_pos_y_1 = y_tmp;
                double new_cam_pos_z_1 = x_tmp * Math.Sin(rotate_x_plane_angle_rad) + z_tmp * Math.Cos(rotate_x_plane_angle_rad);

                // rotate around z
                double new_cam_pos_x_2 = new_cam_pos_x_1 * Math.Cos(rotate_angle_rad) - new_cam_pos_y_1 * Math.Sin(rotate_angle_rad);
                double new_cam_pos_y_2 = new_cam_pos_x_1 * Math.Sin(rotate_angle_rad) + new_cam_pos_y_1 * Math.Cos(rotate_angle_rad);
                double new_cam_pos_z_2 = new_cam_pos_z_1;

                // rotate back to the initial plane (rotate around y with invert direction)
                rotate_x_plane_angle_rad *= -1;

                double new_cam_pos_x_3 = new_cam_pos_x_2 * Math.Cos(rotate_x_plane_angle_rad) - new_cam_pos_z_2 * Math.Sin(rotate_x_plane_angle_rad) + viewport_center_point.X;
                double new_cam_pos_y_3 = new_cam_pos_y_2 + viewport_center_point.Y;
                double new_cam_pos_z_3 = new_cam_pos_x_2 * Math.Sin(rotate_x_plane_angle_rad) + new_cam_pos_z_2 * Math.Cos(rotate_x_plane_angle_rad) + viewport_center_point.Z;

                new_cam_pos = new Point3D(new_cam_pos_x_3, new_cam_pos_y_3, new_cam_pos_z_3);

                camera_pos = new_cam_pos;
                camera_look_direction = new Vector3D(viewport_center_point.X - camera_pos.X,
                    viewport_center_point.Y - camera_pos.Y,
                    viewport_center_point.Z - camera_pos.Z);
            }

            //Console.WriteLine("New Mouse Pos: " + new_mouse_pos.ToString());
            //Console.WriteLine("Start Mouse Pos: " + mouse_start_drag_position.ToString());
            //Console.WriteLine("New Cam Pos: " + camera_pos.ToString());
            //Console.WriteLine("New Look Direction: " + camera_look_direction.ToString());

            // update mouse start pos
            mouse_start_drag_position = new_mouse_pos;
        }
    }
}
