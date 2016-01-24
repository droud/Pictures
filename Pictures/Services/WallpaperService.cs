using Pictures.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Pictures.Services
{
    public class WallpaperService
    {
        private FramingService _framingService = null;
        private Rectangle _bounds = Rectangle.Empty;

        // creates a bitmap and asks the framing service to fill it in with pictures, then sets the desktop wallpaper
        public WallpaperService(Rectangle bounds, FramingService framingService)
        {
            _bounds = bounds;
            _framingService = framingService;
        }

        public void RefreshDesktop()
        {
            var bitmap = new Bitmap(_bounds.Width, _bounds.Height);
            var graphics = Graphics.FromImage(bitmap);

            _framingService.DrawToGraphics(graphics);

            // save the image
            var path = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            bitmap.Save(path, ImageFormat.Bmp);

            WallpaperHelper.Set(path, WallpaperStyle.Stretched);
        }
    }
}
