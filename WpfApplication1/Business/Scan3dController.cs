﻿using System.Windows.Threading;
using TIS_3dAntiCollision.Core;
using System;
using System.Collections.Generic;
using TIS_3dAntiCollision.Services;

namespace TIS_3dAntiCollision.Business
{
    public sealed class Scan3dController
    {
        static readonly Scan3dController m3 = new Scan3dController();

        private DispatcherTimer timer;

        private List<SingleScanData> scan_data_list = new List<SingleScanData>();

        private string file_name = "default_name.txt";

        private double realTimeAngle = 0;

        private int scan_count = 0;

        // lock the trigger while scan is in progress
        private bool scan_lock = false;

        static Scan3dController()
        {
        }

        Scan3dController()
        {
        }

        public static Scan3dController GetInstance
        {
            get { return m3; }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            double step_angle = ConfigParameters.SCAN_STEP_ANGLE;

            realTimeAngle += step_angle;

            updateAngle(realTimeAngle);
        }

        private void timer_waiting_sensor_reset_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            scan_lock = false;
        }

        public void Trigger()
        {
            if (!scan_lock)
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

                scan_lock = true;
            }
        }

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

                        single_scan_data.XPos = PlcManager.GetInstance.GetSensorPosition().X;

                        single_scan_data.YPos = PlcManager.GetInstance.GetSpreaderPosition().Y;

                        single_scan_data.PlaneAngle = real_time_angle;

                        single_scan_data.ScanData = SensorManger.GetInstance.RoughtData;

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
                updateProfile();
                saveScanData();

                // delay until sensor go back the initial position and trigger next scan
                Logger.Log("Waiting sensor reset its position.");
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(ConfigParameters.RESET_SENSOR_POS_DELAY_INTERVAL);
                timer.Tick += new EventHandler(timer_waiting_sensor_reset_Tick);

                timer.Start();
                
            }
        }

        private void saveScanData()
        {
            string scan_data_str = ScanDataEncoder.Encode(scan_data_list);
            DataStorageManager.SaveScanData(file_name, scan_data_str);
        }

        private void updateProfile()
        {
            ProfileController.GetInstance.UpdateProfile(scan_data_list.ToArray());
        }
    }
}
