using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using TIS_3dAntiCollision.Core;

namespace TIS_3dAntiCollision.Display
{
    class Container3dDataGenerator
    {
        private Point3D container_pos;
        private ContainerTypes container_type;

        public Container3dDataGenerator(Point3D container_pos, ContainerTypes container_type)
        {
            this.container_pos = container_pos;
            this.container_type = container_type;
        }

        public MeshGeometry3D CreateContainer()
        {
            MeshGeometry3D container = new MeshGeometry3D();

            // set point for container based on one point
            Point3DCollection corners = new Point3DCollection();
            corners.Add(new Point3D(container_pos.X + 1, container_pos.Y + 1, container_pos.Z + 1));
            corners.Add(new Point3D(container_pos.X + 0, container_pos.Y + 1, container_pos.Z + 1));
            corners.Add(new Point3D(container_pos.X + 0, container_pos.Y + 0, container_pos.Z + 1));
            corners.Add(new Point3D(container_pos.X + 1, container_pos.Y + 0, container_pos.Z + 1));
            corners.Add(new Point3D(container_pos.X + 1, container_pos.Y + 1, container_pos.Z - 1));
            corners.Add(new Point3D(container_pos.X + 0, container_pos.Y + 1, container_pos.Z - 1));
            corners.Add(new Point3D(container_pos.X + 0, container_pos.Y + 0, container_pos.Z - 1));
            corners.Add(new Point3D(container_pos.X + 1, container_pos.Y + 0, container_pos.Z - 1));

            container.Positions = corners;

            Int32[] indices = {
               //front
                 0,1,2,
                 0,2,3,
              //back
                 4,7,6,
                 4,6,5,
              //Right
                 4,0,3,
                 4,3,7,
              //Left
                 1,5,6,
                 1,6,2,
              //Top
                 1,0,4,
                 1,4,5,
              //Bottom
                 2,6,7,
                 2,7,3
              };

            Int32Collection Triangles = new Int32Collection();
            foreach (Int32 index in indices)
                Triangles.Add(index);
            container.TriangleIndices = Triangles;

            return container; 
        }
    }
}
