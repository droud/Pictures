using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Pictures
{
    // stores the path and size of a picture on disk
    class Picture
    {
        #region Properties

        // stores the path of the image
        public string Path = null;

        // stores the size of the image
        public Size Size = Size.Empty;

        #endregion

        #region Constructor

        // standard constructor, requires file path and image size
        public Picture(string Path, Size Size)
        {
            this.Path = Path;
            this.Size = Size;
        }

        #endregion
    }
}
