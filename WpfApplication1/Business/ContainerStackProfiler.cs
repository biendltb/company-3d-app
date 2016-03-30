using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System;
using TIS_3dAntiCollision.Core;

namespace TIS_3dAntiCollision.Business
{
    /// <summary>
    /// Profile container stack
    /// </summary>
    class ContainerStackProfiler
    {
        private Point3D[] point_data;

        private KeyValuePair<double, int> base_container_stack_point = new KeyValuePair<double, int>(ConfigParameters.DEFAULT_FIRST_CONTAINER_CELL_POSITION_X, 
                                                                                                        ConfigParameters.DEFAULT_VERTICAL_LINE_SCORE);

        // save x position and height of each middle column
        List<KeyValuePair<double, double>> list_x_pos_height = new List<KeyValuePair<double, double>>();

        public ContainerStackProfiler(Point3D[] point_data)
        {
            this.point_data = cropData(point_data);
        }

        /// <summary>
        /// Idea:
        /// - We have: container height level, space between containers, container parametes
        /// - We need: start x position of stack (will be specific in the real), height of each column
        /// => Proposal: find the vertical line first, get all section that container is surely inside, calculate the level => display
        /// </summary>
        /// <returns>Set x, y only, haven't set z axis </returns>
        public Point3D[] GetMiddleStackProfile()
        {
            List<Point3D> list_middle_container_position = new List<Point3D>();
            List<Point3D> list_middle_container_stack_point = new List<Point3D>();
            List<Point3D> list_vertical_line_point = new List<Point3D>();
            List<Point3D> list_horizontal_line_point = new List<Point3D>();

            double[] x_arr, y_arr;

            // filter points in middle z range of middle container stack
            foreach (Point3D point in point_data)
                if (Math.Abs(point.Z) <= ConfigParameters.MIDDLE_STACK_SECTION_LENGTH_Z)
                    list_middle_container_stack_point.Add(point);

            // collect vertical line points
            foreach (Point3D point in list_middle_container_stack_point)
                if (!isPointOnContainerLevel(point))
                    list_vertical_line_point.Add(point);
                else
                    list_horizontal_line_point.Add(point);

            //KeyValuePair<double, int> top_line = getStrongestVerticalLine(list_vertical_line_point.ToArray());
            // collect the x array and y array to pass to get lines
            x_arr = new double[list_vertical_line_point.Count];
            y_arr = new double[list_vertical_line_point.Count];

            for (int i = 0; i < list_vertical_line_point.Count; i++)
            {
                x_arr[i] = list_vertical_line_point[i].X;
                y_arr[i] = list_vertical_line_point[i].Y;
            }
            
            KeyValuePair<double, int>[] lines = getLines(x_arr, y_arr, ConfigParameters.SINGLE_SCAN_PROFILING_VERTICAL_LINE_THICKNESS,
                                                            ConfigParameters.SINGLE_SCAN_PROFILING_VERTICAL_NUM_POINT_LIMIT, ConfigParameters.MERGE_LINE_DISTANCE);

            if (lines.Length > 0)
            {
                KeyValuePair<double, int> top_line = lines[0];

                if (top_line.Key != -1)
                {
                    // find the x start of container range
                    double container_column_width = ConfigParameters.CONTAINER_WIDTH + ConfigParameters.DEFAULT_SPACE_BETWEEN_CONTAINER;
                    double start_container_range = top_line.Key - (int)(top_line.Key / container_column_width) * container_column_width;

                    // set the base container stack point
                    if (top_line.Value > base_container_stack_point.Value)
                        base_container_stack_point = new KeyValuePair<double, int>(start_container_range, top_line.Value);

                    double start_column_x = start_container_range;

                    while (start_column_x + container_column_width < ConfigParameters.MAX_X_RANGE)
                    {
                        // get level between column section
                        double avg_y = getAverageYValue(start_column_x, start_column_x + container_column_width, list_horizontal_line_point.ToArray());
                        byte level = (byte)Math.Round(Math.Abs(ConfigParameters.SENSOR_TO_GROUND_DISTANCE - avg_y) / ConfigParameters.CONTAINER_HEIGHT);
                        if (level != 0)
                        {
                            for (int i = 0; i < level; i++)
                                list_middle_container_position.Add(new Point3D(start_column_x, ConfigParameters.SENSOR_TO_GROUND_DISTANCE - i *
                                                                        ConfigParameters.CONTAINER_HEIGHT, ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2));

                            list_x_pos_height.Add(new KeyValuePair<double, double>(start_column_x, avg_y));
                        }

                        start_column_x += container_column_width;
                    }
                }
            }

            return list_middle_container_position.ToArray();
        }

