using Pictures.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Pictures.Services
{
    public class FramingService
    {
        #region Grid size and span settings

        private Size[] _sizes = new Size[] {
            new Size(16, 12),
            new Size(12, 9),
            new Size(8, 6),
            new Size(4, 3),
            new Size(4, 3),
            new Size(2, 2),
            new Size(1, 1),
        };

        private Size[] _tallspans = new Size[] {
            new Size(5, 6),
            new Size(4, 6),
            new Size(4, 5),
            new Size(3, 5),
            new Size(3, 4),
            new Size(2, 3),
            new Size(1, 2),
        };

        private Size[] _widespans = new Size[] {
            new Size(6, 6),
            new Size(5, 5),
            new Size(4, 4),
            new Size(3, 3),
            new Size(2, 2),
            new Size(1, 1),
        };

        #endregion

        #region Private

        // used to randomize stuff
        private Random _random = new Random(Environment.TickCount);

        // picture source
        private PhotoService _picturesRepo;

        #endregion

        #region Constructor

        public FramingService(PhotoService picturesRepo)
        {
            _picturesRepo = picturesRepo;
        }

        #endregion

        #region Drawing

        public void DrawToGraphics(Graphics graphics, Action interrupt = null)
        {
            // hold bitmap and brush reference outside of try/catch so we can dispose even if there are errors
            Bitmap bitmap = null;
            Brush brush = new SolidBrush(Color.Black);

            try
            {
                // get a random grid size
                var size = CreateFrameSize(graphics);

                // initialize segments (grid) according to size
                var segments = CreateFrames(graphics, size);

                // main loop, we break out if needed
                while (true)
                {
                    var available = segments.Where(s => s.Used == false).ToList();
                    if (available.Count == 0) break; // no more segments

                    // randomly choose a segment from unused segments
                    var segment = available[_random.Next() % available.Count];

                    // randomly choose a picture
                    var picture = _picturesRepo.GetRandomPicture();
                    if (picture == null) break; // no more pictures

                    // choose which set of spans to use based on height vs width
                    var spans = picture.Height > picture.Width ? _tallspans : _widespans;

                    // set our span to 1x1 for default
                    var span = Size.Empty;

                    // find largest span we can fit
                    foreach (var testspan in spans)
                    {
                        if (CheckFrames(segments, size, segment.Column, segment.Row, testspan.Width, testspan.Height))
                        {
                            span = testspan;
                            break;
                        }
                    }

                    // check if we have a span, otherwise we need a wide picture
                    if (span.Width == 0)
                    {
                        // choose one randomly and set minimum span
                        picture = _picturesRepo.GetRandomWidePicture();
                        if (picture == null) break;

                        span = new Size(1, 1);
                    }

                    // mark used segments
                    MarkFrames(segments, segment.Column, segment.Row, span.Width, span.Height);

                    // set segment size to cover span
                    segment.Bounds.Width = segment.Bounds.Width * span.Width;
                    segment.Bounds.Height = segment.Bounds.Height * span.Height;

                    // calculate size ratios
                    var xratio = (double)segment.Bounds.Width / (double)picture.Width;
                    var yratio = (double)segment.Bounds.Height / (double)picture.Height;

                    // calculate source sizes bounds
                    var bounds = Rectangle.Empty;
                    if (xratio > yratio) // better width fit than height fit
                    {
                        bounds.Width = picture.Width;
                        bounds.Height = (int)(picture.Height * (yratio / xratio));
                    }
                    else // better height fit than width fit
                    {
                        bounds.Width = (int)(picture.Width * (xratio / yratio));
                        bounds.Height = picture.Height;
                    }

                    // set source bounds to center based on size
                    bounds.X = (int)((picture.Width / 2.0) - (bounds.Width / 2.0));
                    bounds.Y = (int)((picture.Height / 2.0) - (bounds.Height / 2.0));

                    // actually load image from disk
                    bitmap = new Bitmap(picture.Path);

                    // TODO: configurable border size?

                    // hack segment bounds to add some border
                    segment.Bounds.Width = segment.Bounds.Width + 10;
                    segment.Bounds.Height = segment.Bounds.Height + 10;
                    segment.Bounds.X = segment.Bounds.X - 5;
                    segment.Bounds.Y = segment.Bounds.Y - 5;

                    // draw black background
                    graphics.FillRectangle(brush, segment.Bounds);

                    // hack segment bounds to fit inside border
                    segment.Bounds.Width = segment.Bounds.Width - 20;
                    segment.Bounds.Height = segment.Bounds.Height - 20;
                    segment.Bounds.X = segment.Bounds.X + 10;
                    segment.Bounds.Y = segment.Bounds.Y + 10;

                    // draw image on form
                    graphics.DrawImage(bitmap, segment.Bounds, bounds, GraphicsUnit.Pixel);

                    // clean up
                    bitmap.Dispose();
                    bitmap = null;

                    // interrupt so system can breathe
                    if (interrupt != null)
                    {
                        interrupt();
                    }
                }
            }
            catch (Exception exception)
            {
                // TODO: log something
            }

            // make sure we clear bitmap to free memory
            if (bitmap != null)
            {
                bitmap.Dispose();
                bitmap = null;
            }

            // clear out brush too
            brush.Dispose();
            brush = null;
        }

        #endregion

        #region Layout

        private Size CreateFrameSize(Graphics graphics)
        {
            // get screen/form bounds
            var bounds = graphics.VisibleClipBounds;
            //var bounds = this.Bounds;

            // calculate maximum columns and randomly choose one
            // TODO: make minimum image width configurable
            var maxcols = (int)(bounds.Width / 160.0);
            var cols = (_random.Next() % maxcols) + 1;

            // calculate segment width by columns
            var width = bounds.Width / cols;

            // calculate rows by best match for segment width
            var rows = (int)Math.Floor(bounds.Height / (width / (4.0 / 3.0)));

            return new Size(cols, rows);
        }

        // creates a set of segments based on form bounds and grid size
        private List<Frame> CreateFrames(Graphics graphics, Size Size)
        {
            // get screen/form bounds
            var bounds = graphics.VisibleClipBounds;

            // calculate grid cell size
            var width = (int)bounds.Width / Size.Width;
            var height = (int)bounds.Height / Size.Height;

            // create segments
            var segments = new List<Frame>();
            for (int x = 0; x < Size.Width; x++)
            {
                for (int y = 0; y < Size.Height; y++)
                {
                    var segment = new Frame(x, y, new Rectangle(x * width, y * height, width, height));
                    segments.Add(segment);
                }
            }

            return segments;
        }

        // checks to see if a span of segments are all unused
        private bool CheckFrames(List<Frame> Segments, Size Size, int Column, int Row, int Columns, int Rows)
        {
            // sanity checks - can't fit a span that exceeds bounds
            if (Column + Columns > Size.Width)
                return false;
            if (Row + Rows > Size.Height)
                return false;

            // look at each segment
            foreach (var segment in Segments)
            {
                // check if segment is in span and return false if used
                if (segment.Column >= Column && segment.Column < Column + Columns)
                {
                    if (segment.Row >= Row && segment.Row < Row + Rows)
                    {
                        if (segment.Used)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        // mark segments in a span as used
        private void MarkFrames(List<Frame> Segments, int Column, int Row, int Columns, int Rows)
        {
            // we assume input here is valid, no sanity check

            // look at each segment
            foreach (var segment in Segments)
            {
                // if segment is in span mark as used
                if (segment.Column >= Column && segment.Column < Column + Columns)
                {
                    if (segment.Row >= Row && segment.Row < Row + Rows)
                    {
                        segment.Used = true;
                    }
                }
            }
        }

        // stores the position, bounds, and state of a grid cell
        private class Frame
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
            public Frame(int Column, int Row, Rectangle Bounds)
            {
                this.Column = Column;
                this.Row = Row;
                this.Bounds = Bounds;
            }

            #endregion
        }

        #endregion
    }
}
