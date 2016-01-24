using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;

namespace Pictures
{
    public partial class DesktopPictures
    {
        private Timer tmrDelay;

        #region Interop and external call imports
        
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);
        
        #endregion

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

        #region Private variables
        
        // stores last mouse location
        private Point _location = Point.Empty;

        // stores path for files
        private string _path = null;

        // stores paths and dimensions of pictures
        private List<Picture> _pictures = new List<Picture>();

        // used to randomize stuff
        public Random _random = new Random(Environment.TickCount);

        #endregion

        #region Constructors

        // constructor for desktop
        public DesktopPictures()
        {
            var handle = GetDesktopHandle();
            
            // Get the Device Context of the WorkerW
            IntPtr dc = W32.GetDCEx(handle, IntPtr.Zero, (W32.DeviceContextValues)0x403);
            if (dc != IntPtr.Zero)
            {
                // Create a Graphics instance from the Device Context
                using (Graphics g = Graphics.FromHdc(dc))
                {

                    // Use the Graphics instance to draw a white rectangle in the upper 
                    // left corner. In case you have more than one monitor think of the 
                    // drawing area as a rectangle that spans across all monitors, and 
                    // the 0,0 coordinate beeing in the upper left corner.
                    g.FillRectangle(new SolidBrush(Color.White), 0, 0, 500, 500);

                }
                // make sure to release the device context after use.
                W32.ReleaseDC(handle, dc);
            }

            // Place our window inside the parent
            //Rectangle ParentRect;
            //GetClientRect(PreviewWndHandle, out ParentRect);
            //Size = ParentRect.Size;
            //Location = new Point(0, 0);

            //LoadSettings(); // sets _path
            //LoadPictures(_path);

            //Refresh();
        }


        private static IntPtr GetDesktopHandle()
        {
            // Fetch the Progman window
            IntPtr progman = W32.FindWindow("Progman", null);
            IntPtr defview = W32.FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);
            IntPtr listview = W32.FindWindowEx(defview, IntPtr.Zero, "SysListView32", IntPtr.Zero);
            //IntPtr listview = W32.FindWindowEx(defview, IntPtr.Zero, "WorkerW", IntPtr.Zero);

            return listview;

            IntPtr result = IntPtr.Zero;

            // Send 0x052C to Progman. This message directs Progman to spawn a 
            // WorkerW behind the desktop icons. If it is already there, nothing 
            // happens.
            W32.SendMessageTimeout(progman,
                                   0x052C,
                                   new IntPtr(0),
                                   IntPtr.Zero,
                                   W32.SendMessageTimeoutFlags.SMTO_NORMAL,
                                   1000,
                                   out result);

            IntPtr workerw = IntPtr.Zero;

