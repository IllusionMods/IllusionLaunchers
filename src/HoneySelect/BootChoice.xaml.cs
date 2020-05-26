using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InitSetting
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class BootChoice : Window
    {
        public BootChoice()
        {
            InitializeComponent();
        }

        private const string ExecutableStudio = "HoneyStudio_64.exe";
        private const string ExecutableStudio32 = "HoneyStudio_32.exe";
        private const string ExecutableStudioNeo = "StudioNEO_64.exe";
        private const string ExecutableStudioNeo32 = "StudioNEO_32.exe";
        private const string ExecutableVR = "HoneySelectVR.exe";
        private const string ExecutableVRVive = "HoneySelectVR_Vive.exe";

        public static bool _is32 { get; private set; }

        public void SetupWindow(string title, string type, bool is32)
        {
            base.Title = title;
            _is32 = is32;

            switch (type)
            {
                case "studio":
                    SetupStudio();
                    break;
                case "vr":
                    SetupVR();
                    break;
            }

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            base.ShowDialog();
        }

        private void StartupFilter(string exeFile)
        {
            if (_is32)
            {
                switch (exeFile)
                {
                    case ExecutableStudio:
                        exeFile = ExecutableStudio32;
                        break;
                    case ExecutableStudioNeo:
                        exeFile = ExecutableStudioNeo32;
                        break;
                }
            }

            StartGame(exeFile);
        }

        private void StartGame(string strExe)
        {
            SettingManager.SaveSettings();
            if (EnvironmentHelper.StartGame(strExe))
                Close();
        }

        private void SetupVR()
        {
            VRGrid.Visibility = Visibility.Visible;
        }

        private void SetupStudio()
        {
            StudioGrid.Visibility = Visibility.Visible;
        }

        private void StartOculus_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartupFilter(ExecutableVR);
        }

        private void StartVive_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartupFilter(ExecutableVRVive);
        }

        private void StartStudio_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartupFilter(ExecutableStudio);
        }

        private void StartNeo_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartupFilter(ExecutableStudioNeo);
        }
    }
}
