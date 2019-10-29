using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace InitDialog
{
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] IntPtr hProcess, out bool lpSystemInfo);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("User32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr rect, MainWindow.EnumDisplayMonitorsCallback callback, IntPtr dwData);

        [DllImport("User32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MainWindow.MonitorInfoEx info);

        public MainWindow()
        {
            this.InitializeComponent();
            if (!this.DoubleStartCheck())
            {
                System.Windows.Application.Current.MainWindow.Close();
                return;
            }

            // Grabbing versioning of install method

            this.versionAvail = File.Exists(this.m_strCurrentDir + "version");
            if (this.versionAvail)
            {
                var verFileStream = new FileStream(@m_strCurrentDir + "version", FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(verFileStream, Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        Versioning.Text = line;
                    }
                }
                verFileStream.Close();
            }

            int num = Screen.AllScreens.Length;
            this.getDisplayMode_EnumDisplaySettings(num);
            this.m_Setting.m_strSizeChoose = "1280 x 720 (16 : 9)";
            this.m_Setting.m_nWidthChoose = 1280;
            this.m_Setting.m_nHeightChoose = 720;
            this.m_Setting.m_nQualityChoose = 1;
            this.m_Setting.m_nLangChoose = 0;
            this.m_Setting.m_nDisplay = 0;
            this.m_Setting.m_bFullScreen = false;
            if (num == 2)
            {
                this.DisplayBox.Items.Add("PrimaryDisplay");
                this.DisplayBox.Items.Add("SubDisplay : 1");
            }
            else
            {
                for (int i = 0; i < num; i++)
                {
                    string newItem = (i == 0) ? "PrimaryDisplay" : ("SubDisplay : " + i);
                    this.DisplayBox.Items.Add(newItem);
                }
            }
            foreach (string newItem2 in this.m_astrQuality)
            {
                this.QualityBox.Items.Add(newItem2);
            }
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\illusion\\HoneySelect\\HoneySelect\\", false);
            if (this.isGame)
            {
                Microsoft.Win32.RegistryKey key;
                key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\illusion\\HoneySelect\\HoneySelect\\");
                key.SetValue("INSTALLDIR", this.m_strCurrentDir);
                this.m_strCurrentDir = (string)registryKey.GetValue("INSTALLDIR", this.m_strCurrentDir);
                key.Close();
            }
            if (File.Exists(this.m_strCurrentDir + "HoneySelect_64.exe") || File.Exists(this.m_strCurrentDir + "HoneySelect_32.exe"))
            {
                Microsoft.Win32.RegistryKey key;
                key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\illusion\\HoneySelect\\HoneySelect\\");
                key.SetValue("INSTALLDIR", this.m_strCurrentDir);
                this.m_strCurrentDir = (string)registryKey.GetValue("INSTALLDIR", this.m_strCurrentDir);
                key.Close();
            }
            else
            {
                this.m_strCurrentDir = (string)registryKey.GetValue("INSTALLDIR", this.m_strCurrentDir);
                registryKey.Close();
            }
            
            isGame = File.Exists(this.m_strCurrentDir + "HoneySelect_64.exe");
            isGame32 = File.Exists(this.m_strCurrentDir + "HoneySelect_32.exe");
            isStudio = File.Exists(this.m_strCurrentDir + "HoneyStudio_32.exe");
            isStudio32 = File.Exists(this.m_strCurrentDir + "HoneyStudio_64.exe");
            isBattle = File.Exists(this.m_strCurrentDir + "BattleArena_64.exe");
            isBattle32 = File.Exists(this.m_strCurrentDir + "BattleArena_32.exe");
            isStudioNeo = File.Exists(this.m_strCurrentDir + "StudioNEO_64.exe");
            isStudioNeo32 = File.Exists(this.m_strCurrentDir + "StudioNEO_32.exe");
            isVR = File.Exists(this.m_strCurrentDir + "HoneySelectVR.exe");
            isVRVive = File.Exists(this.m_strCurrentDir + "HoneySelectVR_Vive.exe");



            this.SetEnableAndVisible();
            string path = this.m_strCurrentDir + "/UserData/setup.xml";
        CheckConfigFile:
            if (File.Exists(path))
            {
                try
                {
                    using (FileStream fileStream = new FileStream(path, FileMode.Open))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(ConfigSetting));
                        this.m_Setting = (ConfigSetting)xmlSerializer.Deserialize(fileStream);
                    }

                    this.m_Setting.m_nDisplay = Math.Min(this.m_Setting.m_nDisplay, num - 1);
                    this.setDisplayComboBox(this.m_Setting.m_bFullScreen);
                    var flag = false;
                    for (var k = 0; k < this.ResolutionBox.Items.Count; k++)
                    {
                        if (this.ResolutionBox.Items[k].ToString() == this.m_Setting.m_strSizeChoose)
                            flag = true;
                    }
                    this.ResolutionBox.Text = flag ? this.m_Setting.m_strSizeChoose : "1280 x 720 (16 : 9)";
                    this.modeFenetre.IsChecked = this.m_Setting.m_bFullScreen;
                    this.QualityBox.Text = this.m_astrQuality[this.m_Setting.m_nQualityChoose];
                    string text = this.m_Setting.m_nDisplay == 0 ? "PrimaryDisplay" : "SubDisplay : " + this.m_Setting.m_nDisplay;
                    if (num == 2)
                    {
                        text = new[]
                        {
                        "PrimaryDisplay",
                        "SubDisplay : 1"
                        }[this.m_Setting.m_nDisplay];
                    }
                    if (this.DisplayBox.Items.Contains(text))
                        this.DisplayBox.Text = text;
                    else
                    {
                        this.DisplayBox.Text = "PrimaryDisplay";
                        this.m_Setting.m_nDisplay = 0;
                    }
                }
                catch (Exception)
                {
                    System.Windows.Forms.MessageBox.Show("/UserData/setup.xml file was corrupted, settings will be reset.");
                    File.Delete(path);
                    goto CheckConfigFile;
                }
            }
            else
            {
                this.setDisplayComboBox(false);
                this.ResolutionBox.Text = this.m_Setting.m_strSizeChoose;
                this.QualityBox.Text = this.m_astrQuality[this.m_Setting.m_nQualityChoose];
                this.DisplayBox.Text = "PrimaryDisplay";
            }
        }

        private void SetEnableAndVisible()
        {
            if (!this.isGame)
            {
                this.PLAY.IsEnabled = false;
            }
            if (!this.isGame32)
            {
                this.PLAY32.IsEnabled = false;
            }
            if (!this.isStudio)
            {
                this.PLAYStudio.IsEnabled = false;
            }
            if (!this.isStudio32)
            {
                this.PLAYStudio32.IsEnabled = false;
            }
            if (!this.isBattle)
            {
                this.PLAYBattle.IsEnabled = false;
            }
            if (!this.isBattle32)
            {
                this.PLAYBattle32.IsEnabled = false;
            }
            if (!this.isStudioNeo)
            {
                this.PLAYStudioNeo.IsEnabled = false;
            }
            if (!this.isStudioNeo32)
            {
                this.PLAYStudioNeo32.IsEnabled = false;
            }
            if (!this.isVR)
            {
                this.PLAYVR.IsEnabled = false;
            }
            if (!this.isVRVive)
            {
                this.PLAYVRVive.IsEnabled = false;
            }
        }

        private void SaveRegistry()
        {
            using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\illusion\\HoneySelect"))
            {
                registryKey.SetValue("Screenmanager Is Fullscreen mode_h3981298716", this.m_Setting.m_bFullScreen ? 1 : 0);
                registryKey.SetValue("Screenmanager Resolution Height_h2627697771", this.m_Setting.m_nHeightChoose);
                registryKey.SetValue("Screenmanager Resolution Width_h182942802", this.m_Setting.m_nWidthChoose);
                registryKey.SetValue("UnityGraphicsQuality_h1669003810", 2);
                registryKey.SetValue("UnitySelectMonitor_h17969598", this.m_Setting.m_nDisplay);
            }
            using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\illusion\\HoneySelectStudio"))
            {
                registryKey.SetValue("Screenmanager Is Fullscreen mode_h3981298716", this.m_Setting.m_bFullScreen ? 1 : 0);
                registryKey.SetValue("Screenmanager Resolution Height_h2627697771", this.m_Setting.m_nHeightChoose);
                registryKey.SetValue("Screenmanager Resolution Width_h182942802", this.m_Setting.m_nWidthChoose);
                registryKey.SetValue("UnityGraphicsQuality_h1669003810", 2);
                registryKey.SetValue("UnitySelectMonitor_h17969598", this.m_Setting.m_nDisplay);
            }
            using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\illusion\\BattleArena"))
            {
                registryKey.SetValue("Screenmanager Is Fullscreen mode_h3981298716", this.m_Setting.m_bFullScreen ? 1 : 0);
                registryKey.SetValue("Screenmanager Resolution Height_h2627697771", this.m_Setting.m_nHeightChoose);
                registryKey.SetValue("Screenmanager Resolution Width_h182942802", this.m_Setting.m_nWidthChoose);
                registryKey.SetValue("UnityGraphicsQuality_h1669003810", 2);
                registryKey.SetValue("UnitySelectMonitor_h17969598", this.m_Setting.m_nDisplay);
            }
            using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\illusion\\StudioNEO"))
            {
                registryKey.SetValue("Screenmanager Is Fullscreen mode_h3981298716", this.m_Setting.m_bFullScreen ? 1 : 0);
                registryKey.SetValue("Screenmanager Resolution Height_h2627697771", this.m_Setting.m_nHeightChoose);
                registryKey.SetValue("Screenmanager Resolution Width_h182942802", this.m_Setting.m_nWidthChoose);
                registryKey.SetValue("UnityGraphicsQuality_h1669003810", 2);
                registryKey.SetValue("UnitySelectMonitor_h17969598", this.m_Setting.m_nDisplay);
            }
        }

        private void PlayFunc(string strExe)
        {
            this.saveConfigFile(this.m_strCurrentDir + "/UserData/setup.xml");
            this.SaveRegistry();
            string ipa = this.m_strCurrentDir + "IPA.exe";
            string text = this.m_strCurrentDir + strExe;
            string playArgs = text + " --launch";
            if (File.Exists(ipa) && File.Exists(text))
            {
                Process.Start(new ProcessStartInfo(ipa)
                {
                    WorkingDirectory = this.m_strCurrentDir,
                    Arguments = playArgs
                });
                System.Windows.Application.Current.MainWindow.Close();
                return;
            }
            else if (File.Exists(text))
            {
                Process.Start(new ProcessStartInfo(text) { WorkingDirectory = this.m_strCurrentDir });
                System.Windows.Application.Current.MainWindow.Close();
                return;
            }
            new MessageWindow().SetupWindow("Warning", "\nCould not find the executable.", new object[0]);
        }

        private void PLAY_Click(object sender, RoutedEventArgs e)
        {
            this.PlayFunc("HoneySelect_64.exe");
        }

        private void PLAY32_Click(object sender, RoutedEventArgs e)
        {
            this.PlayFunc("HoneySelect_32.exe");
        }

        private void PLAYStudio_Click(object sender, RoutedEventArgs e)
        {
            this.PlayFunc("HoneyStudio_64.exe");
        }

        private void PLAYStudio32_Click(object sender, RoutedEventArgs e)
        {
            this.PlayFunc("HoneyStudio_32.exe");
        }

        private void PLAYStudioNeo_Click(object sender, RoutedEventArgs e)
        {
            this.PlayFunc("StudioNEO_64.exe");
        }

        private void PLAYStudioNeo32_Click(object sender, RoutedEventArgs e)
        {
            this.PlayFunc("StudioNEO_32.exe");
        }

        private void PLAYVR_Click(object sender, RoutedEventArgs e)
        {
            this.PlayFunc("HoneySelectVR.exe");
        }

        private void PLAYVRVive_Click(object sender, RoutedEventArgs e)
        {
            this.PlayFunc("HoneySelectVR_Vive.exe");
        }

        private void PLAYBattle_Click(object sender, RoutedEventArgs e)
        {
            this.PlayFunc("BattleArena_64.exe");
        }

        private void PLAYBattle32_Click(object sender, RoutedEventArgs e)
        {
            this.PlayFunc("BattleArena_32.exe");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.saveConfigFile(this.m_strCurrentDir + "/UserData/setup.xml");
            this.ReleaseMutex();
            System.Windows.Application.Current.MainWindow.Close();
        }

        private void Resolution_Change(object sender, SelectionChangedEventArgs e)
        {
            if (-1 == this.ResolutionBox.SelectedIndex)
            {
                return;
            }
            ComboBoxCustomItem comboBoxCustomItem = (ComboBoxCustomItem)this.ResolutionBox.SelectedItem;
            this.m_Setting.m_strSizeChoose = comboBoxCustomItem.text;
            this.m_Setting.m_nWidthChoose = comboBoxCustomItem.width;
            this.m_Setting.m_nHeightChoose = comboBoxCustomItem.height;
        }

        private void Quality_Change(object sender, SelectionChangedEventArgs e)
        {
            string a = this.QualityBox.SelectedItem.ToString();
            if (a == "Performance")
            {
                this.m_Setting.m_nQualityChoose = 0;
                return;
            }
            if (a == "Normal")
            {
                this.m_Setting.m_nQualityChoose = 1;
                return;
            }
            if (!(a == "Quality"))
            {
                return;
            }
            this.m_Setting.m_nQualityChoose = 2;
        }

        private void WindowUnChecked(object sender, RoutedEventArgs e)
        {
            this.setDisplayComboBox(false);
            this.ResolutionBox.Text = this.m_Setting.m_strSizeChoose;
            this.m_Setting.m_bFullScreen = false;
        }

        private void windowChecked(object sender, RoutedEventArgs e)
        {
            this.setDisplayComboBox(true);
            this.m_Setting.m_bFullScreen = true;
            this.setFullScreenDevice();
        }

        private void ManualOpen(object sender, RoutedEventArgs e)
        {
            string text = this.m_strCurrentDir + "/manual/お読み下さい.html";
            if (File.Exists(text))
            {
                Process.Start(text);
                return;
            }
            new MessageWindow().SetupWindow("Warning", "\nThe manual could not be found.", new object[0]);
        }

        private void ManualOpenS(object sender, RoutedEventArgs e)
        {
            string text = this.m_strCurrentDir + "/manual_s/お読み下さい.html";
            if (File.Exists(text))
            {
                Process.Start(text);
                return;
            }
            new MessageWindow().SetupWindow("Warning", "\nThe manual could not be found.", new object[0]);
        }

        private void ManualOpenV(object sender, RoutedEventArgs e)
        {
            string text = this.m_strCurrentDir + "/manual_v/お読み下さい.html";
            if (File.Exists(text))
            {
                Process.Start(text);
                return;
            }
            new MessageWindow().SetupWindow("Warning", "\nThe manual could not be found.", new object[0]);
        }

        private void ManualOpenAD(object sender, RoutedEventArgs e)
        {
            string text = this.m_strCurrentDir + "/manual_ad/お読み下さい.html";
            if (File.Exists(text))
            {
                Process.Start(text);
                return;
            }
            new MessageWindow().SetupWindow("Warning", "\nThe manual could not be found.", new object[0]);
        }

        private void ManualOpenB(object sender, RoutedEventArgs e)
        {
            string text = this.m_strCurrentDir + "/manual_b/お読み下さい.html";
            if (File.Exists(text))
            {
                Process.Start(text);
                return;
            }
            new MessageWindow().SetupWindow("Warning", "\nThe manual could not be found.", new object[0]);
        }

        private void ManualOpenSN(object sender, RoutedEventArgs e)
        {
            string text = this.m_strCurrentDir + "/manual_sn/お読み下さい.html";
            if (File.Exists(text))
            {
                Process.Start(text);
                return;
            }
            new MessageWindow().SetupWindow("Warning", "\nThe manual could not be found.", new object[0]);
        }

        private void ManualOpenV_Vive(object sender, RoutedEventArgs e)
        {
            string text = this.m_strCurrentDir + "/manual_v_Vive/お読み下さい.html";
            if (File.Exists(text))
            {
                Process.Start(text);
                return;
            }
            new MessageWindow().SetupWindow("Warning", "\nThe manual could not be found.", new object[0]);
        }

        private void Display_Change(object sender, SelectionChangedEventArgs e)
        {
            if (-1 == this.DisplayBox.SelectedIndex)
            {
                return;
            }
            this.m_Setting.m_nDisplay = this.DisplayBox.SelectedIndex;
            if (this.m_Setting.m_bFullScreen)
            {
                this.setDisplayComboBox(true);
                this.setFullScreenDevice();
            }
        }

        private void InstallDir_Open(object sender, RoutedEventArgs e)
        {
            char[] trimChars = new char[]
            {
                '/'
            };
            char[] trimChars2 = new char[]
            {
                '\\'
            };
            string text = this.m_strCurrentDir.TrimEnd(trimChars);
            text = text.TrimEnd(trimChars2);
            if (Directory.Exists(text))
            {
                Process.Start("explorer.exe", text);
                return;
            }
            new MessageWindow().SetupWindow("Warning", "\nCan't find the folder, please launch the game once.", new object[0]);
        }

        private void SceneDir_Open(object sender, RoutedEventArgs e)
        {
            char[] trimChars = new char[]
            {
                '/'
            };
            char[] trimChars2 = new char[]
            {
                '\\'
            };
            string text = this.m_strCurrentDir.TrimEnd(trimChars);
            text = text.TrimEnd(trimChars2) + "\\UserData\\studioneo\\scene";
            if (Directory.Exists(text))
            {
                Process.Start("explorer.exe", text);
                return;
            }
            new MessageWindow().SetupWindow("Warning", "\nCan't find the folder, please launch the game once.", new object[0]);
        }

        private void KoikatuSSDir_Open(object sender, RoutedEventArgs e)
        {
            char[] trimChars = new char[]
            {
                '/'
            };
            char[] trimChars2 = new char[]
            {
                '\\'
            };
            string text = this.m_strCurrentDir.TrimEnd(trimChars);
            text = text.TrimEnd(trimChars2) + "\\UserData\\cap";
            if (Directory.Exists(text))
            {
                Process.Start("explorer.exe", text);
                return;
            }
            new MessageWindow().SetupWindow("Warning", "\nCan't find the folder, please launch the game once.", new object[0]);
        }

        private void KoikatuCharaDir_Open(object sender, RoutedEventArgs e)
        {
            char[] trimChars = new char[]
            {
                '/'
            };
            char[] trimChars2 = new char[]
            {
                '\\'
            };
            string text = this.m_strCurrentDir.TrimEnd(trimChars);
            text = text.TrimEnd(trimChars2) + "\\UserData\\chara";
            if (Directory.Exists(text))
            {
                Process.Start("explorer.exe", text);
                return;
            }
            new MessageWindow().SetupWindow("Warning", "\nCan't find the folder, please launch the game once.", new object[0]);
        }

        private void ECMapDir_Open(object sender, RoutedEventArgs e)
        {
            char[] trimChars = new char[]
            {
                '/'
            };
            char[] trimChars2 = new char[]
            {
                '\\'
            };
            string text = this.m_strCurrentDir.TrimEnd(trimChars);
            text = text.TrimEnd(trimChars2) + "\\UserData\\map\\data";
            if (Directory.Exists(text))
            {
                Process.Start("explorer.exe", text);
                return;
            }
            new MessageWindow().SetupWindow("Warning", "\nCan't find the folder, please launch the game once.", new object[0]);
        }

        private void ECPoseDir_Open(object sender, RoutedEventArgs e)
        {
            char[] trimChars = new char[]
            {
                '/'
            };
            char[] trimChars2 = new char[]
            {
                '\\'
            };
            string text = this.m_strCurrentDir.TrimEnd(trimChars);
            text = text.TrimEnd(trimChars2) + "\\UserData\\pose\\data";
            if (Directory.Exists(text))
            {
                Process.Start("explorer.exe", text);
                return;
            }
            new MessageWindow().SetupWindow("Warning", "\nCan't find the folder, please launch the game once.", new object[0]);
        }

        private void SystemInfo_Open(object sender, RoutedEventArgs e)
        {
            string text = Environment.ExpandEnvironmentVariables("%windir%") + "/System32/dxdiag.exe";
            if (File.Exists(text))
            {
                Process.Start(text);
                return;
            }
            new MessageWindow().SetupWindow("Warning", "\nCan't find the folder, please launch the game once.", new object[0]);
        }

        private bool DoubleStartCheck()
        {
            bool flag;
            mutex = new Mutex(true, "Koikatu", out flag);
            bool v = !flag;
            if (v)
            {
                if (this.mutex != null)
                {
                    this.mutex.Close();
                }
                this.mutex = null;
                return false;
            }
            return true;
        }

        private bool ReleaseMutex()
        {
            if (this.mutex == null)
            {
                return false;
            }
            this.mutex.ReleaseMutex();
            this.mutex.Close();
            this.mutex = null;
            return true;
        }

        private void setDisplayComboBox(bool _bFullScreen)
        {
            this.ResolutionBox.Items.Clear();
            int nDisplay = this.m_Setting.m_nDisplay;
            foreach (MainWindow.DisplayMode displayMode in (_bFullScreen ? this.m_listCurrentDisplay[nDisplay].list : this.m_listDefaultDisplay))
            {
                ComboBoxCustomItem newItem = new ComboBoxCustomItem
                {
                    text = displayMode.text,
                    width = displayMode.Width,
                    height = displayMode.Height
                };
                this.ResolutionBox.Items.Add(newItem);
            }
        }

        private void saveConfigFile(string _strFilePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(_strFilePath)))
            {
                return;
            }
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(_strFilePath, FileMode.Create);
                if (fileStream != null)
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.GetEncoding("UTF-16")))
                    {
                        XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
                        xmlSerializerNamespaces.Add(string.Empty, string.Empty);
                        new XmlSerializer(typeof(ConfigSetting)).Serialize(streamWriter, this.m_Setting, xmlSerializerNamespaces);
                        fileStream = null;
                    }
                }
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Dispose();
                }
            }
        }

        private void getDisplayMode_CIM_VideoControllerResolution()
        {
            ManagementObjectCollection instances = new ManagementClass("CIM_VideoControllerResolution").GetInstances();
            List<MainWindow.DisplayMode> list = new List<MainWindow.DisplayMode>();
            uint num = 0u;
            uint num2 = 0u;
            foreach (ManagementBaseObject managementBaseObject in instances)
            {
                ManagementObject managementObject = (ManagementObject)managementBaseObject;
                uint nXX = (uint)managementObject["HorizontalResolution"];
                uint nYY = (uint)managementObject["VerticalResolution"];
                if ((num != nXX || num2 != nYY) && (ulong)managementObject["NumberOfColors"] == 4294967296UL)
                {
                    MainWindow.DisplayMode displayMode = this.m_listDefaultDisplay.Find((MainWindow.DisplayMode i) => (long)i.Width == (long)((ulong)nXX) && (long)i.Height == (long)((ulong)nYY));
                    if (displayMode.Width != 0)
                    {
                        list.Add(displayMode);
                    }
                    num = nXX;
                    num2 = nYY;
                }
            }
            MainWindow.DisplayModes item = default(MainWindow.DisplayModes);
            item.list = list;
            this.m_listCurrentDisplay.Add(item);
            if (instances.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Failed to list screens");
                return;
            }
            if (this.m_listCurrentDisplay.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Failed to list supported resolutions");
            }
        }

        private void getDisplayMode_EnumDisplaySettings(int numDisplay)
        {
            DISPLAY_DEVICE display_DEVICE = default(DISPLAY_DEVICE);
            display_DEVICE.cb = Marshal.SizeOf(display_DEVICE);
            List<string> list = new List<string>();
            MainWindow.MonitorInfoEx[] monitors = MainWindow.GetMonitors();
            uint num = 0u;
            while (MainWindow.EnumDisplayDevices(null, num, ref display_DEVICE, 1u))
            {
                if ((display_DEVICE.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) == DisplayDeviceStateFlags.AttachedToDesktop)
                {
                    list.Add(display_DEVICE.DeviceName);
                }
                num += 1u;
            }
            int num2 = 0;
            int num3 = -1;
            foreach (string text in list)
            {
                int num4 = 0;
                int num5 = 0;
                DEVMODE devmode = default(DEVMODE);
                List<MainWindow.DisplayMode> list2 = new List<MainWindow.DisplayMode>();
                int num6 = 0;
                while (MainWindow.EnumDisplaySettings(text, num6, ref devmode))
                {
                    int nXX = devmode.dmPelsWidth;
                    int nYY = devmode.dmPelsHeight;
                    if ((num4 != nXX || num5 != nYY) && devmode.dmBitsPerPel == 32)
                    {
                        MainWindow.DisplayMode displayMode = this.m_listDefaultDisplay.Find((MainWindow.DisplayMode dis) => dis.Width == nXX && dis.Height == nYY);
                        if (displayMode.Width != 0)
                        {
                            list2.Add(displayMode);
                        }
                        num4 = nXX;
                        num5 = nYY;
                    }
                    num6++;
                }
                MainWindow.DisplayModes item = default(MainWindow.DisplayModes);
                foreach (MainWindow.MonitorInfoEx monitorInfoEx in monitors)
                {
                    if (monitorInfoEx.szDevice == text)
                    {
                        item.x = monitorInfoEx.rcWork.Left;
                        item.y = monitorInfoEx.rcWork.Top;
                        if (monitorInfoEx.dwFlags == 1)
                        {
                            num3 = num2;
                        }
                    }
                }
                item.list = list2;
                num2++;
                this.m_listCurrentDisplay.Add(item);
            }
            if (this.m_listCurrentDisplay.Count == 0 || this.m_listCurrentDisplay.Count != numDisplay)
            {
                System.Windows.Forms.MessageBox.Show("Failed to list supported resolutions");
            }
            this.m_listCurrentDisplay.Insert(0, this.m_listCurrentDisplay[num3]);
            this.m_listCurrentDisplay.RemoveAt(num3 + 1);
        }

        private static int DisplaySort(MainWindow.DisplayModes a, MainWindow.DisplayModes b)
        {
            if (a.x < b.x)
            {
                return -1;
            }
            if (a.x > b.x)
            {
                return 1;
            }
            if (a.y < b.y)
            {
                return -1;
            }
            if (a.y > b.y)
            {
                return 1;
            }
            return 0;
        }

        private static MainWindow.MonitorInfoEx[] GetMonitors()
        {
            List<MainWindow.MonitorInfoEx> list = new List<MainWindow.MonitorInfoEx>();
            MainWindow.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate (IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData)
            {
                MainWindow.MonitorInfoEx item = new MainWindow.MonitorInfoEx
                {
                    cbSize = Marshal.SizeOf(typeof(MainWindow.MonitorInfoEx))
                };
                MainWindow.GetMonitorInfo(hMonitor, ref item);
                list.Add(item);
            }, IntPtr.Zero);
            return list.ToArray();
        }

        private void setFullScreenDevice()
        {
            int nDisplay = this.m_Setting.m_nDisplay;
            if (this.m_listCurrentDisplay[nDisplay].list.Count == 0)
            {
                this.m_Setting.m_bFullScreen = false;
                this.modeFenetre.IsChecked = new bool?(false);
                System.Windows.Forms.MessageBox.Show("This monitor doesn't support fullscreen.");
                return;
            }
            if (this.m_listCurrentDisplay[nDisplay].list.Find((MainWindow.DisplayMode x) => x.text.Contains(this.m_Setting.m_strSizeChoose)).Width == 0)
            {
                this.m_Setting.m_strSizeChoose = this.m_listCurrentDisplay[nDisplay].list[0].text;
                this.m_Setting.m_nWidthChoose = this.m_listCurrentDisplay[nDisplay].list[0].Width;
                this.m_Setting.m_nHeightChoose = this.m_listCurrentDisplay[nDisplay].list[0].Height;
            }
            this.ResolutionBox.Text = this.m_Setting.m_strSizeChoose;
        }

        public bool IsWow64()
        {
            bool flag;
            return MainWindow.GetProcAddress(MainWindow.GetModuleHandle("Kernel32.dll"), "IsWow64Process") != IntPtr.Zero && MainWindow.IsWow64Process(Process.GetCurrentProcess().Handle, out flag) && flag;
        }

        public bool Is64BitOS()
        {
            if (IntPtr.Size == 4)
            {
                return this.IsWow64();
            }
            return IntPtr.Size == 8;
        }

        private void MenuCloseButton(object sender, EventArgs e)
        {
            this.saveConfigFile(this.m_strCurrentDir + "/UserData/setup.xml");
            this.ReleaseMutex();
        }

        private const int MONITORINFOF_PRIMARY = 1;

        private const string m_strMutexName = "HoneySelect";

        private const string m_strGameRegistry = "Software\\illusion\\HoneySelect";

        private const string m_strGameExe = "HoneySelect.exe";

        private const string m_strManualDir = "/manual/index.html";

        private const string m_strOnlineManual = "http://www.illusion.jp/preview/emocre/manual/index.html";

        private const string m_strVRManualDir = "/manual_v/お読み下さい.html";

        private const string m_strSaveDir = "/UserData/setup.xml";

        private const string m_strDefSizeText = "1280 x 720 (16 : 9)";

        private const int m_nDefQuality = 1;

        private const int m_nDefWidth = 1280;

        private const int m_nDefHeight = 720;

        private const bool m_bDefFullScreen = false;

        private string m_strCurrentDir = Environment.CurrentDirectory + "/";

        private ConfigSetting m_Setting = new ConfigSetting();

        private bool isGame;

        private bool isGame32;

        private bool isBattle;

        private bool isBattle32;

        private bool isStudio;

        private bool isStudio32;

        private bool isStudioNeo;

        private bool isStudioNeo32;

        private bool isVR;

        private bool isVRVive;

        private bool versionAvail;

        private List<MainWindow.DisplayMode> m_listDefaultDisplay = new List<MainWindow.DisplayMode>
        {
            new MainWindow.DisplayMode
            {
                Width = 854,
                Height = 480,
                text = "854 x 480 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 1024,
                Height = 576,
                text = "1024 x 576 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 1136,
                Height = 640,
                text = "1136 x 640 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 1280,
                Height = 720,
                text = "1280 x 720 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 1366,
                Height = 768,
                text = "1366 x 768 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 1536,
                Height = 864,
                text = "1536 x 864 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 1600,
                Height = 900,
                text = "1600 x 900 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 1920,
                Height = 1080,
                text = "1920 x 1080 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 2048,
                Height = 1152,
                text = "2048 x 1152 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 2560,
                Height = 1440,
                text = "2560 x 1440 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 3200,
                Height = 1800,
                text = "3200 x 1800 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 3840,
                Height = 2160,
                text = "3840 x 2160 (16 : 9)"
            }
        };

        private List<MainWindow.DisplayModes> m_listCurrentDisplay = new List<MainWindow.DisplayModes>();

        private const int m_nQualityCount = 3;

        private string[] m_astrQuality = new string[]
        {
            "Performance",
            "Normal",
            "Quality"
        };

        private Mutex mutex;

        private delegate void EnumDisplayMonitorsCallback(IntPtr hMonir, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData);

        internal struct MonitorInfoEx
        {
            public int cbSize;

            public MainWindow.Rect rcMonitor;

            public MainWindow.Rect rcWork;

            public int dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        public struct Rect
        {
            public int Left;

            public int Top;

            public int Right;

            public int Bottom;
        }

        private struct DisplayMode
        {
            public int Width;

            public int Height;

            public string text;
        }

        private struct DisplayModes
        {
            public int x;

            public int y;

            public List<MainWindow.DisplayMode> list;
        }

        private void discord_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://discord.gg/F3bDEFE");
        }
    }
}
