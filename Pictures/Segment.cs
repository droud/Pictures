using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Pictures
{
    // stores the position, bounds, and state of a grid cell
    class Segment
    {
        #region Properties

        // stores the grid column of this cell
        public int Column = 0;

        // stores the grid row of this cell
        public int Row = 0;

        // stores the pixel bounds of this cell
        public Rectangle Bounds = Rectangle.Empty;

        // stores whether this cell has been filled
        public bool Used = false;

        #endregion

        #region Constructor

        // standard constructor
        public Segment(int Column, int Row, Rectangle Bounds)
        {
            this.Column = Column;
            this.Row = Row;
            this.Bounds = Bounds;
        }

        #endregion
    }
}
