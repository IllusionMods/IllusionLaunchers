using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace InitSetting
{
    /// <summary>
    /// Handle the new 'config.xml' and 'setup.xml' split that started in HoneyCome
    /// </summary>
    public sealed class SettingManagerNew : SettingManager
    {
        private readonly string _setupFilePath;

        private class ConfigSettingStub : IConfigSetting
        {
            public string Size { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int Quality { get; set; }
            public bool FullScreen { get; set; }
            public int Display { get; set; }
            public int Language { get; set; }
        }

        public override IConfigSetting CurrentSettings => _currentSettings;
        private readonly ConfigSettingStub _currentSettings = new ConfigSettingStub();

        public override IEnumerable<DisplayMode> DefaultSettingList { get; } = new List<DisplayMode>
        {
            // todo the game refuses to use resolutions other than what's in the config screen already, an unlock plugin is necessary for arbitrary resolutions
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
                Width = 1366,
                Height = 768,
                text = "1366 x 768 (16 : 9)"
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
                text = "3840 x 2400 (16 : 10)"
            },
            new DisplayMode
            {
                Width = 5120,
                Height = 2880,
                text = "5120 x 2880 (16 : 9)"
            }
        };

        public SettingManagerNew(string configFilePath, string setupFilePath, params string[] registryConfigPaths) : base(configFilePath, registryConfigPaths)
        {
            if (setupFilePath == null) throw new ArgumentNullException(nameof(setupFilePath));

            _setupFilePath = setupFilePath;

            _currentSettings.Size = "1600 x 900 (16 : 9)";
            _currentSettings.Width = 1600;
            _currentSettings.Height = 900;
            _currentSettings.FullScreen = false;
            _currentSettings.Display = 0;
            _currentSettings.Quality = 0;
            _currentSettings.Language = 1;
        }

        protected override void LoadConfigFile()
        {
            LoadConfig();

            LoadSetup();
        }
        private void LoadConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    var doc = XDocument.Load(ConfigFilePath);
                    var elementPreferences = doc.Element("Preferences");
                    if (elementPreferences == null) throw new ArgumentNullException(nameof(elementPreferences));
                    var elementGraphic = elementPreferences.Element("Graphic");
                    if (elementGraphic == null) throw new ArgumentNullException(nameof(elementGraphic));

                    _currentSettings.Width = int.Parse(elementGraphic.Element("ScrWidth").Value);
                    _currentSettings.Height = int.Parse(elementGraphic.Element("ScrHeight").Value);
                    _currentSettings.Display = int.Parse(elementGraphic.Element("TargetDisplay").Value);
                    _currentSettings.FullScreen = string.Equals(elementGraphic.Element("FullScreen").Value, bool.TrueString, StringComparison.OrdinalIgnoreCase);
                    _currentSettings.Size = elementGraphic.Element("ScrSize").Value;
                    _currentSettings.Quality = int.Parse(elementGraphic.Element("Quality").Value);
                }
                catch (Exception)
                {
                    MessageBox.Show(ConfigFilePath + " was corrupted, settings will be reset.", "Settings error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    File.Delete(ConfigFilePath);
                }
            }

            // Reset invalid display to primary
            if (_currentSettings.Display >= Screen.AllScreens.Length || _currentSettings.Display < 0)
                _currentSettings.Display = 0;
        }
        private void LoadSetup()
        {
            if (File.Exists(_setupFilePath))
            {
                try
                {
                    var doc = XDocument.Load(_setupFilePath);
                    var elementSetting = doc.Element("Setting");
                    if (elementSetting == null) throw new ArgumentNullException(nameof(elementSetting));
                    var elementLanguage = elementSetting.Element("Language");
                    if (elementLanguage == null) throw new ArgumentNullException(nameof(elementLanguage));

                    _currentSettings.Language = int.Parse(elementLanguage.Value);
                }
                catch (Exception)
                {
                    MessageBox.Show(_setupFilePath + " was corrupted, settings will be reset.", "Settings error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    File.Delete(_setupFilePath);
                }
            }
        }

        protected override void SaveConfigFile()
        {
            SaveConfig();
            SaveSetup();
        }
        private void SaveConfig()
        {
            XDocument doc;
            if (File.Exists(ConfigFilePath))
            {
                doc = XDocument.Load(ConfigFilePath);
            }
            else
            {
                doc = new XDocument(new XDeclaration("1.0", "utf-16", "yes"));
                var elp = new XElement("Preferences");
                doc.Add(elp);
                var elg = new XElement("Graphic");
                elp.Add(elg);
            }

            var elementPreferences = doc.Element("Preferences");
            if (elementPreferences == null) throw new ArgumentNullException(nameof(elementPreferences));
            var elementGraphic = elementPreferences.Element("Graphic");
            if (elementGraphic == null) throw new ArgumentNullException(nameof(elementGraphic));

            SetOrCreateElementValue(elementGraphic, "ScrWidth", _currentSettings.Width);
            SetOrCreateElementValue(elementGraphic, "ScrHeight", _currentSettings.Height);
            SetOrCreateElementValue(elementGraphic, "TargetDisplay", _currentSettings.Display);
            SetOrCreateElementValue(elementGraphic, "FullScreen", _currentSettings.FullScreen);
            SetOrCreateElementValue(elementGraphic, "ScrSize", _currentSettings.Size);
            SetOrCreateElementValue(elementGraphic, "Quality", _currentSettings.Quality);

            doc.Save(ConfigFilePath);
        }
        private void SaveSetup()
        {
            XDocument doc;
            if (File.Exists(_setupFilePath))
            {
                doc = XDocument.Load(_setupFilePath);
            }
            else
            {
                doc = new XDocument(new XDeclaration("1.0", "utf-16", "yes"));
                var elS = new XElement("Setting");
                doc.Add(elS);
            }

            var elementSetting = doc.Element("Setting");
            if (elementSetting == null) throw new ArgumentNullException(nameof(elementSetting));

            SetOrCreateElementValue(elementSetting, "Language", _currentSettings.Language);

            doc.Save(_setupFilePath);
        }

        private static XElement SetOrCreateElementValue(XElement parent, XName name, object value)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value == null) throw new ArgumentNullException(nameof(value));

            var e = parent.Element(name);
            if (e == null)
            {
                e = new XElement(name);
                parent.Add(e);
            }

            e.Value = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";

            return e;
        }
    }
}