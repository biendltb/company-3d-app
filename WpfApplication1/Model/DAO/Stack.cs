using System;
using System.Collections.Generic;

namespace TIS_3dAntiCollision.Model.DAO
{
    class Stack
    {
        private List<Column> columns = new List<Column>();

        internal List<Column> Columns
        {
            get { return columns; }
        }

        public Stack()
        {
            // TODO: Complete member initialization
        }

        public Stack(List<Column> _cols)
        {
            this.columns = _cols;
        }
    }
}
