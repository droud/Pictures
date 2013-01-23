using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Pictures
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // check if there were arguments
            if (args.Length > 0)
            {
                // get option in lower case
                string option = args[0].ToLower().Trim();
                string value = null;

                // if option is too long
                if (option.Length > 2)
                {
                    // split value from option
                    value = option.Substring(3).Trim();
                    option = option.Substring(0, 2);
                }
                else if (args.Length > 1)
                {
                    // get option from second argument
                    value = args[1];
                }

                if (option == "/c") // settings mode
                {
                    Application.Run(new Settings());
                }
                else if (option == "/p") // preview mode
                {
                    // check to ensure window handle was provided
                    if (value == null)
                    {
                        MessageBox.Show("Cannot preview with a window handle!", "Pictures", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    // create pointer to window handle
                    IntPtr handle = new IntPtr(long.Parse(value));
                    
                    // run main form in preview mode
                    Application.Run(new Pictures(handle));
                }
                else // screensaver mode by default
                {
                    // show and start main form on each screen
                    foreach (Screen screen in Screen.AllScreens)
                    {
                        Pictures pictures = new Pictures(screen.Bounds);
                        pictures.Show();
                    }

                    // continuing running application
                    Application.Run();
                }
            }
            else // default to settings mode
            {
                Application.Run(new Settings());
            }
        }
    }
}
