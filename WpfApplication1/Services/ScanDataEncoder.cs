using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TIS_3dAntiCollision.Core;

namespace TIS_3dAntiCollision.Services
{
    static class ScanDataEncoder
    {
        private static char multi_scan_splitter = '|';
        private static char scan_data_splitter = ' ';

        public static string Encode(List<SingleScanData> scan_data_list)
        {
            string result = "";

            foreach (SingleScanData single_scan_data in scan_data_list)
            {
                // 1st and 2nd element will contains the x pos and plane angle
                result += single_scan_data.XPos + scan_data_splitter.ToString();
                result += single_scan_data.YPos + scan_data_splitter.ToString();
                result += single_scan_data.PlaneAngle + scan_data_splitter.ToString();

                // add scan data next to
                for (int i = 0; i < single_scan_data.ScanData.Length; i++)
                    result += single_scan_data.ScanData[i] + scan_data_splitter.ToString();

                // add splitter after every single scan data
                result += multi_scan_splitter;
            }

            return result;
        }

        public static List<SingleScanData> Decode(string multi_scan_str)
        {
            List<SingleScanData> result = new List<SingleScanData>();
            // split string with spliter "|" to get each scan list
            string[] multi_scan_data_arr = multi_scan_str.Trim().Split(multi_scan_splitter);

            foreach (string single_scan_data_str in multi_scan_data_arr)
            {
                if (single_scan_data_str != "")
                {
                    SingleScanData single_scan_data = new SingleScanData();
                    List<double> scan_data_list = new List<double>();
                    string[] scan_data_arr = single_scan_data_str.Trim().Split(scan_data_splitter);

                    // get x pos and plane angle
                    single_scan_data.XPos = double.Parse(scan_data_arr[0]);
                    single_scan_data.YPos = double.Parse(scan_data_arr[1]);
                    single_scan_data.PlaneAngle = double.Parse(scan_data_arr[2]);
                    
                    // collect the scan data
                    for (int i = 3; i < scan_data_arr.Length; i++)
                        if (scan_data_arr[i] != "")
                            scan_data_list.Add(double.Parse(scan_data_arr[i]));

                    single_scan_data.ScanData = scan_data_list.ToArray();

                    result.Add(single_scan_data);
                }
            }

            return result;
        }
    }
}
