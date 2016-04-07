﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TIS_3dAntiCollision.Core;
using TIS_3dAntiCollision.Core.PlcTypes;
using System.Windows.Media.Media3D;

namespace TIS_3dAntiCollision.Business
{
    public sealed class PlcManager
    {
        static readonly PlcManager pm = new PlcManager();

        private PLC plc = new PLC(CPU_Type.S7300, 
                                    ConfigFileManager.ReadPlcIp(), 
                                    short.Parse(ConfigFileManager.ReadPlcRack()), 
                                    short.Parse(ConfigFileManager.ReadPlcSlot()));

        public DBStruct OnlineDataBlock = new DBStruct();

        private bool isConnect = false;

        public bool IsConnect
        {
            get { return isConnect; }
        }

        // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
        static PlcManager()
        {
        }

        PlcManager()
        {
        }

        public static PlcManager GetInstance
        {
            get { return pm; }
        }

        public ErrorCode Connect()
        {
            ErrorCode connect_result = plc.Open();

            if (connect_result == ErrorCode.NoError)
                isConnect = true;

            return connect_result; 
        }

        public void Close()
        {
            isConnect = false;
            plc.Close();
        }

        public void Write(string var_name, object value)
        {
            plc.Write(var_name, value);
        }

        public object Read(string var_name)
        {
            return plc.Read(var_name);
        }

        public void ReadStruct()
        {
            OnlineDataBlock = (DBStruct)plc.ReadStruct(typeof(DBStruct), ConfigParameters.DATA_BLOCK_NUMBER);
        }

        public string WriteStruct()
        {
            return plc.WriteStruct(OnlineDataBlock, ConfigParameters.DATA_BLOCK_NUMBER).ToString();
        }


        public void TriggerMiniMotor()
        {
            PlcManager.GetInstance.OnlineDataBlock.Start_swivel = true;
            //PlcManager.GetInstance.OnlineDataBlock.Remote = true;
            PlcManager.GetInstance.WriteStruct();
        }

        public Point3D GetSensorPosition()
        {
            return new Point3D(OnlineDataBlock.X_post + ConfigParameters.SENSOR_OFFSET_X,
                ConfigParameters.SENSOR_TO_GROUND_DISTANCE - OnlineDataBlock.Y_post,
                ConfigParameters.SENSOR_OFFSET_Z);
        }

        public Point3D GetSpreaderPosition()
        {
            return new Point3D(OnlineDataBlock.X_post + ConfigParameters.SPREADER_OFFSET_X,
                ConfigParameters.SENSOR_TO_GROUND_DISTANCE - OnlineDataBlock.Y_post + ConfigParameters.CONTAINER_HEIGHT,
                ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2);
        }
        
    }
}
