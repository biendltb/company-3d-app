using System.Collections.Generic;
using TIS_3dAntiCollision.Core;
using System.Windows.Media.Media3D;
using TIS_3dAntiCollision.Services;
using System;
using System.Windows;
using TIS_3dAntiCollision.Model.DAO;

namespace TIS_3dAntiCollision.Business.Profiling
{
    public sealed class ProfileController
    {
        static readonly ProfileController pc = new ProfileController();

        private Profile profile = new Profile();

        internal Profile Profile
        {
            get { return profile; }
        }

        public bool IsNewProfileUpdate = false;

        private SingleLine vertical_base_line = new SingleLine(ConfigParameters.DEFAULT_FIRST_CONTAINER_CELL_POSITION_X, //50);
                                                                ConfigParameters.PROFILING_VERTICAL_NUM_POINT_LIMIT);

        static ProfileController() {}
        ProfileController(){}

        public static ProfileController GetInstance
        {
            get { return pc; }
        }

        public void UpdateProfile(SingleScanData[] scan_data)
        {
            // parse scan data to 3d points
            Point3D[] points = SensorOutputParser.Parse3DPoints(scan_data);
            // crop data
            points = cropData(points);

            Profile new_profile = getProfile(points);

            this.profile.UpdateProfile(new_profile);

            // mark that there is an available profile update
            IsNewProfileUpdate = true;
        }

        private Profile getProfile(Point3D[] points)
        {
            //collect profile
            Stack middle_stack = getMiddleStackProfile(points);
            Stack left_stack = getSideStackProfile(points, middle_stack, false);
            Stack right_stack = getSideStackProfile(points, middle_stack, true);

            List<Stack> stacks = new List<Stack>();
            stacks.Add(left_stack);
            stacks.Add(middle_stack);
            stacks.Add(right_stack);

            Profile m_profile = new Profile(vertical_base_line, stacks);

            return m_profile;
        }

        private Stack getMiddleStackProfile(Point3D[] points)
        {
            // Idea:
            // 1) Collect all point inside the middle section of middle container stack
            // 2) Get the strongest vertical line and set it as vertical base line if it meets the criterion
            // 3) Devide middle stack points according to x value of every columns calculated base on 
            // the vertical base line and container column width
            // 4) Initial empty columns and add to list column
            // 5) Get the number of container in each column and the score of column
            //    Update in list empty columns

            List<Column> middle_columns = new List<Column>();
            List<Point3D> middle_stack_points = new List<Point3D>();
            List<Point3D> vertical_points = new List<Point3D>();
            List<Point3D> horizontal_points = new List<Point3D>();

            // filter points in middle z range of middle container stack
            foreach (Point3D point in points)
                if (Math.Abs(point.Z) <= ConfigParameters.MIDDLE_STACK_SECTION_LENGTH_Z - ConfigParameters.Z_OFFSET)
                    middle_stack_points.Add(point);

            // collect vertical and horizontal points
            foreach (Point3D point in middle_stack_points)
                if (!isPointOnContainerLevel(point))
                    vertical_points.Add(point);
                else
                    horizontal_points.Add(point);

            // collect the x array and y array to pass to get lines
            double[] x_arr = new double[vertical_points.Count];
            double[] y_arr = new double[vertical_points.Count];

            for (int i = 0; i < vertical_points.Count; i++)
            {
                x_arr[i] = vertical_points[i].X;
                y_arr[i] = vertical_points[i].Y;
            }

            // get the strongest vertical line
            SingleLine[] v_lines = getLines(x_arr, y_arr, ConfigParameters.PROFILING_VERTICAL_LINE_THICKNESS,
                                    ConfigParameters.PROFILING_VERTICAL_NUM_POINT_LIMIT, ConfigParameters.PROFILING_MERGE_LINE_DISTANCE);
            double container_column_width = ConfigParameters.CONTAINER_WIDTH + ConfigParameters.DEFAULT_SPACE_BETWEEN_CONTAINER;

            // get the top line and update the vertical base line
            if (v_lines.Length > 0)
            {
                SingleLine top_line = v_lines[0];
                if (top_line.Score > vertical_base_line.Score)
                {
                    double new_base_line_value = top_line.Value - (int)(top_line.Value / container_column_width) * container_column_width;
                    vertical_base_line = new SingleLine(new_base_line_value, top_line.Score);
                    Logger.Log("New vertical base line: " + new_base_line_value + " with socre: " + top_line.Score);
                }
            }

            double start_column_x = vertical_base_line.Value;

            // get the height of every column in turn
            while (start_column_x + container_column_width < ConfigParameters.MAX_X_RANGE)
            {
                // collect all points within column range
                List<double> col_points_x = new List<double>();
                List<double> col_points_y = new List<double>();

                foreach(Point3D point in horizontal_points)
                    if (point.X >= start_column_x && point.X < start_column_x + container_column_width)
                    {
                        col_points_x.Add(point.X);
                        col_points_y.Add(point.Y);
                    }
                
                SingleLine[] h_lines = getLines(col_points_y.ToArray(), col_points_x.ToArray(), ConfigParameters.PROFILING_HORIZONTAL_LINE_THICKNESS,
                    ConfigParameters.PROFILING_HORIZONTAL_NUM_POINT_LIMIT, ConfigParameters.PROFILING_MERGE_LINE_DISTANCE);
                
                // set deufault column height is 0
                SingleLine col_highest_line = new SingleLine(ConfigParameters.SENSOR_TO_GROUND_DISTANCE, 0);

                // TODO: check the minimum threshold of score for line in profile by change the zero score above
                if (h_lines.Length > 0)
                    col_highest_line = h_lines[0];

                int quantity = (int)Math.Round((ConfigParameters.SENSOR_TO_GROUND_DISTANCE - col_highest_line.Value) / ConfigParameters.CONTAINER_HEIGHT);

                middle_columns.Add(new Column(quantity, ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2, col_highest_line.Score));

                // move to next column
                start_column_x += container_column_width;
            }

            return new Stack(middle_columns);
        }

