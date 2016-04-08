using TIS_3dAntiCollision.Core;
using System.Collections.Generic;

namespace TIS_3dAntiCollision.Business.DAO
{
    class Profile
    {
        private SingleLine vertical_base_line = new SingleLine(ConfigParameters.DEFAULT_FIRST_CONTAINER_CELL_POSITION_X,
                                                                ConfigParameters.PROFILING_VERTICAL_NUM_POINT_LIMIT);

        public SingleLine VerticalBaseLine
        {
            get { return vertical_base_line; }
        }

        private List<Container> profile_containers = new List<Container>();

        internal List<Container> ProfileContainers
        {
            get { return profile_containers; }
            set { profile_containers = value; }
        }

        public Profile()
        {
            // TODO: Complete member initialization
        }

        public Profile(SingleLine v_base_line, List<Container> profile_containers)
        {
            this.vertical_base_line = v_base_line;
            this.profile_containers = profile_containers;
        }

        public Profile(Profile p)
        {
            this.vertical_base_line = p.VerticalBaseLine;
            this.profile_containers = p.ProfileContainers;
        }

    }
}
