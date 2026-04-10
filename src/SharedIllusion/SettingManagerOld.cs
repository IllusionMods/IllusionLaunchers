using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace InitSetting
{
    /// <summary>
    /// Handle the new 'config.xml' and 'setup.xml' split that started in HoneyCome
    /// </summary>
    public class SettingManagerOld : SettingManager
    {
        [XmlRoot("Setting")]
        public class ConfigSettingXml : IConfigSetting
        {
            [XmlElement("Size")]
            public string Size { get; set; }

            [XmlElement("Width")]
            public int Width { get; set; }

            [XmlElement("Height")]
            public int Height { get; set; }

            [XmlElement("Quality")]
            public int Quality { get; set; }

            [XmlElement("FullScreen")]
            public bool FullScreen { get; set; }

            [XmlElement("Display")]
            public int Display { get; set; }

            [XmlElement("Language")]
            public int Language { get; set; }
        }

        public override IConfigSetting CurrentSettings => _currentSettings;
        private ConfigSettingXml _currentSettings = new ConfigSettingXml();

        public override IEnumerable<DisplayMode> DefaultSettingList { get; } = new List<DisplayMode>
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
                Width = 1366,
                Height = 768,
                text = "1366 x 768 (16 : 9)"
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
                text = "3840 x 2400 (16 : 10)"
            },
            new DisplayMode
            {
                Width = 5120,
                Height = 2880,
                text = "5120 x 2880 (16 : 9)"
            }
        };

        public SettingManagerOld(string configFilePath, params string[] registryConfigPaths) : base(configFilePath, registryConfigPaths)
        {
            _currentSettings.Size = "1280 x 720 (16 : 9)";
            _currentSettings.Width = 1280;
            _currentSettings.Height = 720;
            _currentSettings.Quality = 1;
            _currentSettings.Language = 0;
            _currentSettings.Display = 0;
            _currentSettings.FullScreen = false;
        }

        protected override void SaveConfigFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigFilePath) ?? throw new InvalidOperationException("Invalid config path " + ConfigFilePath));

            using (var fileStream = new FileStream(ConfigFilePath, FileMode.Create))
            using (var streamWriter = new StreamWriter(fileStream, Encoding.GetEncoding("UTF-16")))
            {
                var xmlSerializerNamespaces = new XmlSerializerNamespaces();
                xmlSerializerNamespaces.Add(string.Empty, string.Empty);
                new XmlSerializer(typeof(ConfigSettingXml)).Serialize(streamWriter, CurrentSettings, xmlSerializerNamespaces);
            }
        }

        protected override void LoadConfigFile()
        {
            if (!File.Exists(ConfigFilePath)) return;

            try
            {
                using (var fileStream = new FileStream(ConfigFilePath, FileMode.Open))
                {
                    var xmlSerializer = new XmlSerializer(typeof(ConfigSettingXml));
                    _currentSettings = (ConfigSettingXml)xmlSerializer.Deserialize(fileStream);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("/UserData/setup.xml file was corrupted, settings will be reset.", "Settings error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                File.Delete(ConfigFilePath);
            }

            // Reset invalid display to primary
            if (CurrentSettings.Display >= Screen.AllScreens.Length || CurrentSettings.Display < 0)
                CurrentSettings.Display = 0;
        }
    }
}