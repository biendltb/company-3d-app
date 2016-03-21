using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System;
using TIS_3dAntiCollision.Core;

namespace TIS_3dAntiCollision.Business
{
    class ContainerStackProfiler2
    {
        private Point3D[] point_data;

        public ContainerStackProfiler2(Point3D[] point_data)
        {
            this.point_data = cropData(point_data);
        }

        private Point3D[] cropData(Point3D[] points_3d)
        {
            List<Point3D> result = new List<Point3D>();

            for (int i = 0; i < points_3d.Length; i++)
                if (points_3d[i].X >= ConfigParameters.MIN_X_RANGE && points_3d[i].X <= ConfigParameters.MAX_X_RANGE // valid x range
                    && points_3d[i].Y >= ConfigParameters.MIN_Y_RANGE && points_3d[i].Y <= ConfigParameters.MAX_Y_RANGE) // valide y range
                    result.Add(points_3d[i]);

            return result.ToArray();
        }

    }
}
