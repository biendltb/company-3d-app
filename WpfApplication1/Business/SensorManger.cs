using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TIS_3dAntiCollision.Core;
using TIS_3dAntiCollision.Services;

namespace TIS_3dAntiCollision.Business
{
    public sealed class SensorManger
    {
        static readonly SensorManger sm = new SensorManger();

        private SensorConnection sc = new SensorConnection();

        private bool isConnect = false;

        public double[] RoughtData;

        public bool IsConnect
        {
            get { return isConnect; }
        }

        // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
        static SensorManger()
        {
        }

        SensorManger()
        {
        }

        public static SensorManger GetInstance
        {
            get {return sm;}
        }

        public bool Connect()
        {
            isConnect = sc.Connect(ConfigFileManager.ReadSensorIp(), ushort.Parse(ConfigFileManager.ReadSensorPort()));
            return isConnect;
        }

        public bool DisConnect()
        {
            isConnect = !sc.Disconnect();
            return !isConnect;
        }

        public bool Scan()
        {
            sc.WriteSensor(ConfigParameters.SCAN_CMD);
            if (isConnect)
            {
                if (sc.ReadSensor())
                {
                    RoughtData =  SensorOutputParser.ParseStream(sc.ReceivedData);
                }
                else
                    return false;
            }
            else
            {
                System.Console.WriteLine("Sensor error: No connection available.\n");
                return false;
            }

            return true;
        }
    }
}
