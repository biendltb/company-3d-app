using System.Collections.Generic;
using TIS_3dAntiCollision.Core;
using System;
using System.IO;

namespace TIS_3dAntiCollision.Services
{
    /// <summary>
    /// Logger: log all system event. Every log will immediately write down to log files.
    /// </summary>
    public static class Logger
    {
        private static List<string> log_content_list = new List<string>();
        private static List<LogType> log_type_list = new List<LogType>();

        public static List<string> LogDisplayQueue = new List<string>();

        public static void Log(string log_content, LogType log_type)
        {
            string log = DateTime.Now + "\t" + log_type.ToString() + "\t" + log_content;
            log_content_list.Add(log);
            log_type_list.Add(log_type);

            // not safe for multi thread
            // TODO: synchronize for multi thread acess
            LogDisplayQueue.Add(log);

            storeLog(log);
            popLog();
        }

        public static void Log(string log_content)
        {
            Log(log_content, LogType.Info);
        }

        private static void storeLog(string log)
        {
            log += "\n";
            // save by hour
            //string log_file = DateTime.Now.ToString("dd-MM-yyyy-hh") + ".txt";
            // save by day
            string log_file = DateTime.Now.ToString("dd-MM-yyyy") + ".txt";
            File.AppendAllText(ConfigParameters.LOG_FOLDER_PATH + log_file, log);
        }

        // Remove old log item when the number of log in memory exceed the limit
        private static void popLog()
        {
            while (log_content_list.Count > ConfigParameters.NUM_LOG_ON_MEMORY_LIMIT)
            {
                log_content_list.RemoveAt(0);
                log_type_list.RemoveAt(0);
            }
        }
    }
}