        private Stack getSideStackProfile(Point3D[] _points, Stack _middle_stack, bool _isRight)
        {
            // Idea:
            // 1) Collect all points in overlap range in middle container stack
            // 2) Divide them to many sections based on vertical based line of middle container stack
            // 3) Collect all the highest lines from those sections
            // 4) Compare to middle stack list column height to detect the overlap container
            // 5) Save overlap container information

            List<Column> overlap_cols = new List<Column>();
            List<Column> side_stack_cols = new List<Column>();
            List<Point3D> side_stack_points = new List<Point3D>();
            List<Point3D> overlap_range_points = new List<Point3D>();

            // filter to get all point in the overlap range
            if (_isRight)
            {
                foreach (Point3D point in _points)
                    if (point.Z < -ConfigParameters.MIDDLE_STACK_SECTION_LENGTH_Z && point.Z > -ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2 - ConfigParameters.Z_OFFSET)
                        overlap_range_points.Add(point);
            }
            else
            {
                foreach (Point3D point in _points)
                    if (point.Z > ConfigParameters.MIDDLE_STACK_SECTION_LENGTH_Z && point.Z < ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2 - ConfigParameters.Z_OFFSET)
                        overlap_range_points.Add(point);
            }

            double container_column_width = ConfigParameters.CONTAINER_WIDTH + ConfigParameters.DEFAULT_SPACE_BETWEEN_CONTAINER;

            //(new TIS_3dAntiCollision.UI.DataRepresentChart(overlap_range_points.ToArray())).Show();

            for (int i = 0; i < _middle_stack.Columns.Count; i++)
            {
                // find the highest line from points
                // get the point in range
                List<double> list_x_tmp = new List<double>();
                List<double> list_y_tmp = new List<double>();
                List<double> list_z_tmp = new List<double>();

                // collect all points within column range
                foreach (Point3D point in overlap_range_points)
                    if (point.X >= i * container_column_width && point.X < (i + 1) * container_column_width)
                    {
                        list_x_tmp.Add(point.X);
                        list_y_tmp.Add(point.Y);
                        list_z_tmp.Add(point.Z);
                    }

                // find the highest horizontal line -> exchange x & y
                SingleLine[] lines = getLines(list_y_tmp.ToArray(), list_x_tmp.ToArray(),
                    ConfigParameters.PROFILING_HORIZONTAL_LINE_THICKNESS, ConfigParameters.PROFILING_HORIZONTAL_NUM_POINT_LIMIT,
                    ConfigParameters.PROFILING_MERGE_LINE_DISTANCE);

                SingleLine highest_line = new SingleLine(ConfigParameters.SENSOR_TO_GROUND_DISTANCE, 0);

                // find the highest
                // but score of the highest must be higher than haft of score of the boldest line
                foreach (SingleLine l in lines)
                    // TODO: Check the strong of line in here
                    if (l.Value < highest_line.Value && l.Score > lines[0].Score / 2 && l.Score != 0)
                        highest_line = new SingleLine(l.Value, l.Score);

                // add empty column to list overlap column
                overlap_cols.Add(new Column());

                // compare to middle stack profile
                if (Math.Abs(highest_line.Value - (ConfigParameters.SENSOR_TO_GROUND_DISTANCE - _middle_stack.Columns[i].Quantity * ConfigParameters.CONTAINER_HEIGHT)) 
                    > ConfigParameters.CONTAINER_HEIGHT / 3)
                // detect and collect the overlap container
                {
                    double sum_z_tmp = 0;
                    int z_count = 0;
                    // collect all point in highest line to calculate the averate of z
                    for (int j = 0; j < list_x_tmp.Count; j++)
                        if (Math.Abs(list_y_tmp[j] - highest_line.Value) <= ConfigParameters.PROFILING_MERGE_LINE_DISTANCE / 2)
                        {
                            sum_z_tmp += list_z_tmp[j];
                            z_count++;
                        }
                    if (z_count != 0)
                        overlap_cols[i] = new Column((int)Math.Round((ConfigParameters.SENSOR_TO_GROUND_DISTANCE - highest_line.Value) / ConfigParameters.CONTAINER_HEIGHT),
                                                        _isRight ? sum_z_tmp / z_count
                                                        : sum_z_tmp / z_count + ConfigParameters.LEFT_STACK_CONTAINER_LENGTH, 
                                                        highest_line.Score);
                }
            }

            //--- SIDE STACK PROFILING

            double start_side_stack_z = ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2 + ConfigParameters.DEFAULT_SPACE_BETWEEN_STACK;

            // collect the point of side container stack based on Z
            if (!_isRight)
            {
                foreach (Point3D point in _points)
                    if (point.Z >= start_side_stack_z && point.Z <= start_side_stack_z + ConfigParameters.RIGHT_STACK_CONTAINER_LENGTH)
                        side_stack_points.Add(point);
            }
            else
            {
                foreach (Point3D point in _points)
                    if (point.Z <= -start_side_stack_z && point.Z >= -(start_side_stack_z + ConfigParameters.LEFT_STACK_CONTAINER_LENGTH))
                        side_stack_points.Add(point);
            }

            double start_column_x = vertical_base_line.Value;

            while (start_column_x + container_column_width < ConfigParameters.MAX_X_RANGE)
            {
                // get level of every column
                // collect the point on each column
                List<double> list_x_tmp = new List<double>();
                List<double> list_y_tmp = new List<double>();

                foreach (Point3D point in side_stack_points)
                    if (point.X >= start_column_x && point.X <= start_column_x + ConfigParameters.CONTAINER_WIDTH)
                    {
                        list_x_tmp.Add(point.X);
                        list_y_tmp.Add(point.Y);
                    }

                SingleLine[] h_lines = getLines(list_y_tmp.ToArray(), list_x_tmp.ToArray(), ConfigParameters.PROFILING_HORIZONTAL_LINE_THICKNESS,
                                            ConfigParameters.PROFILING_HORIZONTAL_NUM_POINT_LIMIT, ConfigParameters.PROFILING_MERGE_LINE_DISTANCE);

                SingleLine col_highest_line = new SingleLine(ConfigParameters.SENSOR_TO_GROUND_DISTANCE, 0);

                // TODO: Check the strong of vertical line
                if (h_lines.Length > 0)
                    col_highest_line = h_lines[0];

                int quantity = (int)Math.Round((ConfigParameters.SENSOR_TO_GROUND_DISTANCE - col_highest_line.Value) / ConfigParameters.CONTAINER_HEIGHT);

                side_stack_cols.Add(new Column(quantity, 
                                                _isRight? - ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2 
                                                            - ConfigParameters.DEFAULT_SPACE_BETWEEN_STACK
                                                        : ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2 + ConfigParameters.DEFAULT_SPACE_BETWEEN_STACK
                                                            + ConfigParameters.LEFT_STACK_CONTAINER_LENGTH,
                                                col_highest_line.Score));

                // move to next column
                start_column_x += container_column_width;
            }

            // Update overlap containers in side stack containers
            for (int i = 0; i < side_stack_cols.Count; i++)
                // if same container quantity in same slot
                if (overlap_cols[i].Quantity == side_stack_cols[i].Quantity)
                    // update the z position of the top container
                    side_stack_cols[i] = new Column(side_stack_cols[i].Quantity,
                                                    overlap_cols[i].ZPos,
                                                    overlap_cols[i].Score);


            return new Stack(side_stack_cols);
        }


