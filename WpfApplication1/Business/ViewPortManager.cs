using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System;
using TIS_3dAntiCollision.Core;

namespace TIS_3dAntiCollision.Business
{
    class ViewPortManager
    {
        Viewport3D view_port;
        public PerspectiveCamera Camera = new PerspectiveCamera();
        DirectionalLight directional_light = new DirectionalLight();

        // model 3D
        ModelVisual3D model_visual = new ModelVisual3D();
        Model3DGroup model_group = new Model3DGroup();

        // default parameter
        // CAMERA PARAMS
        private const double camera_far_plane_distance = 10000;
        private const double camera_near_plane_distance = 300;
        private const double camera_horizontal_field_of_view = 90;
        private Point3D camera_position = new Point3D(150, 250, 1050);
        // calculate based on the view port center point
        private Vector3D camera_look_direction = new Vector3D(600, 1000, -1050);
        private Vector3D camera_up_direction = new Vector3D(0, -1, 0);

        // LIGHT PARAMS
        private Color light_color = Colors.White;
        private Vector3D light_direction = new Vector3D(600, 1300, 100);

        public ViewPortManager(Viewport3D view_port)
        {
            this.view_port = view_port;

            init_prams();

            init_display();
        }

        /// <summary>
        /// Set default parameter for camera and light
        /// </summary>
        private void init_prams()
        {
            Camera.FarPlaneDistance = camera_far_plane_distance;
            Camera.NearPlaneDistance = camera_near_plane_distance;
            Camera.FieldOfView = camera_horizontal_field_of_view;
            Camera.Position = camera_position;
            Camera.LookDirection = camera_look_direction;
            Camera.UpDirection = camera_up_direction;

            directional_light.Color = light_color;
            directional_light.Direction = light_direction;
        }

        private void init_display()
        {
            view_port.Camera = Camera;

            model_group.Children.Add(directional_light);
            model_group.Children.Add(new AmbientLight(Colors.DimGray));

            model_visual.Content = model_group;

            view_port.Children.Add(model_visual);

            // create the base lines
            createLineNet(model_group);
        }

        public void DisplayContainerStack(Point3D[] stack_container_position, double container_length)
        {
            // add container stack to view
            foreach (Point3D point in stack_container_position)
                model_group.Children.Add(getContainer(point, container_length, getContainerColor(point.Y)));

        }

        // create the square hollow to represent the line
        private GeometryModel3D getLine(Point3D start_point, Point3D end_point, double thickness)
        {
            GeometryModel3D geometry_model = new GeometryModel3D();
            MeshGeometry3D triangle_mesh = new MeshGeometry3D();

            // four points in start
            triangle_mesh.Positions.Add(start_point);
            triangle_mesh.Positions.Add(new Point3D(start_point.X, start_point.Y, start_point.Z + thickness));
            triangle_mesh.Positions.Add(new Point3D(start_point.X, start_point.Y + thickness, start_point.Z + thickness));
            triangle_mesh.Positions.Add(new Point3D(start_point.X, start_point.Y + thickness, start_point.Z));

            // four point in the end
            triangle_mesh.Positions.Add(end_point);
            triangle_mesh.Positions.Add(new Point3D(end_point.X, end_point.Y, end_point.Z + thickness));
            triangle_mesh.Positions.Add(new Point3D(end_point.X, end_point.Y + thickness, end_point.Z + thickness));
            triangle_mesh.Positions.Add(new Point3D(end_point.X, end_point.Y + thickness, end_point.Z));

            // top
            triangle_mesh.TriangleIndices.Add(4);
            triangle_mesh.TriangleIndices.Add(0);
            triangle_mesh.TriangleIndices.Add(1);
            triangle_mesh.TriangleIndices.Add(1);
            triangle_mesh.TriangleIndices.Add(5);
            triangle_mesh.TriangleIndices.Add(4);

            // left side
            triangle_mesh.TriangleIndices.Add(7);
            triangle_mesh.TriangleIndices.Add(3);
            triangle_mesh.TriangleIndices.Add(0);
            triangle_mesh.TriangleIndices.Add(0);
            triangle_mesh.TriangleIndices.Add(4);
            triangle_mesh.TriangleIndices.Add(7);

            // right side
            triangle_mesh.TriangleIndices.Add(1);
            triangle_mesh.TriangleIndices.Add(2);
            triangle_mesh.TriangleIndices.Add(6);
            triangle_mesh.TriangleIndices.Add(6);
            triangle_mesh.TriangleIndices.Add(5);
            triangle_mesh.TriangleIndices.Add(1);

            // bottom side
            triangle_mesh.TriangleIndices.Add(6);
            triangle_mesh.TriangleIndices.Add(2);
            triangle_mesh.TriangleIndices.Add(3);
            triangle_mesh.TriangleIndices.Add(3);
            triangle_mesh.TriangleIndices.Add(7);
            triangle_mesh.TriangleIndices.Add(6);

            geometry_model.Geometry = triangle_mesh;
            //geometry_model.Material = new DiffuseMaterial(new SolidColorBrush(Colors.LawnGreen));
            geometry_model.Material = new EmissiveMaterial(new SolidColorBrush(Colors.LawnGreen));

            return geometry_model;
        }

