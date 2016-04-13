using TIS_3dAntiCollision.Core;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System;

namespace TIS_3dAntiCollision.Model.DAO
{
    class Profile
    {
        private SingleLine vertical_base_line = new SingleLine(ConfigParameters.DEFAULT_FIRST_CONTAINER_CELL_POSITION_X,
                                                                ConfigParameters.PROFILING_VERTICAL_NUM_POINT_LIMIT);

        public SingleLine VerticalBaseLine
        {
            get { return vertical_base_line; }
        }

        /// <summary>
        /// All container data will be internally generated
        /// </summary>
        private List<Container> containers = new List<Container>();

        internal List<Container> Containers
        {
            get { return containers; }
        }

        /// <summary>
        /// Contain only 3 stacks:
        /// 0 - left
        /// 1 - middle
        /// 2 - right
        /// </summary>
        private List<Stack> stacks = new List<Stack>();

        public Profile()
        {
            // TODO: Complete member initialization
        }

        public Profile(SingleLine _v_base_line, List<Stack> _stacks)
        {
            this.vertical_base_line = _v_base_line;
            this.stacks = _stacks;
            generateContainers();
        }

        public Profile(Profile _p)
        {
            this.vertical_base_line = _p.VerticalBaseLine;
            this.stacks = _p.stacks;
            generateContainers();
        }

        /// <summary>
        /// Generate container data from stack profile
        /// </summary>
        private void generateContainers()
        {
            // clear old container data
            containers.Clear();

            if (stacks.Count > 0)
            {
                double column_width = ConfigParameters.CONTAINER_WIDTH + ConfigParameters.DEFAULT_SPACE_BETWEEN_CONTAINER;
                // generate the left stack containers
                Stack left_stack = stacks[0];
                for (int i = 0; i < left_stack.Columns.Count; i++)
                {
                    for (int j = 0; j < left_stack.Columns[i].Quantity; j++)
                    {
                        double z_pos = ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2 + ConfigParameters.DEFAULT_SPACE_BETWEEN_STACK
                                        + ConfigParameters.LEFT_STACK_CONTAINER_LENGTH;

                        // Update the top container of each column
                        if (j == left_stack.Columns[i].Quantity - 1)
                            z_pos = left_stack.Columns[i].ZPos;

                        containers.Add(new Container(new Point3D(i * column_width,
                                                ConfigParameters.SENSOR_TO_GROUND_DISTANCE - j * ConfigParameters.CONTAINER_HEIGHT,
                                                z_pos),
                                            ConfigParameters.LEFT_STACK_CONTAINER_LENGTH,
                                            left_stack.Columns[i].Score));
                    }
                }

                // generate the middle stack containers
                Stack middle_stack = stacks[1];
                for (int i = 0; i < middle_stack.Columns.Count; i++)
                    for (int j = 0; j < middle_stack.Columns[i].Quantity; j++)
                    {
                        double z_pos = ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2;

                        if (j == middle_stack.Columns[i].Quantity - 1)
                            z_pos = middle_stack.Columns[i].ZPos;

                        containers.Add(new Container(new Point3D(i * column_width,
                                                ConfigParameters.SENSOR_TO_GROUND_DISTANCE - j * ConfigParameters.CONTAINER_HEIGHT,
                                                z_pos),
                                            ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH,
                                            middle_stack.Columns[i].Score));
                    }

                // generate the right stack containers
                Stack right_stack = stacks[2];
                for (int i = 0; i < right_stack.Columns.Count; i++)
                    for (int j = 0; j < right_stack.Columns[i].Quantity; j++)
                    {
                        double z_pos = -ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2 - ConfigParameters.DEFAULT_SPACE_BETWEEN_STACK;

                        if (j == right_stack.Columns[i].Quantity - 1)
                            z_pos = right_stack.Columns[i].ZPos;

                        containers.Add(new Container(new Point3D(i * column_width,
                                                ConfigParameters.SENSOR_TO_GROUND_DISTANCE - j * ConfigParameters.CONTAINER_HEIGHT, 
                                                z_pos),
                                                ConfigParameters.RIGHT_STACK_CONTAINER_LENGTH,
                                            right_stack.Columns[i].Score));
                    }
            }
        }

        /// <summary>
        /// Update new profile data
        /// </summary>
        /// <param name="_p"></param>
        public void UpdateProfile(Profile _p)
        {
            // If new profile has stronger vertical base line and out of base line deviation limit, replace all stack
            // If new profile base line is in the base line deviation limit with the old one, update all column in each stack:
            // If the column in new profile has higher score, update the column info in old profile by the new one
            if (Math.Abs(_p.VerticalBaseLine.Value - this.vertical_base_line.Value) > ConfigParameters.PROFILING_MAX_V_BASE_LINE_DEVIATION)
            {
                if (_p.vertical_base_line.Score > this.vertical_base_line.Score)
                {
                    this.vertical_base_line = _p.vertical_base_line;
                    this.stacks = _p.stacks;
                }
            }
            else
                if (stacks.Count == 0)
                    stacks = _p.stacks;
                else
                    // update all column in old profile if the column data is stronger
                    for (int i = 0; i < stacks.Count; i++)
                    {
                        int num_of_col = _p.stacks[i].Columns.Count < stacks[i].Columns.Count
                                                ? _p.stacks[i].Columns.Count
                                                : stacks[i].Columns.Count;

                        for (int j = 0; j < num_of_col; j++)
                        {
                            Column current_col = stacks[i].Columns[j];
                            Column new_col = _p.stacks[i].Columns[j];
                            if (new_col.Score > current_col.Score)
                            {
                                stacks[i].Columns[j].Quantity = new_col.Quantity;
                                stacks[i].Columns[j].Score = new_col.Score;
                                stacks[i].Columns[j].ZPos = new_col.ZPos;
                            }
                        }
                    }

            generateContainers();
        }

        public Stack GetMiddleContainerMap()
        {
            List<Column> cols = new List<Column>();

            if (stacks.Count != 0)
            {
                // copy columns of the middle stack
                cols = stacks[1].Columns;
                // check left and right stack to find the adjacent bay collision
                for (int i = 0; i < cols.Count; i++)
                {
                    // compare to the left stack columns
                    double left_stack_start_z = ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2
                                                + ConfigParameters.DEFAULT_SPACE_BETWEEN_STACK
                                                + ConfigParameters.LEFT_STACK_CONTAINER_LENGTH;
                    double right_stack_start_z = - ConfigParameters.MIDDLE_STACK_CONTAINER_LENGTH / 2
                                                - ConfigParameters.DEFAULT_SPACE_BETWEEN_STACK;
                    // if the top container of left stack column pass the space between stacks
                    // and higher than the middle
                    if (left_stack_start_z - stacks[0].Columns[i].ZPos > ConfigParameters.DEFAULT_SPACE_BETWEEN_STACK
                        && stacks[0].Columns[i].Quantity > cols[i].Quantity)
                        cols[i] = stacks[0].Columns[i];
                    // and right stack too
                    if (stacks[2].Columns[i].ZPos - right_stack_start_z > ConfigParameters.DEFAULT_SPACE_BETWEEN_STACK
                        && stacks[2].Columns[i].Quantity > cols[i].Quantity)
                        cols[i] = stacks[2].Columns[i];
                }
            }
            return new Stack(cols);
        }
    }
}
