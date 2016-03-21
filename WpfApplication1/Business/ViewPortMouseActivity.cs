
using System.Windows.Media.Media3D;

namespace TIS_3dAntiCollision.Business
{
    static class ViewPortMouseActivity
    {
        public static Point3D viewport_center_point = new Point3D(735, 1280, 0);

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
    }
}
