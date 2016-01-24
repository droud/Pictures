using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pictures.Services
{
    public class SettingService
    {
        public string Path { get; set; }

        public int Delay { get; set; }

        public SettingService()
        {
            LoadSettings();
        }

        #region Persistence

        // loads settings from the registry, sets path and delay
        public void LoadSettings()
        {
            // Get the value stored in the Registry
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\droud\\Pictures");

            try
            {
                Path = Convert.ToString(key.GetValue("path"));
            }
            catch
            {
                Path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }

            try
            {
                Delay = Convert.ToInt32(key.GetValue("delay")) * 1000;
            }
            catch
            {
                Delay = 5000;
            }
        }

        // saves settings to the registry
        public void SaveSettings()
        {
            // create or get subkey
            RegistryKey key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\droud\\Pictures");

            // save values
            key.SetValue("path", Path);
            key.SetValue("delay", Delay);
        }

        #endregion
    }
}
