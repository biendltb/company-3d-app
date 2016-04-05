using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TIS_3dAntiCollision.Core;
using TIS_3dAntiCollision.Services;

namespace TIS_3dAntiCollision.Business
{
    /// <summary>
    /// Auto trolley and scan in one plane
    /// </summary>
    static class SinglePlaneScanController
    {
        private static bool is_scan_task_trigger = false;

        private static bool is_at_start_pos_flag = false;

        private static double start_scan_x;

        private static double end_scan_x;

        private static short scan_speed;

        private static double step_length;

        private static byte scan_count;

        private static double plane_angle;

        private static string file_name;

        private static List<SingleScanData> scan_data_list = new List<SingleScanData>();

        public static void Excute()
        {
            if (is_scan_task_trigger)
            {
                // check the start position
                // NOTE: should be same condition with the scan processing
                double current_pos = PlcManager.GetInstance.OnlineDataBlock.X_post;

                // check whether the trolley move to the start position
                if (Math.Abs(current_pos - start_scan_x) <= ConfigParameters.MIN_TROLLEY_STOP_RANGE)
                    is_at_start_pos_flag = true;

                if (is_at_start_pos_flag)
                {
                    if (current_pos < end_scan_x - ConfigParameters.MIN_TROLLEY_STOP_RANGE)
                    {
                        if (Math.Abs(current_pos - (scan_count * step_length + start_scan_x)) < ConfigParameters.MIN_TROLLEY_STOP_RANGE)
                        {
                            // scan
                            if (SensorManger.GetInstance.IsConnect)
                            {
                                SensorManger.GetInstance.Scan();
                                SingleScanData single_scan_data = new SingleScanData();
                                single_scan_data.ScanData = SensorManger.GetInstance.RoughtData;
                                single_scan_data.XPos = current_pos;
                                single_scan_data.PlaneAngle = plane_angle;

                                scan_data_list.Add(single_scan_data);
                            }
                            scan_count++;
                        }
                    }
                    else
                    {
                        saveScanData();
                        is_scan_task_trigger = false;
                    }
                }
            }
        }

        public static void TriggerScan(double m_start_scan_x, 
                                            double m_end_scan_x, 
                                            short m_scan_speed,
                                            double m_step_length,
                                            double m_scan_angle,
                                            string m_file_name
                                            )
        {
            resetScanParams();

            // update params
            start_scan_x = m_start_scan_x;
            end_scan_x = m_end_scan_x;
            scan_speed = m_scan_speed;
            step_length = m_step_length;
            plane_angle = m_scan_angle;
            file_name = m_file_name;

            // set move plan
            MovementController.AddMove(start_scan_x);
            MovementController.AddMove(end_scan_x, scan_speed);

            // trigger scan
            is_scan_task_trigger = true;
            is_at_start_pos_flag = false;
        }

        private static void resetScanParams()
        {
            start_scan_x = ConfigParameters.MIN_X_RANGE;
            end_scan_x = ConfigParameters.MAX_X_RANGE;
            scan_speed = ConfigParameters.NORMAL_SPEED;
            step_length = ConfigParameters.DEFAULT_STEP_LENGTH;
            scan_count = 0;
            scan_data_list = new List<SingleScanData>();
            plane_angle = 90;
            file_name = "";
        }

        private static void saveScanData()
        {
            string scan_data_str = ScanDataEncoder.Encode(scan_data_list);
            DataStorageManager.SaveScanData(file_name, scan_data_str);
        }
    }
}
