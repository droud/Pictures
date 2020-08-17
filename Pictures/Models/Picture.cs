using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SQLite;

namespace Pictures.Models
{
    // stores the path and size of a picture on disk
    public class Picture
    {
        #region Static Properties

        private static Random _random = new Random(Environment.TickCount);

        #endregion

        #region Properties

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // stores the path of the image
        [MaxLength(8192), Indexed]
        public string Path { get; set; }

        // stores the width of the image
        public int Width { get; set; }

        // stores the height of the image
        public int Height { get; set; }

        // stores the orientation of the image
        public int Rotate { get; set; }

        // stores whether the picture is wide
        [Indexed]
        public bool Wide
        {
            get {
                if (Rotate == 90 || Rotate == 270)
                    return Width > Height;
                else
                    return Height > Width;
            }
            set { }
        }

        // used to randomly select pictures
        [Indexed]
        public int Random { get; set; }

        #endregion

        #region Constructor

        public Picture()
        {
            if (Random == 0)
            {
                Random = _random.Next();
            }
        }

        #endregion
    }
}
