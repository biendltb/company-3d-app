using System;

namespace TIS_3dAntiCollision.Model.DAO
{
    class Column
    {
        // number of containers in column
        private int quantity;

        public int Quantity
        {
            get { return quantity; }
            set { quantity = value; }
        }

        // z position of top container in column
        private double z_pos;

        public double ZPos
        {
            get { return z_pos; }
            set { z_pos = value; }
        }

        // score of column
        private int score;

        public int Score
        {
            get { return score; }
            set { score = value; }
        }

        public Column()
        {
            this.quantity = 0;
            this.z_pos = 0;
            this.score = 0;
        }

        public Column(int _quantity, double _z_pos, int _score)
        {
            this.quantity = _quantity;
            this.z_pos = _z_pos;
            this.score = _score;
        }
    }
}
