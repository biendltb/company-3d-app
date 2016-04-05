using System.IO;
using TIS_3dAntiCollision.Core;
using System;

namespace TIS_3dAntiCollision.Services
{
    static class DataStorageManager
    {
        public static void SaveScanData(string file_name, string scan_data)
        {
            string file_path = ConfigParameters.SCAN_DATA_STORE_PATH + file_name;
            try
            {
                //File.WriteAllText(file_path, scan_data);
                File.AppendAllText(file_path, scan_data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Cannot save scan data.\n" + ex.ToString());
            }
        }

        public static string ReadScanData(string file_path)
        {
            string result = "";
            try
            {
                using (StreamReader sr = new StreamReader(file_path))
                {
                    result = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Cannot read stored data.\n" + ex.Message);
            }

            return result;
        }
    }
}
