using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Pictures
{
    public partial class Settings : Form
    {
        #region Constructor

        // constructor loads settings
        public Settings()
        {
            InitializeComponent();

            LoadSettings();
        }

        #endregion

        #region Setting loading and saving

        // saves settings to the registry
        private void SaveSettings()
        {
            // create or get subkey
            RegistryKey key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\droud\\Pictures");

            // save values
            key.SetValue("path", txtPath.Text);
            key.SetValue("delay", numDelay.Value);
        }

        private void LoadSettings()
        {
            // create or get subkey
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\droud\\Pictures");

            try
            {
                // retrieve values
                txtPath.Text = Convert.ToString(key.GetValue("path"));
                numDelay.Value = Convert.ToInt32(key.GetValue("delay"));
            }
            catch
            {
                // use default values
                txtPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                numDelay.Value = 5;
            }
        }

        #endregion

        #region Form and control events

        // handle save button
        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveSettings();

            Close();
        }

        // handle choose button
        private void btnChoose_Click(object sender, EventArgs e)
        {
            // show the form dialog
            dlgFolder.ShowDialog();

            // set path textbox to dialog result
            txtPath.Text = dlgFolder.SelectedPath;
        }

        // handle cancel button
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion
    }
}
