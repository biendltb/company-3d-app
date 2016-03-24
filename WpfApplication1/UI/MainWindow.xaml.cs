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

        // the timer only run when PLC connected
        DispatcherTimer m_plc_timer = new DispatcherTimer();

        // the service timer, always run
        DispatcherTimer service_timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            init();

            vpm = new ViewPortManager(m_viewPort);
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
            m_plc_timer.Interval = new TimeSpan(ConfigParameters.TIMER_INTERVAL);
            m_plc_timer.Tick += new EventHandler(m_plc_timer_Tick);

            // set service timer
            service_timer.Interval = new TimeSpan(ConfigParameters.TIMER_INTERVAL);
            service_timer.Tick += new EventHandler(service_timer_Tick);
            service_timer.Start();
        }

        void service_timer_Tick(object sender, EventArgs e)
        {
            if (Logger.LogDisplayQueue.Count > 0)
                foreach (string log in Logger.LogDisplayQueue)
                    updateLog(log);

            // clear queue (not synchronize multi-thread)
            Logger.LogDisplayQueue.Clear();

            if (lb_log.Items.Count > ConfigParameters.NUM_LOG_ON_MEMORY_LIMIT)
                lb_log.Items.RemoveAt(0);

            if (lb_log.Items.Count > 0)
                lb_log.ScrollIntoView(lb_log.Items[lb_log.Items.Count - 1]);
        }

        private void m_plc_timer_Tick(object sender, EventArgs e)
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
                    Logger.Log("PLC is successfully connected.", LogType.Info);
                    m_plc_timer.Start();
                }
                else
                {
                    Logger.Log("Unable to connect to PLC.", LogType.Error);
                }
            else
            {
                PlcManager.GetInstance.Close();
                WindowContentManager.ActivePlcConnectBtn(plc_btn);
                Logger.Log("PLC is disconnected.", LogType.Info);
                m_plc_timer.Stop();
            }
        }

        private void sensor_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!SensorManger.GetInstance.IsConnect)
                if (SensorManger.GetInstance.Connect())
                {
                    Logger.Log("Sensor is successfully connected.", LogType.Info);
                    WindowContentManager.ActiveSensorConnectBtn(sensor_btn);
                }
                else
                    Logger.Log("Error: Unable to connect to sensor.", LogType.Error);
            else
                if (SensorManger.GetInstance.DisConnect())
                {
                    WindowContentManager.DeactiveSensorConnectBtn(sensor_btn);
                    Logger.Log("Sensor is disconnected.", LogType.Info);

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

        private void updateLog(string log_cmd)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = log_cmd;
            lb_log.Items.Add(item);
        }

        private void updateUIDisplay()
        {
            tb_x_pos.Text = Math.Round(PlcManager.GetInstance.OnlineDataBlock.X_post, 2).ToString();
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
            //Console.WriteLine(e.Delta);
        }

        private void m_viewPort_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewPortMouseActivity.SetStartDragPoint(e.GetPosition(null));
        }

        private void m_viewPort_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point3D new_camera_position = new Point3D(vpm.Camera.Position.X, vpm.Camera.Position.Y, vpm.Camera.Position.Z);
                Vector3D new_camera_look_direction = new Vector3D(vpm.Camera.LookDirection.X, vpm.Camera.LookDirection.Y, vpm.Camera.LookDirection.Z);
                ViewPortMouseActivity.RotateViewPort(e.GetPosition(null), ref new_camera_position, ref new_camera_look_direction);
                vpm.Camera.Position = new_camera_position;
                vpm.Camera.LookDirection = new_camera_look_direction;
            }
        }
    }
}
