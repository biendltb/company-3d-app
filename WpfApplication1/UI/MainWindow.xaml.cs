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
using TIS_3dAntiCollision.Business.Profiling;
using TIS_3dAntiCollision.Business.AntiCollision;

namespace TIS_3dAntiCollision.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Button plc_btn = WindowContentManager.GetDefaultBtn();
        Button sensor_btn = WindowContentManager.GetDefaultBtn();
        Button scan_btn = WindowContentManager.GetDefaultBtn();

        // the timer only run when PLC connected
        DispatcherTimer m_plc_timer = new DispatcherTimer();

        // the service timer, always run
        DispatcherTimer service_timer = new DispatcherTimer();

        // the timer for calculate the scan angle
        DispatcherTimer scan_timer = new DispatcherTimer();

        // turn on auto-profiling mode
        private bool isProfileMode = false;

        public MainWindow()
        {
            InitializeComponent();

            init();
            ViewPortManager.GetInstance.AssignViewPort(m_viewPort);
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
            m_plc_timer.Interval = TimeSpan.FromMilliseconds(ConfigParameters.TIMER_INTERVAL);
            m_plc_timer.Tick += new EventHandler(m_plc_timer_Tick);

            // set service timer
            service_timer.Interval = TimeSpan.FromMilliseconds(ConfigParameters.TIMER_INTERVAL);
            service_timer.Tick += new EventHandler(service_timer_Tick);
            service_timer.Start();

            // set scan timer
            scan_timer.Interval = TimeSpan.FromMilliseconds(ConfigParameters.SCAN_TIMER_INTERVAL);
            scan_timer.Tick += new EventHandler(scan_timer_Tick);
        }

        void scan_timer_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Always keep running when program operates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void service_timer_Tick(object sender, EventArgs e)
        {
            if (Logger.LogDisplayQueue.Count > 0)
            {
                foreach (string log in Logger.LogDisplayQueue)
                    updateLog(log);

                lb_log.ScrollIntoView(lb_log.Items[lb_log.Items.Count - 1]);
            }

            // clear queue (not synchronize multi-thread)
            Logger.LogDisplayQueue.Clear();

            if (lb_log.Items.Count > ConfigParameters.NUM_LOG_ON_MEMORY_LIMIT)
                lb_log.Items.RemoveAt(0);
        }

        /// <summary>
        /// Work only when plc is connected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_plc_timer_Tick(object sender, EventArgs e)
        {
            // update struct
            PlcManager.GetInstance.ReadStruct();
            updateUIDisplay();

            // execute movement command list
            MovementController.Execute();

            // excute scan command
            //SinglePlaneScanController.Excute();

            // execute scan 3d controller
            if (PlcManager.GetInstance.IsConnect && SensorManger.GetInstance.IsConnect && isProfileMode)
                Scan3dController.GetInstance.Trigger();

            // check anti-collision
            AntiCollision.GetInstance.CheckCollision();
            //Logger.Log("Collision status: " + status);
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
            //(new Scan()).Show();
            //Scan3dController.GetInstance.Trigger();
            if (!isProfileMode)
            {
                WindowContentManager.ActiveScanBtn(scan_btn);
                isProfileMode = true;
            }
            else
            {
                WindowContentManager.DeactiveScanBtn(scan_btn);
                isProfileMode = false;
            }
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
            tb_y_pos.Text = Math.Round(PlcManager.GetInstance.OnlineDataBlock.Y_post, 2).ToString();

            tb_trolley_speed.Text = Math.Round(PlcManager.GetInstance.RealTrolleySpeed, 2).ToString();
            tb_hoist_speed.Text = Math.Round(PlcManager.GetInstance.RealHoistSpeed, 2).ToString();

            // update spreader display position
            ViewPortManager.GetInstance.DisplaySpreaderHoldingContainer(PlcManager.GetInstance.SpreaderPosition, true);

            // update light indicator
            string led_red_key = "led_red";
            string led_orange_key = "led_orange";
            string led_green_key = "led_green";
            // trolley forward indicator
            if (PlcManager.GetInstance.OnlineDataBlock.T_Forward_Stop)
                trolley_forward_led.Source = (ImageSource)Resources[led_red_key];
            else
                if (PlcManager.GetInstance.OnlineDataBlock.T_Forward_Slow)
                    trolley_forward_led.Source = (ImageSource)Resources[led_orange_key];
                else
                    trolley_forward_led.Source = (ImageSource)Resources[led_green_key];

            // trolley revert indicator
            if (PlcManager.GetInstance.OnlineDataBlock.T_Reverse_Stop)
                trolley_revert_led.Source = (ImageSource)Resources[led_red_key];
            else
                if (PlcManager.GetInstance.OnlineDataBlock.T_Reverse_Slow)
                    trolley_revert_led.Source = (ImageSource)Resources[led_orange_key];
                else
                    trolley_revert_led.Source = (ImageSource)Resources[led_green_key];

            // hoist up/down
            if (PlcManager.GetInstance.OnlineDataBlock.H_Down_Stop || PlcManager.GetInstance.OnlineDataBlock.H_Up_Stop)
                hoist_up_down_led.Source = (ImageSource)Resources[led_red_key];
            else
                if (PlcManager.GetInstance.OnlineDataBlock.H_Down_Slow || PlcManager.GetInstance.OnlineDataBlock.H_Up_Slow)
                    hoist_up_down_led.Source = (ImageSource)Resources[led_orange_key];
                else
                    hoist_up_down_led.Source = (ImageSource)Resources[led_green_key];
        }

        // add for testing purpose
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //scan_data_1457689858
            string file_path = @"../../ScanData/scan_data_1459942184.txt";
            string data_file_content = DataStorageManager.ReadScanData(file_path);
            List<SingleScanData> multi_scan_data_list = ScanDataEncoder.Decode(data_file_content);

            Point3D[] list_point = SensorOutputParser.Parse3DPoints(multi_scan_data_list.ToArray());

             //profiling the middle container profile
            ContainerStackProfiler csp = new ContainerStackProfiler(list_point.ToArray());

            Point3D[] middle_stack_profile_points = csp.GetMiddleStackProfile();
            Point3D[] left_stack_profile_points = csp.GetSideStackProfile(false);
            Point3D[] right_stack_profile_points = csp.GetSideStackProfile(true);

            ViewPortManager.GetInstance.DisplayContainerStack(middle_stack_profile_points, ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH);
            ViewPortManager.GetInstance.DisplayContainerStack(left_stack_profile_points, ConfigParameters.LEFT_STACK_CONTAINER_LENGTH);
            ViewPortManager.GetInstance.DisplayContainerStack(right_stack_profile_points, ConfigParameters.LEFT_STACK_CONTAINER_LENGTH);

            //(new DataRepresentChart(list_point.ToArray())).Show();
        }

        private void m_viewPort_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ViewPortManager.GetInstance.Camera.Position = ViewPortMouseActivity.ZoomViewPort(e.Delta);
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
                Point3D new_camera_position = new Point3D(ViewPortManager.GetInstance.Camera.Position.X,
                    ViewPortManager.GetInstance.Camera.Position.Y, 
                    ViewPortManager.GetInstance.Camera.Position.Z);
                Vector3D new_camera_look_direction = new Vector3D(ViewPortManager.GetInstance.Camera.LookDirection.X, 
                    ViewPortManager.GetInstance.Camera.LookDirection.Y, 
                    ViewPortManager.GetInstance.Camera.LookDirection.Z);
                ViewPortMouseActivity.RotateViewPort(e.GetPosition(null), ref new_camera_position, ref new_camera_look_direction);
                ViewPortManager.GetInstance.Camera.Position = new_camera_position;
                ViewPortManager.GetInstance.Camera.LookDirection = new_camera_look_direction;
            }
        }

        private void MenuMotorItem_Click(object sender, RoutedEventArgs e)
        {
            //// reset hoist
            //PlcManager.GetInstance.OnlineDataBlock.Hoist_Position_Reset = true;
            //PlcManager.GetInstance.OnlineDataBlock.Remote = true;
            //PlcManager.GetInstance.WriteStruct();
            //PlcManager.GetInstance.OnlineDataBlock.Hoist_Position_Reset = false;
            //PlcManager.GetInstance.WriteStruct();
            //PlcManager.GetInstance.OnlineDataBlock.Remote = false;
            //PlcManager.GetInstance.WriteStruct();

            // reset trolley
            //PlcManager.GetInstance.OnlineDataBlock.Trolley_Position_Reset = true;
            //PlcManager.GetInstance.OnlineDataBlock.Remote = true;
            //PlcManager.GetInstance.WriteStruct();
            //PlcManager.GetInstance.OnlineDataBlock.Trolley_Position_Reset = false;
            //PlcManager.GetInstance.WriteStruct();
            //PlcManager.GetInstance.OnlineDataBlock.Remote = false;
            //PlcManager.GetInstance.WriteStruct();


            //PlcManager.GetInstance.OnlineDataBlock.Remote = true;
            //PlcManager.GetInstance.OnlineDataBlock.H_SetPoint = 0;
            //PlcManager.GetInstance.OnlineDataBlock.H_Down_Stop = false;
            //PlcManager.GetInstance.OnlineDataBlock.H_Up_Stop = false;
            //PlcManager.GetInstance.WriteStruct();
            //PlcManager.GetInstance.OnlineDataBlock.Remote = false;
            //PlcManager.GetInstance.WriteStruct();

            //AntiCollision.GetInstance.CheckCollision();

            //PlcManager.GetInstance.OnlineDataBlock.T_Reverse_Stop = false;
            //PlcManager.GetInstance.WriteStruct();

            //reset hoist position
            //PlcManager.GetInstance.OnlineDataBlock.Hoist_Position_Reset = true;
            //PlcManager.GetInstance.WriteStruct();
            //PlcManager.GetInstance.OnlineDataBlock.Hoist_Position_Reset = false;
            //PlcManager.GetInstance.WriteStruct();

            // reset trolley position
            PlcManager.GetInstance.OnlineDataBlock.Trolley_Position_Reset = true;
            PlcManager.GetInstance.WriteStruct();
            PlcManager.GetInstance.OnlineDataBlock.Trolley_Position_Reset = false;
            PlcManager.GetInstance.WriteStruct();

        }

        private void MenuItem_Test3DScan_Click(object sender, RoutedEventArgs e)
        {
            // trigger mini motor to start scan
            //Scan3dController.GetInstance.Trigger();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            PlcManager.GetInstance.ResetNormalMode();
        }
    }
}
