using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InitSetting
{
    public abstract class SettingManager
    {
        public static SettingManager Current { get; private set; }
        public static void Initialize<T>(T instance) where T : SettingManager
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (Current != null) throw new InvalidOperationException("Already initialized");
            instance.LoadSettings();
            Current = instance;
        }

        public abstract IConfigSetting CurrentSettings { get; }

        public abstract IEnumerable<DisplayMode> DefaultSettingList { get; }

        private readonly List<DisplayModes> _displayModes = new List<DisplayModes>();
        protected readonly string ConfigFilePath;
        protected readonly string[] RegistryConfigPaths;
        public List<DisplayMode> GetDisplayModes(int nDisplay, bool fullScreen) => fullScreen ? _displayModes[nDisplay].fullscreenList : _displayModes[nDisplay].windowedList;
        public List<DisplayMode> GetCurrentDisplayModes() => GetDisplayModes(CurrentSettings.Display, CurrentSettings.FullScreen);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        private void getDisplayMode_EnumDisplaySettings(int numDisplay)
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
        public bool SetFullScreen(bool fullScreenEnabled)
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

        public SettingManager(string configFilePath, params string[] registryConfigPaths)
        {
            ConfigFilePath = configFilePath ?? throw new ArgumentNullException(nameof(configFilePath));
            RegistryConfigPaths = registryConfigPaths ?? throw new ArgumentNullException(nameof(registryConfigPaths));

            var num = Screen.AllScreens.Length;
            getDisplayMode_EnumDisplaySettings(num);
        }

        public virtual void SaveSettings()
        {
            SaveRegistry();
            SaveConfigFile();
        }
        protected virtual void SaveRegistry()
        {
            foreach (var keyPath in RegistryConfigPaths)
            {
                using (var registryKey = Registry.CurrentUser.CreateSubKey(keyPath))
                {
                    if (registryKey != null)
                    {
                        registryKey.SetValue("Screenmanager Is Fullscreen mode_h3981298716", CurrentSettings.FullScreen ? 1 : 0);
                        registryKey.SetValue("Screenmanager Resolution Height_h2627697771", CurrentSettings.Height);
                        registryKey.SetValue("Screenmanager Resolution Width_h182942802", CurrentSettings.Width);
                        registryKey.SetValue("UnityGraphicsQuality_h1669003810", 2);
                        registryKey.SetValue("UnitySelectMonitor_h17969598", CurrentSettings.Display);
                    }
                }
            }
        }
        protected abstract void SaveConfigFile();

        public virtual void LoadSettings()
        {
            LoadConfigFile();
        }
        protected abstract void LoadConfigFile();
    }
}