            // We enumerate all Windows, until we find one, that has the SHELLDLL_DefView 
            // as a child. 
            // If we found that window, we take its next sibling and assign it to workerw.
            W32.EnumWindows(new W32.EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = W32.FindWindowEx(tophandle,
                                            IntPtr.Zero,
                                            "SHELLDLL_DefView",
                                            IntPtr.Zero);

                if (p != IntPtr.Zero)
                {
                    // Gets the WorkerW Window after the current one.
                    workerw = W32.FindWindowEx(IntPtr.Zero,
                                               tophandle,
                                               "WorkerW",
                                               IntPtr.Zero);
                }

                return true;
            }), IntPtr.Zero);

            return workerw;
        }


        #endregion

        #region Settings and picture loading

        // loads settings from the registry, sets path and timer delay
        private void LoadSettings()
        {
            // Get the value stored in the Registry
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\droud\\Pictures");
            try
            {
                _path = Convert.ToString(key.GetValue("path"));
                tmrDelay.Interval = Convert.ToInt32(key.GetValue("delay")) * 1000;
            }
            catch
            {
                 _path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                tmrDelay.Interval = 5000;
           }
        }

        // loads pictures from the path
        private void LoadPictures(string Path)
        {
            var files = Directory.GetFiles(Path);
            foreach (var file in files)
            {
                var lower = file.ToLower();

                // ensure we only try to work with JPEGs
                if (lower.EndsWith(".jpg") || lower.EndsWith(".jpeg"))
                {
                    try
                    {
                        // create new picture object to store filename and size
                        var picture = new Picture(file, GetJpegImageSize(file));
                        _pictures.Add(picture);
                    }
                    catch // skip if there was an error
                    {
                        Console.WriteLine("Could not load picture: " + file);
                    }
                }

                // TODO: support BMPs and PNGs and such (need quick dimension loader)
            }

            // recursively traverse folders
            foreach (var dir in Directory.GetDirectories(Path))
                LoadPictures(dir);
        }

        // gets a JPEG image size quickly, pulled from StackOverflow
        public static Size GetJpegImageSize(string filename)
        {
            FileStream stream = null;
            BinaryReader rdr = null;
            try
            {
                stream = File.OpenRead(filename);
                rdr = new BinaryReader(stream);
                // keep reading packets until we find one that contains Size info
                for (; ; )
                {
                    byte code = rdr.ReadByte();
                    if (code != 0xFF) throw new ApplicationException(
                               "Unexpected value in file " + filename);
                    code = rdr.ReadByte();
                    switch (code)
                    {
                        // filler byte
                        case 0xFF:
                            stream.Position--;
                            break;
                        // packets without data
                        case 0xD0:
                        case 0xD1:
                        case 0xD2:
                        case 0xD3:
                        case 0xD4:
                        case 0xD5:
                        case 0xD6:
                        case 0xD7:
                        case 0xD8:
                        case 0xD9:
                            break;
                        // packets with size information
                        case 0xC0:
                        case 0xC1:
                        case 0xC2:
                        case 0xC3:
                        case 0xC4:
                        case 0xC5:
                        case 0xC6:
                        case 0xC7:
                        case 0xC8:
                        case 0xC9:
                        case 0xCA:
                        case 0xCB:
                        case 0xCC:
                        case 0xCD:
                        case 0xCE:
                        case 0xCF:
                            ReadBEUshort(rdr);
                            rdr.ReadByte();
                            ushort h = ReadBEUshort(rdr);
                            ushort w = ReadBEUshort(rdr);
                            return new Size(w, h);
                        // irrelevant variable-length packets
                        default:
                            int len = ReadBEUshort(rdr);
                            stream.Position += len - 2;
                            break;
                    }
                }
            }
            finally
            {
                if (rdr != null) rdr.Close();
                if (stream != null) stream.Close();
            }
        }

        // helper for JPEG image sizing
        private static ushort ReadBEUshort(BinaryReader rdr)
        {
            ushort hi = rdr.ReadByte();
            hi <<= 8;
            ushort lo = rdr.ReadByte();
            return (ushort)(hi | lo);
        }

        #endregion

        #region Form painting and layout methods

        // stops form from painting its background each refresh
        protected void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        // paints the form, does most of the real work
        protected void OnPaint(PaintEventArgs e)
        {
            // stop update timer while we display, ensures entire delay occurs between refreshes
            tmrDelay.Enabled = false;

            // hold bitmap and brush reference outside of try/catch so we can dispose even if there are errors
            Bitmap bitmap = null;
            Brush brush = new SolidBrush(Color.Black);

            try
            {
                // copy picture list
                var pictures = new List<Picture>(_pictures);

                // get a random grid size
                var size = CreateSegmentSize();

                // initialize segments (grid) according to size
                var segments = CreateSegments(size);

                // main loop, we break out if needed
                while (true)
                {
                    var available = segments.Where(s => s.Used == false).ToList();
                    if (available.Count == 0) break; // no more segments

                    // randomly choose a segment from unused segments
                    var segment = available[_random.Next() % available.Count];

                    // randomly choose a picture
                    if (pictures.Count == 0) break; // no more pictures
                    var picture = pictures[_random.Next() % pictures.Count];

                    // choose which set of spans to use based on height vs width
                    var spans = picture.Size.Height > picture.Size.Width ? _tallspans : _widespans;

                    // set our span to 1x1 for default
                    var span = Size.Empty;

                    // find largest span we can fit
                    foreach (var testspan in spans)
                    {
                        if (CheckSegments(segments, size, segment.Column, segment.Row, testspan.Width, testspan.Height))
                        {
                            span = testspan;
                            break;
                        }
                    }

                    // check if we have a span, otherwise we need a wide picture
                    if (span.Width == 0)
                    {
                        // break if there are no wide pictures to fill this segment
                        var wides = pictures.Where(p => p.Size.Width > p.Size.Height).ToList();
                        if (wides.Count() == 0) break;

                        // choose one randomly and set minimum span
                        span = new Size(1, 1);
                        picture = wides[_random.Next() % wides.Count];
                    }

                    // mark used segments
                    MarkSegments(segments, segment.Column, segment.Row, span.Width, span.Height);

                    // set segment size to cover span
                    segment.Bounds.Width = segment.Bounds.Width * span.Width;
                    segment.Bounds.Height = segment.Bounds.Height * span.Height;

                    // calculate size ratios
                    var xratio = (double)segment.Bounds.Width / (double)picture.Size.Width;
                    var yratio = (double)segment.Bounds.Height / (double)picture.Size.Height;

                    // calculate source sizes bounds
                    var bounds = Rectangle.Empty;
                    if (xratio > yratio) // better width fit than height fit
                    {
                        bounds.Width = picture.Size.Width;
                        bounds.Height = (int)(picture.Size.Height * (yratio / xratio));
                    }
                    else // better height fit than width fit
                    {
                        bounds.Width = (int)(picture.Size.Width * (xratio / yratio));
                        bounds.Height = picture.Size.Height;
                    }

                    // set source bounds to center based on size
                    bounds.X = (int)((picture.Size.Width / 2.0) - (bounds.Width / 2.0));
                    bounds.Y = (int)((picture.Size.Height / 2.0) - (bounds.Height / 2.0));

                    // actually load image from disk
                    bitmap = new Bitmap(picture.Path);

                    // TODO: configurable border size?

                    // hack segment bounds to add some border
                    segment.Bounds.Width = segment.Bounds.Width + 10;
                    segment.Bounds.Height = segment.Bounds.Height + 10;
                    segment.Bounds.X = segment.Bounds.X - 5;
                    segment.Bounds.Y = segment.Bounds.Y - 5;

                    // draw black background
                    e.Graphics.FillRectangle(brush, segment.Bounds);

                    // hack segment bounds to fit inside border
                    segment.Bounds.Width = segment.Bounds.Width - 20;
                    segment.Bounds.Height = segment.Bounds.Height - 20;
                    segment.Bounds.X = segment.Bounds.X + 10;
                    segment.Bounds.Y = segment.Bounds.Y + 10;

                    // draw image on form
                    e.Graphics.DrawImage(bitmap, segment.Bounds, bounds, GraphicsUnit.Pixel);
                    
                    // allow other events to fire
                    Application.DoEvents();

                    // remove image from source set
                    pictures.Remove(picture);

                    // clean up
                    bitmap.Dispose();
                    bitmap = null;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
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

            // allow form to draw itself (nothing left though!)
            //base.OnPaint(e);

            // re-enable timer so refresh happens again after delay
            tmrDelay.Enabled = true;
        }

        private Size CreateSegmentSize()
        {
            // get screen/form bounds
            var bounds = new Rectangle(0, 0, 100, 100);// this.Bounds;

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
        private List<Segment> CreateSegments(Size Size)
        {
            // get screen/form bounds
            var bounds = new Rectangle(0, 0, 100, 100);// this.Bounds;

            // calculate grid cell size
            var width = bounds.Width / Size.Width;
            var height = bounds.Height / Size.Height;

            // create segments
            var segments = new List<Segment>();
            for (int x = 0; x < Size.Width; x++)
            {
                for (int y = 0; y < Size.Height; y++)
                {
                    var segment = new Segment(x, y, new Rectangle(x * width, y * height, width, height));
                    segments.Add(segment);
                }
            }

            return segments;
        }

        // checks to see if a span of segments are all unused
        private bool CheckSegments(List<Segment> Segments, Size Size, int Column, int Row, int Columns, int Rows)
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
        private void MarkSegments(List<Segment> Segments, int Column, int Row, int Columns, int Rows)
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

        #endregion

        #region Form and control events

        // prepares form and displays initial pictures
        private void Pictures_Load(object sender, EventArgs e)
        {
#if !DEBUG
            // hide cursor
            Cursor.Hide();

            // make sure form is on top
            this.TopMost = true;
#endif

            // first display
            //Refresh();
        }

        // handles mouse movement
        private void Pictures_MouseMove(object sender, MouseEventArgs e)
        {
            // check to see if we have a last location
            if (!_location.IsEmpty)
            {
                // exit if mouse is moved a bit
                if (Math.Abs(_location.X - e.X) > 5 ||
                    Math.Abs(_location.Y - e.Y) > 5)
                    Application.Exit();
            }

            // update current mouse location
            _location = e.Location;
        }

        // exit if mouse is clicked
        private void Pictures_MouseClick(object sender, MouseEventArgs e)
        {
            Application.Exit();
        }

        // exit if key is pressed
        private void Pictures_KeyPress(object sender, KeyPressEventArgs e)
        {
            Application.Exit();
        }

        // refresh each time the timer ticks
        private void tmrDelay_Tick(object sender, EventArgs e)
        {
            //Refresh();
        }

        #endregion
    }
}
