using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace InitSetting
{
    public static class SettingManager
    {
        public static ConfigSetting CurrentSettings { get; private set; } = new ConfigSetting();

        public static IEnumerable<DisplayMode> DefaultSettingList = new List<DisplayMode>
        {
            new DisplayMode
            {
                Width = 854,
                Height = 480,
                text = "854 x 480 (16 : 9)"
            },new DisplayMode
            {
                Width = 960,
                Height = 600,
                text = "960 x 600 (16 : 10)"
            },
            new DisplayMode
            {
                Width = 1024,
                Height = 576,
                text = "1024 x 576 (16 : 9)"
            },
            new DisplayMode
            {
                Width = 1136,
                Height = 640,
                text = "1136 x 640 (16 : 9)"
            },
            new DisplayMode
            {
                Width = 1280,
                Height = 720,
                text = "1280 x 720 (16 : 9)"
            },
            new DisplayMode
            {
                Width = 1280,
                Height = 800,
                text = "1280 x 800 (16 : 10)"
            },
            new DisplayMode
            {
                Width = 1440,
                Height = 900,
                text = "1440 x 900 (16 : 10)"
            },
            new DisplayMode
            {
                Width = 1536,
                Height = 864,
                text = "1536 x 864 (16 : 9)"
            },
            new DisplayMode
            {
                Width = 1600,
                Height = 900,
                text = "1600 x 900 (16 : 9)"
            },
            new DisplayMode
            {
                Width = 1680,
                Height = 1050,
                text = "1680 x 1050 (16 : 10)"
            },
            new DisplayMode
            {
                Width = 1920,
                Height = 1080,
                text = "1920 x 1080 (16 : 9)"
            },
            new DisplayMode
            {
                Width = 1920,
                Height = 1200,
                text = "1920 x 1200 (16 : 10)"
            },
            new DisplayMode
            {
                Width = 2048,
                Height = 1152,
                text = "2048 x 1152 (16 : 9)"
            },
            new DisplayMode
            {
                Width = 2560,
                Height = 1440,
                text = "2560 x 1440 (16 : 9)"
            },
            new DisplayMode
            {
                Width = 2560,
                Height = 1600,
                text = "2560 x 1600 (16 : 10)"
            },
            new DisplayMode
            {
                Width = 3200,
                Height = 1800,
                text = "3200 x 1800 (16 : 9)"
            },
            new DisplayMode
            {
                Width = 3840,
                Height = 2160,
                text = "3840 x 2160 (16 : 9)"
            },
            new DisplayMode
            {
                Width = 3840,
                Height = 2400,
                text = "3840 x 2160 (16 : 10)"
            }
        };

        private static readonly List<DisplayModes> _displayModes = new List<DisplayModes>();
        private static string _configFilePath;
        private static string[] _registryConfigPaths;
        public static List<DisplayMode> GetDisplayModes(int nDisplay, bool fullScreen) => fullScreen ? _displayModes[nDisplay].fullscreenList : _displayModes[nDisplay].windowedList;
        public static List<DisplayMode> GetCurrentDisplayModes() => GetDisplayModes(CurrentSettings.Display, CurrentSettings.FullScreen);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        public static void SaveSettings()
        {
            SaveConfigFile();
            SaveRegistry();
        }

        private static void SaveRegistry()
        {
            foreach (var keyPath in _registryConfigPaths)
            {
                using (var registryKey = Registry.CurrentUser.CreateSubKey(keyPath))
                {
                    registryKey.SetValue("Screenmanager Is Fullscreen mode_h3981298716", CurrentSettings.FullScreen ? 1 : 0);
                    registryKey.SetValue("Screenmanager Resolution Height_h2627697771", CurrentSettings.Height);
                    registryKey.SetValue("Screenmanager Resolution Width_h182942802", CurrentSettings.Width);
                    registryKey.SetValue("UnityGraphicsQuality_h1669003810", 2);
                    registryKey.SetValue("UnitySelectMonitor_h17969598", CurrentSettings.Display);
                }
            }
        }

        private static void SaveConfigFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configFilePath) ?? throw new InvalidOperationException("Invalid config path " + _configFilePath));

            using (var fileStream = new FileStream(_configFilePath, FileMode.Create))
            using (var streamWriter = new StreamWriter(fileStream, Encoding.GetEncoding("UTF-16")))
            {
                var xmlSerializerNamespaces = new XmlSerializerNamespaces();
                xmlSerializerNamespaces.Add(string.Empty, string.Empty);
                new XmlSerializer(typeof(ConfigSetting)).Serialize(streamWriter, CurrentSettings, xmlSerializerNamespaces);
            }
        }

        private static void getDisplayMode_EnumDisplaySettings(int numDisplay)
        {
            // todo this method needs cleanup

            var displayDevice = default(DISPLAY_DEVICE);
            displayDevice.cb = Marshal.SizeOf(displayDevice);
            var allDisplayNames = new List<string>();
            var dispNum = 0u;
            while (EnumDisplayDevices(null, dispNum, ref displayDevice, 1u))
            {
                if ((displayDevice.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) ==
                    DisplayDeviceStateFlags.AttachedToDesktop) allDisplayNames.Add(displayDevice.DeviceName);
                dispNum += 1u;
            }

            var primaryIndex = -1;
            for (var currentDisp = 0; currentDisp < allDisplayNames.Count; currentDisp++)
            {
                var item = default(DisplayModes);
                var displayName = allDisplayNames[currentDisp];

                var lastWidth = 0;
                var lastHeight = 0;
                var devmode = default(DEVMODE);
                var fullscreenList = new List<DisplayMode>();
                var count = 0;
                while (EnumDisplaySettings(displayName, count, ref devmode))
                {
                    var devWidth = devmode.dmPelsWidth;
                    var devHeight = devmode.dmPelsHeight;
                    if ((lastWidth != devWidth || lastHeight != devHeight) && devmode.dmBitsPerPel == 32)
                    {
                        var displayMode = DefaultSettingList.FirstOrDefault(dis => dis.Width == devWidth && dis.Height == devHeight);
                        if (displayMode.Width != 0) fullscreenList.Add(displayMode);
                        lastWidth = devWidth;
                        lastHeight = devHeight;
                    }

                    count++;
                }
                item.fullscreenList = fullscreenList;

                foreach (var screen in Screen.AllScreens)
                {
                    if (screen.DeviceName == displayName)
                    {
                        //item.x = screen.WorkingArea.Left;
                        //item.y = screen.WorkingArea.Top;
                        //item.screen = screen;
                        if (screen.Primary) primaryIndex = currentDisp;

                        item.windowedList = DefaultSettingList.Where(x => x.Height <= screen.Bounds.Height && x.Width <= screen.Bounds.Width).ToList();
                    }
                }

                _displayModes.Add(item);
            }

            if (_displayModes.Count == 0 || _displayModes.Count != numDisplay)
                MessageBox.Show("Failed to list supported resolutions");

            if (primaryIndex < 0) return;
            _displayModes.Insert(0, _displayModes[primaryIndex]);
            _displayModes.RemoveAt(primaryIndex + 1);
        }

        /// <summary>
        /// Returns false if fullscreen is not supported on the current display and it could not be enabled
        /// </summary>
        public static bool SetFullScreen(bool fullScreenEnabled)
        {
            var displayModes = GetDisplayModes(CurrentSettings.Display, fullScreenEnabled);
            CurrentSettings.FullScreen = fullScreenEnabled && displayModes.Any();

            displayModes = GetDisplayModes(CurrentSettings.Display, CurrentSettings.FullScreen);
            // See if the resolution is available
            if (displayModes.Find(x => x.text.Contains(CurrentSettings.Size)).Width == 0)
            {
                // if not, find the closest available resolution
                var displayMode = displayModes.OrderBy(x => Math.Abs(x.Width - CurrentSettings.Width) + Math.Abs(x.Height - CurrentSettings.Height)).First();
                CurrentSettings.Size = displayMode.text;
                CurrentSettings.Width = displayMode.Width;
                CurrentSettings.Height = displayMode.Height;
            }

            return fullScreenEnabled == CurrentSettings.FullScreen;
        }

        public struct DisplayMode
        {
            public int Width;

            public int Height;

            public string text;
        }

        public struct DisplayModes
        {
            //public int x;
            //
            //public int y;
            //
            //public Screen screen;

            public List<DisplayMode> fullscreenList;
            public List<DisplayMode> windowedList;
        }

        public static void Initialize(string configFilePath, params string[] registryConfigPaths)
        {
            _configFilePath = configFilePath ?? throw new ArgumentNullException(nameof(configFilePath));
            _registryConfigPaths = registryConfigPaths ?? throw new ArgumentNullException(nameof(registryConfigPaths));

            var num = Screen.AllScreens.Length;
            getDisplayMode_EnumDisplaySettings(num);
            CurrentSettings.Size = "1280 x 720 (16 : 9)";
            CurrentSettings.Width = 1280;
            CurrentSettings.Height = 720;
            CurrentSettings.Quality = 1;
            CurrentSettings.Language = 0;
            CurrentSettings.Display = 0;
            CurrentSettings.FullScreen = false;
        }

        public static void LoadSettings()
        {
            if (!File.Exists(_configFilePath)) return;

            try
            {
                using (var fileStream = new FileStream(_configFilePath, FileMode.Open))
                {
                    var xmlSerializer = new XmlSerializer(typeof(ConfigSetting));
                    CurrentSettings = (ConfigSetting)xmlSerializer.Deserialize(fileStream);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("/UserData/setup.xml file was corrupted, settings will be reset.");
                File.Delete(_configFilePath);
            }

            // Reset invalid display to primary
            if (CurrentSettings.Display >= Screen.AllScreens.Length || CurrentSettings.Display < 0)
                CurrentSettings.Display = 0;
        }
    }
}