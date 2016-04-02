using System;

namespace TIS_3dAntiCollision.Core
{
    public enum ContainerTypes
    {
        TwentyFeet,
        FortyFeet
    }

    public struct DBStruct
    {
        public bool Run_Bit;
        public bool Remote;
        public bool Reset;
        public bool T_Forward_Slow;
        public bool T_Revert_Slow;
        public bool T_Forward_Stop;
        public bool T_Revert_Stop;
        public bool H_Up_Slow;
        public bool H_Down_Slow;
        public bool H_Up_Stop;
        public bool H_Down_Stop;
        public bool Start_swivel;
        public double X_post;
        public double Y_post;
        public Int16 T_SetPoint;
        public Int16 H_SetPoint;
        public Int16 X_Speed;
        public Int16 Y_Speed;
    }

    public struct SingleScanData
    {
        public double[] ScanData;
        public double XPos;
        public double PlaneAngle;
    }

    public enum LogType
    {
        Info,
        Warning,
        Error
    }
}