        /// <summary>
        /// Check whether a point lays in any container
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool isPointOnContainerLevel(Point3D point)
        {
            double point_height_from_ground = Math.Abs(ConfigParameters.SENSOR_TO_GROUND_DISTANCE - point.Y);
            if (Math.Abs(point_height_from_ground - Math.Round(point_height_from_ground / ConfigParameters.CONTAINER_HEIGHT) 
                * ConfigParameters.CONTAINER_HEIGHT) < ConfigParameters.MAX_Y_DEVIATION)
                return true;

            return false;
        }

        /// <summary>
        /// Get all the lines which contains points exceed the threshold. It will get the vertical lines by default
        /// </summary>
        /// <param name="points"></param>
        /// <param name="line_thickness"></param>
        /// <param name="num_point_limit"></param>
        /// <returns>Return the strongest line value with its score</returns>
        private SingleLine[] getLines(double[] x_arr, double[] y_arr, double line_thickness, int num_point_limit, double merge_distance)
        {
            SingleLine[] line_freq_arr = new SingleLine[0];

            List<double> list_line = new List<double>();
            List<int> list_frequency = new List<int>();

            double[] line_arr;
            int[] frequency_arr;

            // sort array according to x
            Array.Sort(x_arr, y_arr);

            // round point value based on the thickness and count how many point on the line
            list_line.Add(0);
            list_frequency.Add(0);
            for (int i = 0; i < x_arr.Length; i++)
            {
                double rounded_num = Math.Round(x_arr[i] / line_thickness) * line_thickness;
                if (list_line[list_line.Count - 1] == rounded_num)
                    list_frequency[list_frequency.Count - 1]++;
                else
                {
                    list_line.Add(rounded_num);
                    list_frequency.Add(1);
                }
            }

            // filter the line that not meet the limited threshold
            for (int i = 0; i < list_frequency.Count; i++)
                if (list_frequency[i] < num_point_limit)
                {
                    list_frequency.RemoveAt(i);
                    list_line.RemoveAt(i);
                    i--;
                }

            // collecting all line that meet the threshold
            // clustering line based on width or height params
            // merging every line with its nearby lines
            if (list_line.Count > 0)
            {
                line_arr = list_line.ToArray();
                frequency_arr = list_frequency.ToArray();

                // MERGE LINE
                Array.Sort(line_arr, frequency_arr);
                list_line.Clear();
                list_frequency.Clear();
                // init first value
                list_line.Add(line_arr[0]);
                list_frequency.Add(frequency_arr[0]);

                for (int i = 1; i < line_arr.Length; i++)
                    if (line_arr[i] - list_line[list_line.Count - 1] <= merge_distance)
                    {
                        list_line[list_line.Count - 1] = (list_line[list_line.Count - 1] * list_frequency[list_frequency.Count - 1] + line_arr[i] * frequency_arr[i]) /
                            (list_frequency[list_frequency.Count - 1] + frequency_arr[i]);
                        list_frequency[list_frequency.Count - 1] += frequency_arr[i];
                    }
                    else
                    {
                        list_line.Add(line_arr[i]);
                        list_frequency.Add(frequency_arr[i]);
                    }

                line_arr = list_line.ToArray();
                frequency_arr = list_frequency.ToArray();

                // sort by frequency DES
                Array.Sort(frequency_arr, line_arr);
                Array.Reverse(frequency_arr);
                Array.Reverse(line_arr);

                line_freq_arr = new SingleLine[line_arr.Length];

                for (int i = 0; i < line_arr.Length; i++)
                    line_freq_arr[i] = new SingleLine(line_arr[i], frequency_arr[i]);
            }

            return line_freq_arr;
        }

        private Point3D[] cropData(Point3D[] points_3d)
        {
            List<Point3D> result = new List<Point3D>();

            for (int i = 0; i < points_3d.Length; i++)
                if (points_3d[i].X >= ConfigParameters.MIN_X_RANGE && points_3d[i].X <= ConfigParameters.MAX_X_RANGE // valid x range
                    && points_3d[i].Y >= ConfigParameters.MIN_Y_RANGE && points_3d[i].Y <= ConfigParameters.MAX_Y_RANGE) // valide y range
                    result.Add(points_3d[i]);

            return result.ToArray();
        }
    }
}
