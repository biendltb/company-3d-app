using System.Windows.Media;
using System.Windows.Media.Media3D;
namespace TIS_3dAntiCollision.Core
{
    class ConfigParameters
    {
        // CONTAINER & PROFILING PARAMETERS
        public const double CONTAINER_HEIGHT = 126;
        public const double CONTAINER_WIDTH = 126;
        public const double TWENTY_FEET_CONTAINER_LENGTH = 300;
        public const double FORTY_FEET_CONTAINER_LENGTH = 600;
        public const double DEFAULT_SPACE_BETWEEN_CONTAINER = 15;
        public const double SENSOR_TO_GROUND_DISTANCE = 1286;
        public const double DEFAULT_SPACE_BETWEEN_STACK = 25;

        public const double MIDDLE_STACK_CONTAINER_LENGTH = 600;
        public const double LEFT_STACK_CONTAINER_LENGTH = 300;
        public const double RIGHT_STACK_CONTAINER_LENGTH = 300;

        // Timer interval (ms)
        public const int TIMER_INTERVAL = 10;
        public const int SCAN_TIMER_INTERVAL = 625;

        // Configuration file path
        public const string CONFIG_FILE_PATH = @"..\..\dvconf.ini";
        public const string SCAN_DATA_STORE_PATH = @"..\..\ScanData\";

        // LOG
        public const string LOG_FOLDER_PATH = @"..\..\logs\";
        public const int NUM_LOG_ON_MEMORY_LIMIT = 1000;

        // PLC
        public const int DATA_BLOCK_NUMBER = 3;

        // Scan
        public const string SCAN_CMD = "sRN LMDscandata";
        public const double DEFAULT_STEP_LENGTH = 200;
        public const double SENSOR_INIT_POSITION = 80;
        public const double START_3D_ANGLE = 27.8;
        public const double SCAN_3D_ANGLE_RANGE = 119.88;
        public const double SINGLE_SWIVEL_TIME = 20813; // milisecond
        public const double SCAN_STEP_ANGLE = 3.6;
        public const double OFFSET_SCAN_SETUP = 27;

        // movement
        public const double MIN_TROLLEY_STOP_RANGE = 10;
        public const short MAX_TROLLEY_SPEED = 16384;
        public const short NORMAL_SPEED = 40;

        // range limit
        public const double MAX_X_RANGE = 1725;
        public const double MIN_X_RANGE = 0;
        public const double MIN_Y_RANGE = 0;
        public const double MAX_Y_RANGE = 1400;

        // Divide middle container to many sections
        // Middle section area for conducting the middle container stack profile
        public const double MIDDLE_STACK_SECTION_LENGTH_Z = 50; // from -50 mm to 50mm

        // 3D representation
        public ContainerTypes MIDDLE_CONTAINER_TYPE = ContainerTypes.FortyFeet;

        // Profiling params
        public const double MAX_Y_DEVIATION = 15;
        public const double MAX_X_DEVIATION = 10;
        public const int PROFILING_VERTICAL_LINE_THICKNESS = 3;
        public const int PROFILING_VERTICAL_NUM_POINT_LIMIT = 3;
        public const int PROFILING_HORIZONTAL_LINE_THICKNESS = 3;
        public const int PROFILING_HORIZONTAL_NUM_POINT_LIMIT = 5;
        public const int MERGE_LINE_DISTANCE = 10;
        public const int DEFAULT_FIRST_CONTAINER_CELL_POSITION_X = 102;

    }
}
