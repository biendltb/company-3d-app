using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using TIS_3dAntiCollision.Core;
using TIS_3dAntiCollision.Display;
using TIS_3dAntiCollision.Business;
using System.Windows.Threading;
using TIS_3dAntiCollision.Services;

namespace TIS_3dAntiCollision.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewPortManager vpm;

        Button plc_btn = WindowContentManager.GetDefaultBtn();
        Button sensor_btn = WindowContentManager.GetDefaultBtn();
        Button scan_btn = WindowContentManager.GetDefaultBtn();

        // timer
        DispatcherTimer m_timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            init();

            vpm = new ViewPortManager(m_viewPort);

            //initContainerStack();
        }

        private void init()
        {
            // set the button
            WindowContentManager.ActivePlcConnectBtn(plc_btn);
            plc_btn.Click += new RoutedEventHandler(plc_btn_Click);

            WindowContentManager.DeactiveSensorConnectBtn(sensor_btn);
            sensor_btn.Click += new RoutedEventHandler(sensor_btn_Click);

            WindowContentManager.DeactiveScanBtn(scan_btn);
            scan_btn.Click += new RoutedEventHandler(scan_btn_Click);

            Button move_to_btn = WindowContentManager.GetDefaultBtn();
            WindowContentManager.SetImageMoveBtn(move_to_btn);
            move_to_btn.Click += new RoutedEventHandler(move_to_btn_Click);

            Button chart_btn = WindowContentManager.GetDefaultBtn();
            WindowContentManager.SetImageChartBtn(chart_btn);
            chart_btn.Click += new RoutedEventHandler(chart_btn_Click);


            buttonArea.Children.Add(plc_btn);
            buttonArea.Children.Add(sensor_btn);
            buttonArea.Children.Add(scan_btn);
            buttonArea.Children.Add(move_to_btn);
            buttonArea.Children.Add(chart_btn);
            
            // set the timer
            // 1 ticks = 100 nanoseconds = 100 * 10^-6 milisecond
            // 50 milisecond
            m_timer.Interval = new TimeSpan(ConfigParameters.TIMER_INTERVAL);
            m_timer.Tick += new EventHandler(m_timer_Tick);
        }

        private void m_timer_Tick(object sender, EventArgs e)
        {
            // update struct
            PlcManager.GetInstance.ReadStruct();
            updateUIDisplay();

            // execute movement command list
            MovementController.Execute();

            // excute scan command
            ScanController.Excute();
        }

        private void plc_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!PlcManager.GetInstance.IsConnect)
                if (PlcManager.GetInstance.Connect() == 0)
                {
                    WindowContentManager.DeactivePlcConnectBtn(plc_btn);
                    log("PLC is successfully connected.");
                    m_timer.Start();
                }
                else
                {
                    log("Unable to connect to PLC.");
                }
            else
            {
                PlcManager.GetInstance.Close();
                WindowContentManager.ActivePlcConnectBtn(plc_btn);
                log("PLC is disconnected.");
                m_timer.Stop();
            }
        }

        private void sensor_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!SensorManger.GetInstance.IsConnect)
                if (SensorManger.GetInstance.Connect())
                {
                    log("Sensor is successfully connected.");
                    WindowContentManager.ActiveSensorConnectBtn(sensor_btn);
                }
                else
                    log("Error: Unable to connect to sensor.");
            else
                if (SensorManger.GetInstance.DisConnect())
                {
                    WindowContentManager.DeactiveSensorConnectBtn(sensor_btn);
                    log("Sensor is disconnected.");

                }
        }

        private void scan_btn_Click(object sender, RoutedEventArgs e)
        {
            (new Scan()).Show();
        }

        void move_to_btn_Click(object sender, RoutedEventArgs e)
        {
            Window move_to_frm = new MoveTo();
            move_to_frm.Show();
        }

        void chart_btn_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text files (*.txt)|*.txt";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string file_name = dlg.FileName;
                (new DataRepresentChart(file_name)).Show();
            }
        }

        private void log(string log_cmd)
        {
            string log_line = DateTime.Now + " " + log_cmd + "\n";
            rtb_log.AppendText(log_line);
            rtb_log.ScrollToEnd();
        }

        private void updateUIDisplay()
        {
            tb_x_pos.Text = Math.Round(PlcManager.GetInstance.OnlineDataBlock.X_post, 2).ToString();
        }
        
        private void initContainerStack()
        {
            DirectionalLight DirLight1 = new DirectionalLight();
            DirLight1.Color = Colors.White;
            DirLight1.Direction = new Vector3D(1, -1, 1);

            PerspectiveCamera Camera1 = new PerspectiveCamera();
            Camera1.FarPlaneDistance = 30;
            Camera1.NearPlaneDistance = 1;
            Camera1.FieldOfView = 60;
            Camera1.Position = new Point3D(-10, 10, -10);
            Camera1.LookDirection = new Vector3D(1, -1, 1);
            Camera1.UpDirection = new Vector3D(0, 1, 0);

            Model3DGroup modelGroup = new Model3DGroup();
            modelGroup.Children.Add(drawContainer(new Point3D(0, 0, 0), Colors.Green));
            modelGroup.Children.Add(drawContainer(new Point3D(1, 0, 0), Colors.Green));
            modelGroup.Children.Add(drawContainer(new Point3D(2, 0, 0), Colors.Green));
            modelGroup.Children.Add(drawContainer(new Point3D(3, 0, 0), Colors.Green));
            modelGroup.Children.Add(drawContainer(new Point3D(0, 1, 0), Colors.Blue));
            //modelGroup.Children.Add(drawContainer(new Point3D(1, 1, 0), Colors.Blue));
            modelGroup.Children.Add(drawContainer(new Point3D(2, 1, 0), Colors.Blue));
            //modelGroup.Children.Add(drawContainer(new Point3D(3, 1, 0), Colors.Blue));
            modelGroup.Children.Add(drawContainer(new Point3D(0, 2, 0), Colors.Yellow));
            //modelGroup.Children.Add(drawContainer(new Point3D(1, 2, 0), Colors.Yellow));
            modelGroup.Children.Add(drawContainer(new Point3D(2, 2, 0), Colors.Yellow));
            //modelGroup.Children.Add(drawContainer(new Point3D(3, 2, 0), Colors.Yellow));
            modelGroup.Children.Add(drawContainer(new Point3D(0, 3, 0), Colors.Red));
            //modelGroup.Children.Add(drawContainer(new Point3D(1, 3, 0), Colors.Red));
            //modelGroup.Children.Add(drawContainer(new Point3D(2, 3, 0), Colors.Red));
            //modelGroup.Children.Add(drawContainer(new Point3D(3, 3, 0), Colors.Red));

            modelGroup.Children.Add(DirLight1);
            ModelVisual3D modelsVisual = new ModelVisual3D();
            modelsVisual.Content = modelGroup;

            m_viewPort.Camera = Camera1;
            m_viewPort.Children.Add(modelsVisual);

            // rotate
            //AxisAngleRotation3D axis = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
            //RotateTransform3D Rotate = new RotateTransform3D(axis);
            //Cube1.Transform = Rotate;
            //Cube2.Transform = Rotate;
            //Cube3.Transform = Rotate;
            //Cube4.Transform = Rotate;
            //DoubleAnimation RotAngle = new DoubleAnimation();
            //RotAngle.From = 0;
            //RotAngle.To = 360;
            //RotAngle.Duration = new Duration(TimeSpan.FromSeconds(20.0));
            //RotAngle.RepeatBehavior = RepeatBehavior.Forever;
            //NameScope.SetNameScope(m_viewPort, new NameScope());
            //m_viewPort.RegisterName("cubeaxis", axis);
            //Storyboard.SetTargetName(RotAngle, "cubeaxis");
            //Storyboard.SetTargetProperty(RotAngle, new PropertyPath(AxisAngleRotation3D.AngleProperty));
            //Storyboard RotCube = new Storyboard();
            //RotCube.Children.Add(RotAngle);
            //RotCube.Begin(m_viewPort);
        }

        private GeometryModel3D drawContainer(Point3D pos, Color color)
        {
            GeometryModel3D Cube1 = new GeometryModel3D();
            Container3dDataGenerator cdg = new Container3dDataGenerator(pos, Core.ContainerTypes.TwentyFeet);
            MeshGeometry3D cubeMesh = cdg.CreateContainer();
            Cube1.Geometry = cubeMesh;
            Cube1.Material = new DiffuseMaterial(new SolidColorBrush(color));

            return Cube1;
        }

        // add for testing purpose
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //scan_data_1457689858
            string file_path = @"../../ScanData/scan_data_side_2.txt";
            string data_file_content = DataStorageManager.ReadScanData(file_path);
            List<SingleScanData> multi_scan_data_list = ScanDataEncoder.Decode(data_file_content);
            Point3D[][] multi_scan_3d_point = new Point3D[multi_scan_data_list.Count][];

            List<Point3D> list_point = new List<Point3D>();

            for (int i = 0; i < multi_scan_data_list.Count; i++)
            {
                multi_scan_3d_point[i] = SensorOutputParser.Parse3DPoints(multi_scan_data_list[i].XPos,
                                                                            multi_scan_data_list[i].PlaneAngle,
                                                                            multi_scan_data_list[i].ScanData).ToArray();
            }

            foreach (Point3D[] point_arr in multi_scan_3d_point)
                foreach (Point3D point in point_arr)
                    list_point.Add(point);

            // profiling the middle container profile
            ContainerStackProfiler csp = new ContainerStackProfiler(list_point.ToArray());

            Point3D[] middle_stack_profile_points = csp.GetMiddleStackProfile();
            Point3D[] side_stack_profile_points = csp.GetSideStackProfile();

            vpm.DisplayContainerStack(middle_stack_profile_points, ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH);
            vpm.DisplayContainerStack(side_stack_profile_points, ConfigParameters.LEFT_STACK_CONTAINER_LENGTH);

            //(new DataRepresentChart(list_point.ToArray())).Show();
        }

        private void m_viewPort_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            vpm.Camera.Position = ViewPortMouseActivity.ZoomViewPort(e.Delta, vpm.Camera.Position);
            Console.WriteLine(e.Delta);
        }
    }
}
