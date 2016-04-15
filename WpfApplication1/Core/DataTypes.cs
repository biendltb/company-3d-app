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
        public bool Trolley_Position_Reset;
        public bool Hoist_Position_Reset;
        public bool T_Forward_Slow;
        public bool T_Reverse_Slow;
        public bool T_Forward_Stop;
        public bool T_Reverse_Stop;
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
        // based x pos
        public double XPos;
        // spreader y pos 
        public double YPos;
        public double PlaneAngle;
    }

    public enum LogType
    {
        Info,
        Warning,
        Error
    }

    public struct SingleLine
    {
        public double Value;
        public int Score;

        public SingleLine(double value, int score)
        {
            this.Value = value;
            this.Score = score;
        }
    }

    public enum MoveRoute
    {
        Up = 0,
        Down,
        Reverse,
        Forward,
        UpReverse,
        UpForward,
        DownReverse,
        DownForward,
        NotMove
    };

    public enum CollisionStatus
    {
        Normal = 0,
        Slow,
        Stop
    }
}
