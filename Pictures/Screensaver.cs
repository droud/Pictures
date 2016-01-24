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
using Pictures.Models;
using Pictures.Helpers;
using Pictures.Services;

namespace Pictures
{
    public partial class Screensaver : Form
    {
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

        #region Private variables
        
        // stores last mouse location
        private Point _location = Point.Empty;

        private FramingService _framingService = null;

        #endregion

        #region Constructors

        // constructor for display
        public Screensaver(Rectangle Bounds, FramingService framingService)
        {
            InitializeComponent();

            this.Bounds = Bounds;

            _framingService = framingService;

            Refresh();
        }

        // constructor for preview
        public Screensaver(IntPtr PreviewWndHandle, FramingService framingService)
        {
            InitializeComponent();

            // Set the preview window as the parent of this window
            SetParent(this.Handle, PreviewWndHandle);

            // Make this a child window so it will close when the parent dialog closes
            // GWL_STYLE = -16, WS_CHILD = 0x40000000
            SetWindowLong(this.Handle, -16, new IntPtr(GetWindowLong(this.Handle, -16) | 0x40000000));

            // Place our window inside the parent
            Rectangle ParentRect;
            GetClientRect(PreviewWndHandle, out ParentRect);
            Size = ParentRect.Size;
            Location = new Point(0, 0);

            _framingService = framingService;

            Refresh();
        }

        #endregion

        #region Settings 

        #endregion

        #region Form painting and layout methods

        // stops form from painting its background each refresh
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        // paints the form, does most of the real work
        protected override void OnPaint(PaintEventArgs e)
        {
            // stop update timer while we display, ensures entire delay occurs between refreshes
            tmrDelay.Enabled = false;

            _framingService.DrawToGraphics(e.Graphics, Application.DoEvents);

            // allow form to draw itself (nothing left though!)
            base.OnPaint(e);

            // re-enable timer so refresh happens again after delay
            tmrDelay.Enabled = true;
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
            Refresh();
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
            Refresh();
        }

        #endregion
    }
}
