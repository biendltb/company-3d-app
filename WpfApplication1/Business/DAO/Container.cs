﻿using System.Windows.Media.Media3D;
using TIS_3dAntiCollision.Core;

namespace TIS_3dAntiCollision.Business.DAO
{
    class Container
    {
        // position of container in 3D space
        private Point3D position = new Point3D();

        public Point3D Position
        {
            get { return position; }
        }
        // length of container (20 or 40 feet)
        private double length = ConfigParameters.TWENTY_FEET_CONTAINER_LENGTH;

        public double Length
        {
            get { return length; }
        }
        // trust score for position (how many point on this container pos)
        private int score = 0;

        public int Score
        {
            get { return score; }
        }

        public Container(Point3D position, double length, int score)
        {
            this.position = position;
            this.length = length;
            this.score = score;
        }
    }
}
