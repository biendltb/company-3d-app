using System.Windows.Media.Media3D;
using TIS_3dAntiCollision.Core;

namespace TIS_3dAntiCollision.Business
{
    class Container
    {
        // position of container in 3D space
        private Point3D position = new Point3D();
        // length of container (20 or 40 feet)
        private ContainerTypes type = ContainerTypes.TwentyFeet;
        // trust score for position (how many point on this container pos)
        private int score = 0;

        public Container(Point3D position, ContainerTypes type, int score)
        {
            this.position = position;
            this.type = type;
            this.score = score;
        }
    }
}
