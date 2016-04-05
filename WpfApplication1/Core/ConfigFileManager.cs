namespace TIS_3dAntiCollision.Core
{
    static class ConfigFileManager
    {
        
        private static IniParser iniPsr = new IniParser(ConfigParameters.CONFIG_FILE_PATH);

        // sensor config parameter names
        private const string sensorSectionName = "SENSOR";
        private const string ipAddrVarName = "IPADDRESS";
        private const string portVarName = "PORT";
        private const string startAngVarName = "START_ANGLE";
        private const string stopAngVarName = "STOP_ANGLE";
        private const string angResVarName = "ANG_RESOL";

        // plc config parameter names
        private const string plcSectionName = "PLC";
        private const string rackVarName = "RACK";
        private const string slotVarName = "SLOT";

        #region Read sensor parameters
        public static string ReadSensorIp()
        {
            return iniPsr.GetSetting(sensorSectionName, ipAddrVarName);
        }

        public static string ReadSensorPort()
        {
            return iniPsr.GetSetting(sensorSectionName, portVarName);
        }

        public static string ReadSensorStartAngle()
        {
            return iniPsr.GetSetting(sensorSectionName, startAngVarName);
        }

        public static string ReadSensorStopAngle()
        {
            return iniPsr.GetSetting(sensorSectionName, stopAngVarName);
        }

        public static string ReadSensorAngleResolution()
        {
            return iniPsr.GetSetting(sensorSectionName, angResVarName);
        }
        #endregion

        #region Read plc parameters
        public static string ReadPlcIp()
        {
            return iniPsr.GetSetting(plcSectionName, ipAddrVarName);
        }

        public static string ReadPlcPort()
        {
            return iniPsr.GetSetting(plcSectionName, portVarName);
        }

        public static string ReadPlcRack()
        {
            return iniPsr.GetSetting(plcSectionName, rackVarName);
        }

        public static string ReadPlcSlot()
        {
            return iniPsr.GetSetting(plcSectionName, slotVarName);
        }

        #endregion

        #region Write sensor parameters
        public static void WriteSensorIp(string ipAdress)
        {
            iniPsr.AddSetting(sensorSectionName, ipAddrVarName, ipAdress);
        }

        public static void WriteSensorPort(string port)
        {
            iniPsr.AddSetting(sensorSectionName, portVarName, port);
        }

        public static void WriteSensorStartAngle(string startAngle)
        {
            iniPsr.AddSetting(sensorSectionName, startAngVarName, startAngle);
        }

        public static void WriteSensorStopAngle(string stopAngle)
        {
            iniPsr.AddSetting(sensorSectionName, stopAngVarName, stopAngle);
        }

        public static void WriteSensorAngleResolution(string angleResolution)
        {
            iniPsr.AddSetting(sensorSectionName, angResVarName, angleResolution);
        }

        #endregion

        #region Write PLC parameters
        public static void WritePlcIp(string ipAddress)
        {
            iniPsr.AddSetting(plcSectionName, ipAddrVarName, ipAddress);
        }

        public static void WritePlcPort(string port)
        {
            iniPsr.AddSetting(plcSectionName, portVarName, port);
        }

        public static void WritePlcRack(string rack)
        {
            iniPsr.AddSetting(plcSectionName, rackVarName, rack);
        }

        public static void WritePlcSlot(string slot)
        {
            iniPsr.AddSetting(plcSectionName, slotVarName, slot);
        }
        #endregion

        /// <summary>
        /// Save all parameters to the config file
        /// </summary>
        public static void Save()
        {
            iniPsr.SaveSettings();
        }

    }
}
