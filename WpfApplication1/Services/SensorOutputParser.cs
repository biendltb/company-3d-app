using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TIS_3dAntiCollision.Core;
using System.Windows.Media.Media3D;

namespace TIS_3dAntiCollision.Services
{
    static class SensorOutputParser
    {
        public static double[] ParseStream(byte[] instream)
        {
            string dataString = "";
            double startAngle;
            double angleResolution;
            int data_count;

            #region Process output data and get all parameters
            try
            {
                // convert output from byte to string
                dataString = System.Text.Encoding.ASCII.GetString(instream);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                return null;
            }

            // the sign of mark the end of output string
            char etx = (char)3;

            // end data output position
            int endPst = dataString.IndexOf(etx);

            // remove the unusable data string
            dataString = dataString.Remove(endPst);

            string tmp_ouput_data = dataString;

            // split data output and put in an array
            string[] data_output_arr = tmp_ouput_data.Split(' ');

            #endregion

            #region Get the start angle
            string start_angle_hex = data_output_arr[23];

            int start_angle_raw;

            try
            {
                // convert angle from hex to int
                start_angle_raw = Int32.Parse(start_angle_hex, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                return null;
            }

            // Normalizing angle
            startAngle = Math.Ceiling((double)(start_angle_raw / 10000.00));

            #endregion

            #region Get the angle resolution
            string angle_resolution_hex = data_output_arr[24];
            int angle_resolution_raw;

            try
            {
                angle_resolution_raw = Int32.Parse(angle_resolution_hex, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ex: " + ex.ToString());
                return null;
            }

            angleResolution = (double)(angle_resolution_raw) / 10000.00;
            #endregion

            #region Get the number of items in measured output
            string data_number_hex = data_output_arr[25];
            try
            {
                data_count = Int32.Parse(data_number_hex, System.Globalization.NumberStyles.HexNumber);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.ToString());
                return null;
            }
            #endregion

            #region Get measured output data

            int measured_distance;
            double[] measuredDataArr = new double[data_count];

            // add translate measured data to X and Y and add it to arr
            for (int i = 0; i < data_count; i++)
            {
                try
                {
                    // convert measured number from hex to int
                    measured_distance = Int32.Parse(data_output_arr[i + 26], System.Globalization.NumberStyles.HexNumber);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.ToString());
                    return null;
                }

                measuredDataArr[i] = measured_distance;
            }

            #endregion

            // check whether all config parameters match with output
            if (startAngle != Convert.ToDouble(ConfigFileManager.ReadSensorStartAngle()) || angleResolution != Convert.ToDouble(ConfigFileManager.ReadSensorAngleResolution()))
                return null;

            return measuredDataArr;
        }

        public static List<Point3D> Parse3DPoints(double x_pos, double plane_angle, double[] scan_data)
        {
            List<Point3D> list_3d_point = new List<Point3D>();

            double start_scan_angle = double.Parse(ConfigFileManager.ReadSensorStartAngle());
            double angle_resolution = double.Parse(ConfigFileManager.ReadSensorAngleResolution());

            double plane_angle_rad = plane_angle * Math.PI / 180;

            for (int i = 0; i < scan_data.Length; i++)
            {
                Point3D point = new Point3D();
                double beam_angle_rad = (start_scan_angle + i * angle_resolution) * Math.PI / 180;

                // add the offset because the incorrect sensor set up
                point.X = scan_data[i] * Math.Cos(beam_angle_rad) + x_pos;
                point.Y = scan_data[i] * Math.Sin(beam_angle_rad) * Math.Cos(plane_angle_rad);
                point.Z = scan_data[i] * Math.Sin(beam_angle_rad) * Math.Sin(plane_angle_rad);

                list_3d_point.Add(point);
            }

            return list_3d_point;
        }
    }
}
