using System.Windows.Threading;
using TIS_3dAntiCollision.Core;
using System;
using System.Collections.Generic;
using TIS_3dAntiCollision.Services;

namespace TIS_3dAntiCollision.Business
{
    public sealed class MiniMotorManager
    {
        static readonly MiniMotorManager m3 = new MiniMotorManager();

        private static DispatcherTimer timer;

        private static List<SingleScanData> scan_data_list = new List<SingleScanData>();

        private static string file_name = "default_name.txt";

        private static double realTimeAngle = 0;

        static MiniMotorManager()
        {
        }

        MiniMotorManager()
        {
        }

        public static MiniMotorManager GetInstance()
        {
            return m3;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //double step_angle = ConfigParameters.SCAN_3D_ANGLE_RANGE
            //    / ConfigParameters.SINGLE_SWIVEL_TIME
            //    * ConfigParameters.SCAN_TIMER_INTERVAL;

            double step_angle = ConfigParameters.SCAN_STEP_ANGLE;

            realTimeAngle += step_angle;

            updateAngle(realTimeAngle);

            //Logger.Log(realTimeAngle.ToString());

            // trigger motor for next scan
            //PlcManager.GetInstance.TriggerMiniMotor();
        }

        public void Trigger()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(ConfigParameters.SCAN_TIMER_INTERVAL);
            timer.Tick += new EventHandler(timer_Tick);

            file_name = "scan_data_" + ((int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString() + ".txt";

            // reset real time angle
            realTimeAngle = ConfigParameters.START_3D_ANGLE;
            scan_data_list.Clear();

            // trigger motor and start timer
            PlcManager.GetInstance.TriggerMiniMotor();
            timer.Start();

            Logger.Log("Start 3D scan.", LogType.Info);
        }

        private static int scan_count = 0;

        private void updateAngle(double real_time_angle)
        {
            // stop at the last scan
            if (Math.Abs(real_time_angle - (ConfigParameters.START_3D_ANGLE + ConfigParameters.SCAN_3D_ANGLE_RANGE)) > ConfigParameters.SCAN_STEP_ANGLE)
            {
                //if (real_time_angle >= ConfigParameters.START_3D_ANGLE + scan_count * ConfigParameters.SCAN_STEP_ANGLE)
                {
                    // scan
                    if (SensorManger.GetInstance.IsConnect)
                    {
                        SensorManger.GetInstance.Scan();
                        SingleScanData single_scan_data = new SingleScanData();
                        single_scan_data.ScanData = SensorManger.GetInstance.RoughtData;

                        // add offset
                        //for (int i = 0; i < single_scan_data.ScanData.Length; i++)
                        //    single_scan_data.ScanData[i] = Math.Sqrt(ConfigParameters.OFFSET_SCAN_SETUP * ConfigParameters.OFFSET_SCAN_SETUP 
                        //        + single_scan_data.ScanData[i] * single_scan_data.ScanData[i]);

                        // add offset to angle
                        //single_scan_data.PlaneAngle = real_time_angle + Math.Asin(ConfigParameters.OFFSET_SCAN_SETUP / single_scan_data.ScanData[135]) / Math.PI * 180;

                        single_scan_data.PlaneAngle = real_time_angle;

                        single_scan_data.XPos = PlcManager.GetInstance.OnlineDataBlock.X_post;

                        scan_data_list.Add(single_scan_data);
                    }

                    scan_count++;
                }
            }
            else
            {
                Logger.Log("Scan 3D finished.", LogType.Info);

                // stop scan
                timer.Stop();
                saveScanData();
            }
        }

        private static void saveScanData()
        {
            string scan_data_str = ScanDataEncoder.Encode(scan_data_list);
            DataStorageManager.SaveScanData(file_name, scan_data_str);
        }
    }
}
