using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using Pictures.Services;

namespace Pictures
{
    public partial class Configuration : Form
    {
        private SettingService _settingsService = null;

        #region Constructor

        // constructor loads settings
        public Configuration(SettingService settingsService)
        {
            InitializeComponent();

            _settingsService = settingsService;
        }

        #endregion


        #region Form and control events

        // handle save button
        private void btnSave_Click(object sender, EventArgs e)
        {
            _settingsService.Path = txtPath.Text;
            _settingsService.Delay = (int)numDelay.Value;

            _settingsService.SaveSettings();

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
