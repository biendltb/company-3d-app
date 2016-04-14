using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System;
using TIS_3dAntiCollision.Core;
using System.Collections.Generic;
using TIS_3dAntiCollision.Model.DAO;
using System.Windows;
using System.Windows.Documents;
using TIS_3dAntiCollision.Business;

namespace TIS_3dAntiCollision.Display
{
    public sealed class ViewPortManager
    {
        // declare singleton
        static readonly ViewPortManager vpm = new ViewPortManager();

        static ViewPortManager() { }
        ViewPortManager() { }

        public static ViewPortManager GetInstance
        {
            get { return vpm; }
        }

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

        // CONTAINERS
        List<int> container_indexes = new List<int>();

        private int index_of_spreader = -1;

        public void AssignViewPort(Viewport3D view_port)
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

            resetModelGroup();

            model_visual.Content = model_group;

            view_port.Children.Add(model_visual);
        }

        private void resetModelGroup()
        {
            model_group.Children.Clear();
            model_group.Children.Add(directional_light);
            model_group.Children.Add(new AmbientLight(Colors.DimGray));

            // create the base lines
            createLineNet(model_group);

            index_of_spreader = -1;
        }

        public void DisplayContainerStack(Point3D[] stack_container_position, double container_length)
        {
            // add container stack to view
            foreach (Point3D point in stack_container_position)
                model_group.Children.Add(getContainer(point, container_length, getContainerColor(point.Y)));
        }

        public void DisplayContainerStack(Container[] containers)
        {
            // remove all the old container
            //foreach (int i in container_indexes)
            //    model_group.Children.RemoveAt(i);

            //container_indexes.Clear();

            resetModelGroup();

            // add new containers and index them
            foreach (Container m_container in containers)
            {
                GeometryModel3D container_model = getContainer(m_container.Position, m_container.Length, getContainerColor(m_container.Position.Y));
                model_group.Children.Add(container_model);
                // store index
                //container_indexes.Add(model_group.Children.IndexOf(container_model));
            }

        }

        public void DisplaySpreaderHoldingContainer(Point3D spreader_position, bool isHoldingContainer)
        {
            Point3D holding_container_position = new Point3D(spreader_position.X, spreader_position.Y + ConfigParameters.CONTAINER_HEIGHT, spreader_position.Z);
            GeometryModel3D spreader_container_model = getContainer(holding_container_position, ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH, Colors.Gold);

            if (index_of_spreader != -1)
                model_group.Children.RemoveAt(index_of_spreader);

            model_group.Children.Add(spreader_container_model);
            index_of_spreader = model_group.Children.IndexOf(spreader_container_model);
        }

        public void DisplayVirtualBox(Point3D next_point, MoveRoute move_direction)
        {
            GeometryModel3D virtual_box_model = createVirtualBox(PlcManager.GetInstance.SpreaderPosition, next_point, move_direction);
            model_group.Children.Add(virtual_box_model);
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
                // add label for x
                if (i % 2 == 0)
                    model_group.Children.Add(createTextLabel3D( (i * net_resolution).ToString(), 
                        new SolidColorBrush(Colors.Yellow), 
                        true, 
                        20,
                        new Point3D(i * net_resolution, y_plane, end_z + 50), 
                        new Vector3D(0, 0, 1), 
                        new Vector3D(1, 0, 0))
                        );

                // add net
                model_group.Children.Add(getLine(new Point3D(i * net_resolution, y_plane, start_z), new Point3D(i * net_resolution, y_plane, end_z), line_thickness));
            }

