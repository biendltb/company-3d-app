using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TIS_3dAntiCollision.Core;
using System.Windows.Media.Media3D;
using TIS_3dAntiCollision.Business;

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

        public static Point3D[] Parse3DPoints(SingleScanData single_scan_data)
        {
            List<Point3D> list_3d_point = new List<Point3D>();

            double x_pos = single_scan_data.XPos;
            double plane_angle = single_scan_data.PlaneAngle;
            double[] scan_data = single_scan_data.ScanData;

            double start_scan_angle = double.Parse(ConfigFileManager.ReadSensorStartAngle());
            double angle_resolution = double.Parse(ConfigFileManager.ReadSensorAngleResolution());

            double plane_angle_rad = plane_angle * Math.PI / 180;

            // calculate the start and end angle
            double start_spreader_range_angle = Math.Atan((single_scan_data.XPos + ConfigParameters.SENSOR_OFFSET_X
                                                        - (single_scan_data.XPos + ConfigParameters.SPREADER_OFFSET_X + ConfigParameters.CONTAINER_WIDTH
                                                            + ConfigParameters.SPREADER_SWING_RANGE)) / single_scan_data.YPos) / Math.PI * 180 + 90;

            double end_spreader_range_angle = Math.Atan((single_scan_data.XPos + ConfigParameters.SENSOR_OFFSET_X
                                                        - (single_scan_data.XPos + ConfigParameters.SPREADER_OFFSET_X - ConfigParameters.SPREADER_SWING_RANGE)) //rate
                                                        / (single_scan_data.YPos - ConfigParameters.CONTAINER_HEIGHT)) / Math.PI * 180 + 90;

            double spreader_angle_range = (end_spreader_range_angle - start_spreader_range_angle);

            bool isRemoveSpreader = false;

            if (Math.Abs((1 / Math.Tan(plane_angle_rad)) * (single_scan_data.YPos - ConfigParameters.CONTAINER_HEIGHT))
                < ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2 + ConfigParameters.SPREADER_SWING_RANGE)
                isRemoveSpreader = true;
            
            for (int i = 0; i < scan_data.Length; i++)
            {
                Point3D point = new Point3D();
                double beam_angle_deg = start_scan_angle + i * angle_resolution;

                // skip if in spreader range
                if (beam_angle_deg >= start_spreader_range_angle && beam_angle_deg <= (start_spreader_range_angle + spreader_angle_range) && isRemoveSpreader)
                    continue;

                double beam_angle_rad = beam_angle_deg * Math.PI / 180;

                // change the angle
                //plane_angle_rad -= Math.PI;

                point.X = scan_data[i] * Math.Cos(beam_angle_rad) + x_pos;
                point.Y = scan_data[i] * Math.Sin(beam_angle_rad) * Math.Sin(plane_angle_rad);
                point.Z = scan_data[i] * Math.Sin(beam_angle_rad) * Math.Cos(plane_angle_rad);

                list_3d_point.Add(point);
            }

            return list_3d_point.ToArray();
        }

        public static Point3D[] Parse3DPoints(SingleScanData[] single_scan_data_arr)
        {
            List<Point3D> list_3d_points = new List<Point3D>();

            foreach (SingleScanData single_scan_data in single_scan_data_arr)
                list_3d_points.AddRange(Parse3DPoints(single_scan_data));

            return list_3d_points.ToArray();
        }
    }
}