        // create the base line net on X-Z plane
        private void createLineNet(Model3DGroup model_group)
        {
            // resolution: 50mm
            // x: 0 -> 2000
            // z: -800 -> 800

            int net_resolution = 50;
            double y_plane = ConfigParameters.SENSOR_TO_GROUND_DISTANCE;
            double line_thickness = 1;

            double start_x = 0,
                end_x = 2000,
                start_z = -800,
                end_z = 800;

            for (int i = (int)(start_x /net_resolution); i < end_x / net_resolution; i++)
            {
                model_group.Children.Add(getLine(new Point3D(i * net_resolution, y_plane, start_z), new Point3D(i * net_resolution, y_plane, end_z), line_thickness));
            }

            for (int i = (int)(start_z / net_resolution); i < end_z / net_resolution + 1; i++)
            {
                model_group.Children.Add(getLine(new Point3D(start_x, y_plane, i * net_resolution), new Point3D(end_x, y_plane, i * net_resolution), line_thickness));
            }



        }

        // generate model 
        private GeometryModel3D getContainer(Point3D container_position, double container_length, Color container_color)
        {
            GeometryModel3D geometry_model = new GeometryModel3D();
            MeshGeometry3D triangle_mesh = new MeshGeometry3D();

            triangle_mesh.Positions.Add(container_position);
            triangle_mesh.Positions.Add(new Point3D(container_position.X, container_position.Y - ConfigParameters.CONTAINER_HEIGHT, container_position.Z));
            triangle_mesh.Positions.Add(new Point3D(container_position.X + ConfigParameters.CONTAINER_WIDTH, container_position.Y - ConfigParameters.CONTAINER_HEIGHT, container_position.Z));
            triangle_mesh.Positions.Add(new Point3D(container_position.X + ConfigParameters.CONTAINER_WIDTH, container_position.Y, container_position.Z));

            triangle_mesh.Positions.Add(new Point3D(container_position.X, container_position.Y,
                                                        container_position.Z - container_length));
            triangle_mesh.Positions.Add(new Point3D(container_position.X, container_position.Y - ConfigParameters.CONTAINER_HEIGHT,
                                                        container_position.Z - container_length));
            triangle_mesh.Positions.Add(new Point3D(container_position.X + ConfigParameters.CONTAINER_WIDTH, container_position.Y - ConfigParameters.CONTAINER_HEIGHT,
                                                        container_position.Z - container_length));
            triangle_mesh.Positions.Add(new Point3D(container_position.X + ConfigParameters.CONTAINER_WIDTH, container_position.Y,
                                                        container_position.Z - container_length));

            // top
            triangle_mesh.TriangleIndices.Add(1);
            triangle_mesh.TriangleIndices.Add(5);
            triangle_mesh.TriangleIndices.Add(6);
            triangle_mesh.TriangleIndices.Add(6);
            triangle_mesh.TriangleIndices.Add(2);
            triangle_mesh.TriangleIndices.Add(1);

            // left side
            triangle_mesh.TriangleIndices.Add(6);
            triangle_mesh.TriangleIndices.Add(7);
            triangle_mesh.TriangleIndices.Add(3);
            triangle_mesh.TriangleIndices.Add(3);
            triangle_mesh.TriangleIndices.Add(2);
            triangle_mesh.TriangleIndices.Add(6);

            // right side
            triangle_mesh.TriangleIndices.Add(5);
            triangle_mesh.TriangleIndices.Add(1);
            triangle_mesh.TriangleIndices.Add(0);
            triangle_mesh.TriangleIndices.Add(0);
            triangle_mesh.TriangleIndices.Add(4);
            triangle_mesh.TriangleIndices.Add(5);

            // bottom side
            triangle_mesh.TriangleIndices.Add(4);
            triangle_mesh.TriangleIndices.Add(0);
            triangle_mesh.TriangleIndices.Add(3);
            triangle_mesh.TriangleIndices.Add(3);
            triangle_mesh.TriangleIndices.Add(7);
            triangle_mesh.TriangleIndices.Add(4);

            // first square side
            triangle_mesh.TriangleIndices.Add(1);
            triangle_mesh.TriangleIndices.Add(2);
            triangle_mesh.TriangleIndices.Add(3);
            triangle_mesh.TriangleIndices.Add(3);
            triangle_mesh.TriangleIndices.Add(0);
            triangle_mesh.TriangleIndices.Add(1);

            // second square side
            triangle_mesh.TriangleIndices.Add(6);
            triangle_mesh.TriangleIndices.Add(5);
            triangle_mesh.TriangleIndices.Add(4);
            triangle_mesh.TriangleIndices.Add(4);
            triangle_mesh.TriangleIndices.Add(7);
            triangle_mesh.TriangleIndices.Add(6);


            geometry_model.Geometry = triangle_mesh;
            geometry_model.Material = new DiffuseMaterial(new SolidColorBrush(container_color));
            //geometry_model.Material = new EmissiveMaterial(new SolidColorBrush(container_color));

            return geometry_model;
        }

        // get the color of container based on its height level
        private Color getContainerColor(double y_pos)
        {
            byte level = (byte)Math.Round(Math.Abs(ConfigParameters.SENSOR_TO_GROUND_DISTANCE - y_pos) / ConfigParameters.CONTAINER_HEIGHT);

            switch(level)
            {
                case 0:
                    return Colors.Green;
                case 1:
                    return Colors.SteelBlue;
                case 2:
                    return Colors.Tomato;
                case 3:
                    return Colors.PaleGoldenrod;
                case 4:
                    return Colors.Yellow;
                case 5:
                    return Colors.Red;
            }

            return Colors.White;
        }
    }
}
