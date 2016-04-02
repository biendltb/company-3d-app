using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TIS_3dAntiCollision.Core;
using System.IO;

namespace TIS_3dAntiCollision.Business
{
    static class SingleScanEncoder
    {
        private static double[] scan_data;
        public static double[] Scan_data { get { return scan_data; } }

        private static double x_pos;
        public static double X_pos { get { return x_pos; } }

        private static double scan_angle;
        public static double Scan_angle { get { return scan_angle; } }

        public static string Encode(double[] scan_data, double x_pos, double scan_angle)
        {
            string result = "";

            result += x_pos + " ";
            result += scan_angle + " ";

            for (int i = 0; i < scan_data.Length; i++)
            {
                result += scan_data[i] + " ";
            }

            return result;
        }

        public static void Decode(string single_scan_string)
        {
            string[] data_arr;
            // extract data
            single_scan_string.Trim();

            if (single_scan_string != "")
            {
                data_arr = single_scan_string.Split(' ');

                x_pos = double.Parse(data_arr[0]);
                scan_angle = double.Parse(data_arr[1]);

                scan_data = new double[data_arr.Length - 2];

                for (int i = 2; i < data_arr.Length; i++)
                {
                    scan_data[i - 2] = double.Parse(data_arr[i]);
                }
            }
        }
    }
}