            for (int i = (int)(start_z / net_resolution); i < end_z / net_resolution + 1; i++)
            {
                model_group.Children.Add(getLine(new Point3D(start_x, y_plane, i * net_resolution), new Point3D(end_x, y_plane, i * net_resolution), line_thickness));
            }



        }

        // generate model
        // container pos: bottom - right
        private GeometryModel3D getContainer(Point3D container_position, double container_length, Color container_color)
        {
            GeometryModel3D geometry_model = new GeometryModel3D();
            MeshGeometry3D triangle_mesh = new MeshGeometry3D();

            triangle_mesh.Positions.Add(container_position);
            triangle_mesh.Positions.Add(new Point3D(container_position.X, container_position.Y - ConfigParameters.CONTAINER_HEIGHT, container_position.Z));
            triangle_mesh.Positions.Add(new Point3D(container_position.X + ConfigParameters.CONTAINER_WIDTH, 
                                        container_position.Y - ConfigParameters.CONTAINER_HEIGHT, container_position.Z));
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

        /// <summary>
        /// Creates a ModelVisual3D containing a text label.
        /// </summary>
        /// <param name="text">The string</param>
        /// <param name="textColor">The color of the text.</param>
        /// <param name="bDoubleSided">Visible from both sides?</param>
        /// <param name="height">Height of the characters</param>
        /// <param name="center">The center of the label</param>
        /// <param name="over">Horizontal direction of the label</param>
        /// <param name="up">Vertical direction of the label</param>
        /// <returns>Suitable for adding to your Viewport3D</returns>
        private GeometryModel3D createTextLabel3D(
            string text,
            Brush textColor,
            bool bDoubleSided,
            double height,
            Point3D center,
            Vector3D over,
            Vector3D up)
        {
            // First we need a textblock containing the text of our label
            TextBlock tb = new TextBlock(new Run(text));
            tb.Foreground = textColor;
            tb.FontFamily = new FontFamily("Arial");

            // Now use that TextBlock as the brush for a material
            DiffuseMaterial mat = new DiffuseMaterial();
            mat.Brush = new VisualBrush(tb);

            // We just assume the characters are square
            double width = text.Length * height;

            // Since the parameter coming in was the center of the label,
            // we need to find the four corners
            // p0 is the lower left corner
            // p1 is the upper left
            // p2 is the lower right
            // p3 is the upper right
            Point3D p0 = center - width / 2 * over - height / 2 * up;
            Point3D p1 = p0 + up * 1 * height;
            Point3D p2 = p0 + over * width;
            Point3D p3 = p0 + up * 1 * height + over * width;

            // Now build the geometry for the sign.  It's just a
            // rectangle made of two triangles, on each side.

            MeshGeometry3D mg = new MeshGeometry3D();
            mg.Positions = new Point3DCollection();
            mg.Positions.Add(p0);    // 0
            mg.Positions.Add(p1);    // 1
            mg.Positions.Add(p2);    // 2
            mg.Positions.Add(p3);    // 3

            if (bDoubleSided)
            {
                mg.Positions.Add(p0);    // 4
                mg.Positions.Add(p1);    // 5
                mg.Positions.Add(p2);    // 6
                mg.Positions.Add(p3);    // 7
            }

            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(3);
            mg.TriangleIndices.Add(1);
            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(2);
            mg.TriangleIndices.Add(3);

            if (bDoubleSided)
            {
                mg.TriangleIndices.Add(4);
                mg.TriangleIndices.Add(5);
                mg.TriangleIndices.Add(7);
                mg.TriangleIndices.Add(4);
                mg.TriangleIndices.Add(7);
                mg.TriangleIndices.Add(6);
            }

            // These texture coordinates basically stretch the
            // TextBox brush to cover the full side of the label.

            mg.TextureCoordinates.Add(new Point(0, 1));
            mg.TextureCoordinates.Add(new Point(0, 0));
            mg.TextureCoordinates.Add(new Point(1, 1));
            mg.TextureCoordinates.Add(new Point(1, 0));

            if (bDoubleSided)
            {
                mg.TextureCoordinates.Add(new Point(1, 1));
                mg.TextureCoordinates.Add(new Point(1, 0));
                mg.TextureCoordinates.Add(new Point(0, 1));
                mg.TextureCoordinates.Add(new Point(0, 0));
            }

            // And that's all.  Return the result.

            GeometryModel3D model_3d = new GeometryModel3D(mg, mat); ;
            return model_3d;
        }

        private GeometryModel3D createVirtualBox(Point3D spreader_point, Point3D next_point, MoveRoute move_direction)
        {
            GeometryModel3D geometry_model = new GeometryModel3D();
            MeshGeometry3D triangle_mesh = new MeshGeometry3D();
            Point3D holding_container_point = new Point3D(spreader_point.X,
                                                spreader_point.Y + ConfigParameters.CONTAINER_HEIGHT,
                                                spreader_point.Z);

            // holding container first square points: 0 -> 3
            triangle_mesh.Positions.Add(holding_container_point);
            triangle_mesh.Positions.Add(new Point3D(holding_container_point.X,
                                                    holding_container_point.Y - ConfigParameters.CONTAINER_HEIGHT,
                                                    holding_container_point.Z));
            triangle_mesh.Positions.Add(new Point3D(holding_container_point.X + ConfigParameters.CONTAINER_WIDTH,
                                                    holding_container_point.Y - ConfigParameters.CONTAINER_HEIGHT,
                                                    holding_container_point.Z));
            triangle_mesh.Positions.Add(new Point3D(holding_container_point.X + ConfigParameters.CONTAINER_WIDTH,
                                                    holding_container_point.Y,
                                                    holding_container_point.Z));

            // holding container second square points: 4 -> 7
            triangle_mesh.Positions.Add(new Point3D(holding_container_point.X,
                                                    holding_container_point.Y,
                                                    holding_container_point.Z - ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH));
            triangle_mesh.Positions.Add(new Point3D(holding_container_point.X,
                                                    holding_container_point.Y - ConfigParameters.CONTAINER_HEIGHT,
                                                    holding_container_point.Z - ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH));
            triangle_mesh.Positions.Add(new Point3D(holding_container_point.X + ConfigParameters.CONTAINER_WIDTH,
                                                    holding_container_point.Y - ConfigParameters.CONTAINER_HEIGHT,
                                                    holding_container_point.Z - ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH));
            triangle_mesh.Positions.Add(new Point3D(holding_container_point.X + ConfigParameters.CONTAINER_WIDTH,
                                                    holding_container_point.Y,
                                                    holding_container_point.Z - ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH));

            // virtual box first square : 8 -> 11
            triangle_mesh.Positions.Add(next_point);
            triangle_mesh.Positions.Add(new Point3D(next_point.X,
                                                    next_point.Y - ConfigParameters.CONTAINER_HEIGHT,
                                                    next_point.Z));
            triangle_mesh.Positions.Add(new Point3D(next_point.X + ConfigParameters.CONTAINER_WIDTH,
                                                    next_point.Y - ConfigParameters.CONTAINER_HEIGHT,
                                                    next_point.Z));
            triangle_mesh.Positions.Add(new Point3D(next_point.X + ConfigParameters.CONTAINER_WIDTH,
                                                    next_point.Y,
                                                    next_point.Z));

            // virtual second square points : 12-15
            triangle_mesh.Positions.Add(new Point3D(next_point.X,
                                                    next_point.Y,
                                                    next_point.Z - ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH));
            triangle_mesh.Positions.Add(new Point3D(next_point.X,
                                                    next_point.Y - ConfigParameters.CONTAINER_HEIGHT,
                                                    next_point.Z - ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH));
            triangle_mesh.Positions.Add(new Point3D(next_point.X + ConfigParameters.CONTAINER_WIDTH,
                                                    next_point.Y - ConfigParameters.CONTAINER_HEIGHT,
                                                    next_point.Z - ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH));
            triangle_mesh.Positions.Add(new Point3D(next_point.X + ConfigParameters.CONTAINER_WIDTH,
                                                    next_point.Y,
                                                    next_point.Z - ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH));

            //   _______
            // 2|       |1    
            //  |       |
            // 3|_______|0

            List<int> list_indices = new List<int>();

            switch (move_direction)
            {
                case MoveRoute.DownReverse:
                    list_indices.AddRange(new int[]{9, 1, 0, 0, 8, 9,
                                                    8, 0, 3, 3, 11, 8});
                    break;
                case MoveRoute.DownForward:
                    // left side
                    list_indices.AddRange(new int[]{3, 2, 10, 10, 11, 3,
                                            0, 3, 11, 11, 8, 0});
                    break;
            }


            foreach (int indice_pos in list_indices)
                triangle_mesh.TriangleIndices.Add(indice_pos);

            geometry_model.Geometry = triangle_mesh;
            geometry_model.Material = new DiffuseMaterial(new SolidColorBrush(Colors.LimeGreen));
            return geometry_model;
        }
    }
}