        /// <summary>
        /// Idea:
        /// - Filter the data near the side of middle container stack
        /// - Detect the overlap container (if exist)
        /// - Get the side container stack profile from side data
        /// - NOTE: Middle container stack profile must be conducted first
        /// </summary>
        /// <param name="isLeft">Left stack or Right stack</param>
        /// <returns></returns>
        public Point3D[] GetSideStackProfile(bool isLeft)
        {
            List<Point3D> list_side_container_position = new List<Point3D>();

            List<Point3D> list_point_in_side_stack = new List<Point3D>();
            List<Point3D> list_point_in_container_overlap_range = new List<Point3D>();
            List<Point3D> list_overlap_container_points = new List<Point3D>();

            // save the detected obstacle in the middle container stack
            List<Point3D> list_obstacle_containers_pos = new List<Point3D>();

            // filter to get all point in the overlap range
            if (!isLeft)
            {
                foreach (Point3D point in point_data)
                    if (point.Z > ConfigParameters.MIDDLE_STACK_SECTION_LENGTH_Z && point.Z < ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2)
                        list_point_in_container_overlap_range.Add(point);
            }
            else
            {
                foreach (Point3D point in point_data)
                    if (point.Z < - ConfigParameters.MIDDLE_STACK_SECTION_LENGTH_Z && point.Z > - ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2)
                        list_point_in_container_overlap_range.Add(point);
            }

            // calculate the average height of every section
            double container_column_width = ConfigParameters.CONTAINER_WIDTH + ConfigParameters.DEFAULT_SPACE_BETWEEN_CONTAINER;

            for (int i = 0; i < list_x_pos_height.Count; i++)
            {
                // find the highest line from points
                // get the point in range
                List<double> list_x_tmp = new List<double>();
                List<double> list_y_tmp = new List<double>();
                List<double> list_z_tmp = new List<double>();

                // collect all points within column range
                foreach (Point3D point in list_point_in_container_overlap_range)
                    if (point.X >= list_x_pos_height[i].Key && point.X < list_x_pos_height[i].Key + container_column_width)
                    {
                        list_x_tmp.Add(point.X);
                        list_y_tmp.Add(point.Y);
                        list_z_tmp.Add(point.Z);
                    }

                // find the highest horizontal line -> exchange x & y
                KeyValuePair<double, int>[] lines = getLines(list_y_tmp.ToArray(), list_x_tmp.ToArray(),
                    ConfigParameters.SINGLE_SCAN_PROFILING_VERTICAL_LINE_THICKNESS, ConfigParameters.SINGLE_SCAN_PROFILING_VERTICAL_NUM_POINT_LIMIT, 
                    ConfigParameters.MERGE_LINE_DISTANCE);

                double highest_stack = ConfigParameters.SENSOR_TO_GROUND_DISTANCE;

                foreach (KeyValuePair<double, int> p in lines)
                    if (p.Key < highest_stack)
                        highest_stack = p.Key;

                // compare to middle stack profile
                if (Math.Abs(highest_stack - list_x_pos_height[i].Value) > ConfigParameters.CONTAINER_HEIGHT / 3)
                    // detect the obstacle
                {
                    double sum_z_tmp = 0;
                    int z_count = 0;
                    for (int j = 0; j < list_x_tmp.Count; j++)
                        if (Math.Abs(list_y_tmp[j] - highest_stack) <= ConfigParameters.MERGE_LINE_DISTANCE / 2)
                        {
                            sum_z_tmp += list_z_tmp[j];
                            z_count++;
                        }

                    list_obstacle_containers_pos.Add(new Point3D(list_x_pos_height[i].Key,
                        ConfigParameters.SENSOR_TO_GROUND_DISTANCE -
                            (((byte)((ConfigParameters.SENSOR_TO_GROUND_DISTANCE - highest_stack) / ConfigParameters.CONTAINER_HEIGHT)) * ConfigParameters.CONTAINER_HEIGHT),
                        sum_z_tmp / z_count));

                }
            }

            // TODO: based on the base point to profiling of side container stack
            double start_side_stack_x = ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2 + ConfigParameters.DEFAULT_SPACE_BETWEEN_STACK;
            double start_column_x = base_container_stack_point.Key;
            // collect the point of side container stack based on Z
            if (!isLeft)
            {
                foreach (Point3D point in point_data)
                    if (point.Z >= start_side_stack_x && point.Z <= start_side_stack_x + ConfigParameters.RIGHT_STACK_CONTAINER_LENGTH)
                        list_point_in_side_stack.Add(point);
            }
            else
            {
                foreach (Point3D point in point_data)
                    if (point.Z <= - start_side_stack_x && point.Z >= - (start_side_stack_x + ConfigParameters.LEFT_STACK_CONTAINER_LENGTH))
                        list_point_in_side_stack.Add(point);
            }
                    

            while (start_column_x + container_column_width < ConfigParameters.MAX_X_RANGE)
            {
                // get level of every column
                // collect the point on each column
                List<double> list_x_tmp = new List<double>();
                List<double> list_y_tmp = new List<double>();

                foreach (Point3D point in list_point_in_side_stack)
                    if (point.X >= start_column_x && point.X <= start_column_x + ConfigParameters.CONTAINER_WIDTH)
                    {
                        list_x_tmp.Add(point.X);
                        list_y_tmp.Add(point.Y);
                    }

                KeyValuePair<double, int>[] lines = getLines(list_y_tmp.ToArray(), list_x_tmp.ToArray(), ConfigParameters.SINGLE_SCAN_PROFILING_VERTICAL_LINE_THICKNESS,
                                            ConfigParameters.SINGLE_SCAN_PROFILING_VERTICAL_NUM_POINT_LIMIT, ConfigParameters.MERGE_LINE_DISTANCE);

                double column_height = ConfigParameters.SENSOR_TO_GROUND_DISTANCE;
                if (lines.Length > 0)
                    column_height = lines[0].Key;

                byte level = (byte)Math.Round((ConfigParameters.SENSOR_TO_GROUND_DISTANCE - column_height) / ConfigParameters.CONTAINER_HEIGHT);

                // add point to list
                for (int i = 0; i < level; i++)
                    list_side_container_position.Add(new Point3D(start_column_x, ConfigParameters.SENSOR_TO_GROUND_DISTANCE - (i * ConfigParameters.CONTAINER_HEIGHT), 0));

                // move to next column
                start_column_x += container_column_width;
            }

            // update Z position of obstacle container
            if (!isLeft)
                for (int i = 0; i < list_side_container_position.Count; i++)
                {
                    list_side_container_position[i] = new Point3D(list_side_container_position[i].X, list_side_container_position[i].Y,
                                                                            -ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2 - ConfigParameters.DEFAULT_SPACE_BETWEEN_STACK);

                    foreach (Point3D obstacle_point in list_obstacle_containers_pos)
                        if (list_side_container_position[i].X == obstacle_point.X && list_side_container_position[i].Y == obstacle_point.Y)
                            list_side_container_position[i] = new Point3D(obstacle_point.X, obstacle_point.Y, obstacle_point.Z);

                }
            else
                for (int i = 0; i < list_side_container_position.Count; i++)
                {
                    list_side_container_position[i] = new Point3D(list_side_container_position[i].X, list_side_container_position[i].Y,
                                                                            ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2 + ConfigParameters.DEFAULT_SPACE_BETWEEN_STACK
                                                                            + ConfigParameters.LEFT_STACK_CONTAINER_LENGTH);

                    foreach (Point3D obstacle_point in list_obstacle_containers_pos)
                        if (list_side_container_position[i].X == obstacle_point.X && list_side_container_position[i].Y == obstacle_point.Y)
                            list_side_container_position[i] = new Point3D(obstacle_point.X, obstacle_point.Y, obstacle_point.Z + ConfigParameters.LEFT_STACK_CONTAINER_LENGTH);

                }


            return list_side_container_position.ToArray();
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

        private bool isPointOnContainerLevel(Point3D point)
        {
            double point_height_from_ground = Math.Abs(ConfigParameters.SENSOR_TO_GROUND_DISTANCE - point.Y);
            if (Math.Abs(point_height_from_ground -
               Math.Round(point_height_from_ground / ConfigParameters.CONTAINER_HEIGHT) * ConfigParameters.CONTAINER_HEIGHT) < ConfigParameters.MAX_Y_DEVIATION)
                return true;

            return false;
        }

        // check whether the point lay on container
        private bool isPointOnContainer(Point3D point, Point3D container_pos)
        {
            // the left side: same x, y lower (revert)
            if (Math.Abs(point.X - (container_pos.X + ConfigParameters.CONTAINER_WIDTH)) < ConfigParameters.MAX_X_DEVIATION &&
                container_pos.Y >= point.Y &&
                container_pos.Y - point.Y <= ConfigParameters.CONTAINER_HEIGHT)
                return true;

            // the right side
            if (Math.Abs(point.X - container_pos.X) <= ConfigParameters.MAX_X_DEVIATION &&
                container_pos.Y >= point.Y &&
                container_pos.Y - point.Y <= ConfigParameters.CONTAINER_HEIGHT)
                return true;

            // the top: same y, diff x
            if (Math.Abs((container_pos.Y - ConfigParameters.CONTAINER_HEIGHT) - point.Y) <= ConfigParameters.MAX_Y_DEVIATION &&
                container_pos.X <= point.X &&
                point.X - container_pos.X < ConfigParameters.CONTAINER_WIDTH)
                return true;

            return false;
        }

        /// <summary>
        /// Get all the lines which contains points exceed the threshold
        /// The default is get the vertical line
        /// </summary>
        /// <param name="points"></param>
        /// <param name="line_thickness"></param>
        /// <param name="num_point_limit"></param>
        /// <returns></returns>
        private KeyValuePair<double, int>[] getLines(double[] x_arr, double[] y_arr, double line_thickness, int num_point_limit, double merge_distance)
        {
            KeyValuePair<double, int>[] line_freq_arr = new KeyValuePair<double, int>[0];

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
                if (list_frequency[i] < ConfigParameters.SINGLE_SCAN_PROFILING_VERTICAL_NUM_POINT_LIMIT)
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

                line_freq_arr = new KeyValuePair<double, int>[line_arr.Length];

                for (int i = 0; i < line_arr.Length; i++)
                    line_freq_arr[i] = new KeyValuePair<double, int>(line_arr[i], frequency_arr[i]);
            }

            return line_freq_arr;
        }

        private double getAverageYValue(double from, double to, Point3D[] points)
        {
            double sum = 0;
            double count = 0;

            foreach (Point3D point in points)
                if (point.X >= from && point.X <= to)
                {
                    sum += point.Y;
                    count++;
                }

            return sum / count;
        }
    }
}
