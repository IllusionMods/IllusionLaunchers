using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Application = System.Windows.Application;
using CheckBox = System.Windows.Controls.CheckBox;
using MessageBox = System.Windows.Forms.MessageBox;
using Orientation = System.Windows.Controls.Orientation;

namespace InitSetting
{
    public partial class MainWindow : Window
    {
        private const string MStrGameRegistry = "Software\\illusion\\AI-Syoujyo\\AI-Syoujyo";
        private const string MStrStudioRegistry = "Software\\illusion\\AI-Syoujyo\\StudioNEOV2";

        private const string MStrGameExe = "AI-Syoujyo.exe";
        private const string MStrStudioExe = "StudioNEOV2.exe";

        // launcher and xua language code
        private static readonly string[] _builtinLanguages = {"ja-JP"};
        private readonly bool _isDuringStartup;
        private readonly bool _isMainGame;
        private readonly bool _isStudio;
        private readonly string[] _qLevelNames;
        private readonly string _qNormal;

        private readonly string _qPerformance;
        private readonly string _qQuality;

        private readonly SettingManager _settingManager;

        private readonly string _sPrimarydisplay;
        private readonly string _sSubdisplay;

        public MainWindow()
        {
            try
            {
                EnvironmentHelper.InitializeDirectories();
                EnvironmentHelper.InitializeLanguage();

                InitializeComponent();

                _qPerformance = Localizable.QualityPerformance;
                _qNormal = Localizable.QualityNormal;
                _qQuality = Localizable.QualityQuality;
                _qLevelNames = new[] {_qPerformance, _qNormal, _qQuality};
                _sPrimarydisplay = Localizable.PrimaryDisplay;
                _sSubdisplay = Localizable.SubDisplay;

                if (string.IsNullOrEmpty((string) labelTranslated.Content))
                {
                    labelTranslated.Visibility = Visibility.Hidden;
                    labelTranslatedBorder.Visibility = Visibility.Hidden;
                }

                Closing += (sender, args) => _settingManager.SaveConfigFile(EnvironmentHelper.GetConfigFilePath());

                _isDuringStartup = true;

                _settingManager = new SettingManager();

                EnvironmentHelper.Initialize();

                EnvironmentHelper.CheckDuplicateStartup();

                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                CustomRes.Visibility = Visibility.Hidden;
                if (!EnvironmentHelper.KKmanExist) gridUpdate.Visibility = Visibility.Hidden;

                switch (EnvironmentHelper.DeveloperModeEnabled)
                {
                    case null:
                        toggleConsole.IsEnabled = false;
                        break;
                    case false:
                        toggleConsole.IsChecked = false;
                        break;
                    case true:
                        toggleConsole.IsChecked = true;
                        break;
                }

                foreach (var newItem2 in _qLevelNames) dropQual.Items.Add(newItem2);


                _isDuringStartup = false;

                SetupUiLanguage();

                GeneratePluginToggles();

                // Launcher Customization: Defining Warning, background and character

                if (!string.IsNullOrEmpty(EnvironmentHelper.VersionString))
                    labelDist.Content = EnvironmentHelper.VersionString;

                _isStudio = File.Exists(EnvironmentHelper.GameRootDirectory + MStrStudioExe);
                _isMainGame = File.Exists(EnvironmentHelper.GameRootDirectory + MStrGameExe);

                if (!string.IsNullOrEmpty(EnvironmentHelper.WarningString))
                    warningText.Text = EnvironmentHelper.WarningString;

                if (EnvironmentHelper.CustomCharacterImage != null)
                    PackChara.Source = EnvironmentHelper.CustomCharacterImage;
                if (EnvironmentHelper.CustomBgImage != null)
                    appBG.ImageSource = EnvironmentHelper.CustomBgImage;

                if (string.IsNullOrEmpty(EnvironmentHelper.PatreonUrl))
                {
                    linkPatreon.Visibility = Visibility.Collapsed;
                    patreonBorder.Visibility = Visibility.Collapsed;
                    patreonIMG.Visibility = Visibility.Collapsed;
                }

                // todo ?? is this check necessary?
                var num = Screen.AllScreens.Length;
                if (num == 2)
                {
                    dropDisplay.Items.Add(_sPrimarydisplay);
                    dropDisplay.Items.Add($"{_sSubdisplay} : 1");
                }
                else
                {
                    for (var i = 0; i < num; i++)
                    {
                        var newItem = i == 0 ? _sPrimarydisplay : $"{_sSubdisplay} : " + i;
                        dropDisplay.Items.Add(newItem);
                    }
                }

                ParseGameConfigFile();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw;
            }
        }

        private void GeneratePluginToggles()
        {
            var innerStack = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            toggleFullscreen.Margin = new Thickness(0, 0, 0, 0);
            ToggleBox.Children.Remove(toggleFullscreen);
            innerStack.Children.Add(toggleFullscreen);

            toggleConsole.Margin = new Thickness(0, 0, 0, 0);
            ToggleBox.Children.Remove(toggleConsole);
            innerStack.Children.Add(toggleConsole);

            ToggleablesMan.SetupModList();

            foreach (var c in ToggleablesMan.Modlist)
            {
                var cb = new CheckBox();

                cb.Name = c.CodeName;
                cb.Content = c.ModName;
                cb.Foreground = Brushes.White;

                SetupPluginToggle(cb, c.PluginDLL, c.OnAction, c.IsIPA);

                innerStack.Children.Add(cb);
            }

            Toggleables.Children.Add(innerStack);
        }

        private void ParseGameConfigFile()
        {
            var num = Screen.AllScreens.Length;
            var configFilePath = EnvironmentHelper.GetConfigFilePath();
            CheckConfigFile:
            if (File.Exists(configFilePath))
            {
                try
                {
                    _settingManager.LoadSettings(configFilePath);

                    _settingManager.CurrentSettings.Display =
                        Math.Min(_settingManager.CurrentSettings.Display, num - 1);
                    SetDisplayComboBox(_settingManager.CurrentSettings.FullScreen);
                    var flag = false;
                    foreach (var resItem in dropRes.Items)
                        if (resItem.ToString() == _settingManager.CurrentSettings.Size)
                            flag = true;

                    dropRes.Text = flag ? _settingManager.CurrentSettings.Size : "1280 x 720 (16 : 9)";
                    toggleFullscreen.IsChecked = _settingManager.CurrentSettings.FullScreen;
                    dropQual.Text = _qLevelNames[_settingManager.CurrentSettings.Quality];
                    var text = _settingManager.CurrentSettings.Display == 0
                        ? _sPrimarydisplay
                        : $"{_sSubdisplay} : " + _settingManager.CurrentSettings.Display;

                    // todo ?? is this necessary?
                    if (num == 2)
                        text = new[]
                        {
                            _sPrimarydisplay,
                            $"{_sSubdisplay} : 1"
                        }[_settingManager.CurrentSettings.Display];

                    if (dropDisplay.Items.Contains(text))
                    {
                        dropDisplay.Text = text;
                    }
                    else
                    {
                        dropDisplay.Text = _sPrimarydisplay;
                        _settingManager.CurrentSettings.Display = 0;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("/UserData/setup.xml file was corrupted, settings will be reset.");
                    File.Delete(configFilePath);
                    goto CheckConfigFile;
                }
            }
            else
            {
                SetDisplayComboBox(false);
                dropRes.Text = _settingManager.CurrentSettings.Size;
                dropQual.Text = _qLevelNames[_settingManager.CurrentSettings.Quality];
                dropDisplay.Text = _sPrimarydisplay;
            }
        }

        private void SetupPluginToggle(CheckBox toggle, string pluginName, Action<bool> onSetEnabled, bool IsIPA)
        {
            var pluginFile = "";

            if (!IsIPA)
                pluginFile = Directory.Exists(EnvironmentHelper.BepinPluginsDir)
                    ? Directory.GetFiles(EnvironmentHelper.BepinPluginsDir, pluginName + ".dl*",
                        SearchOption.AllDirectories).FirstOrDefault()
                    : null;
            else
                pluginFile = Directory.Exists(EnvironmentHelper.IPAPluginsDir)
                    ? Directory.GetFiles(EnvironmentHelper.IPAPluginsDir, pluginName + ".dl*",
                        SearchOption.AllDirectories).FirstOrDefault()
                    : null;

            if (pluginFile == null)
            {
                toggle.IsEnabled = false;
                toggle.Visibility = Visibility.Collapsed;
                return;
            }

            var f = new FileInfo(pluginFile);
            var name = pluginFile.Substring(0, f.FullName.Length - f.Extension.Length + 1);

            toggle.IsChecked = f.Extension == ".dll";

            toggle.Checked += (sender, args) =>
            {
                onSetEnabled?.Invoke(true);
                f.MoveTo(Path.Combine(f.FullName, name + ".dll"));
            };
            toggle.Unchecked += (sender, args) =>
            {
                onSetEnabled?.Invoke(false);
                f.MoveTo(Path.Combine(f.FullName, name + ".dl_"));
            };
        }

        private void LangEnglish(object sender, MouseButtonEventArgs e)
        {
            PartyFilter("en-US");
        }

        private void LangJapanese(object sender, MouseButtonEventArgs e)
        {
            PartyFilter("ja-JP");
        }

        private void LangChinese(object sender, MouseButtonEventArgs e)
        {
            PartyFilter("zh-CN");
        }

        private void LangKorean(object sender, MouseButtonEventArgs e)
        {
            PartyFilter("ko-KR");
        }

        private void LangSpanish(object sender, MouseButtonEventArgs e)
        {
            PartyFilter("es-ES");
        }

        private void LangBrazil(object sender, MouseButtonEventArgs e)
        {
            PartyFilter("pt-PT");
        }

        private void LangFrench(object sender, MouseButtonEventArgs e)
        {
            PartyFilter("fr-FR");
        }

        private void LangGerman(object sender, MouseButtonEventArgs e)
        {
            PartyFilter("de-DE");
        }

        private void LangNorwegian(object sender, MouseButtonEventArgs e)
        {
            PartyFilter("no-NB");
        }

        private void PartyFilter(string language)
        {
            EnvironmentHelper.SetLanguage(language, _builtinLanguages, _settingManager);
        }

        private void StartGame(string strExe)
        {
            _settingManager.SaveConfigFile(EnvironmentHelper.GetConfigFilePath());

            _settingManager.SaveRegistry(MStrGameRegistry);
            if (_isStudio) _settingManager.SaveRegistry(MStrStudioRegistry);

            EnvironmentHelper.StartGame(strExe);
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            StartGame(MStrGameExe);
        }

        private void buttonStartS_Click(object sender, RoutedEventArgs e)
        {
            StartGame(MStrStudioExe);
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            _settingManager.SaveConfigFile(EnvironmentHelper.GetConfigFilePath());
            Application.Current.MainWindow?.Close();
        }

        private void Resolution_Change(object sender, SelectionChangedEventArgs e)
        {
            if (-1 == dropRes.SelectedIndex) return;

            var comboBoxCustomItem = (ComboBoxCustomItem) dropRes.SelectedItem;
            _settingManager.CurrentSettings.Size = comboBoxCustomItem.text;
            _settingManager.CurrentSettings.Width = comboBoxCustomItem.width;
            _settingManager.CurrentSettings.Height = comboBoxCustomItem.height;
        }

        private void Quality_Change(object sender, SelectionChangedEventArgs e)
        {
            var a = dropQual.SelectedItem.ToString();
            if (a == _qPerformance)
            {
                _settingManager.CurrentSettings.Quality = 0;
                return;
            }

            if (a == _qNormal)
            {
                _settingManager.CurrentSettings.Quality = 1;
                return;
            }

            if (a != _qQuality) return;

            _settingManager.CurrentSettings.Quality = 2;
        }

        private void WindowUnChecked(object sender, RoutedEventArgs e)
        {
            SetDisplayComboBox(false);
            dropRes.Text = _settingManager.CurrentSettings.Size;
            _settingManager.CurrentSettings.FullScreen = false;
        }

        private void WindowChecked(object sender, RoutedEventArgs e)
        {
            SetDisplayComboBox(true);
            if (!_settingManager.SetFullScreen(_settingManager.CurrentSettings.FullScreen))
                toggleFullscreen.IsChecked = false;
            dropRes.Text = _settingManager.CurrentSettings.Size;
        }

        private void buttonManual_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.ShowManual($"{EnvironmentHelper.GameRootDirectory}\\manual\\");
        }

        private void buttonManualS_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.ShowManual($"{EnvironmentHelper.GameRootDirectory}\\manual_s\\");
        }

        private void buttonManualV_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.ShowManual($"{EnvironmentHelper.GameRootDirectory}\\manual_vr\\");
        }

        private void Display_Change(object sender, SelectionChangedEventArgs e)
        {
            if (-1 == dropDisplay.SelectedIndex) return;

            _settingManager.CurrentSettings.Display = dropDisplay.SelectedIndex;
            if (_settingManager.CurrentSettings.FullScreen)
            {
                SetDisplayComboBox(true);
                if (!_settingManager.SetFullScreen(true))
                {
                    toggleFullscreen.IsChecked = false;
                    MessageBox.Show("This monitor doesn't support fullscreen.");
                }
                else
                {
                    dropRes.Text = _settingManager.CurrentSettings.Size;
                }
            }
        }

        private void SetDisplayComboBox(bool bFullScreen)
        {
            dropRes.Items.Clear();
            var nDisplay = _settingManager.CurrentSettings.Display;
            foreach (var displayMode in bFullScreen
                ? _settingManager.GetDisplayModes(nDisplay).list
                : _settingManager.DefaultSettingList)
            {
                var newItem = new ComboBoxCustomItem
                {
                    text = displayMode.text,
                    width = displayMode.Width,
                    height = displayMode.Height
                };
                dropRes.Items.Add(newItem);
            }
        }

        private void buttonInst_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.OpenDirectory("");
        }

        private void buttonScenes_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.OpenDirectory("UserData\\Studio\\scene");
        }

        private void buttonUserData_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.OpenDirectory("UserData");
        }

        private void buttonHousing_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.OpenDirectory("UserData\\housing");
        }

        private void buttonScreenshot_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.OpenDirectory("UserData\\cap");
        }

        private void buttonFemaleCard_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.OpenDirectory("UserData\\chara\\female");
        }

        private void buttonMaleCard_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.OpenDirectory("UserData\\chara\\male");
        }

        private void discord_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.StartProcess("https://discord.gg/F3bDEFE");
        }

        private void patreon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.StartProcess(EnvironmentHelper.PatreonUrl);
        }

        private void update_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _settingManager.SaveConfigFile(EnvironmentHelper.GetConfigFilePath());
            _settingManager.SaveRegistry(MStrGameRegistry);
            if (_isStudio) _settingManager.SaveRegistry(MStrStudioRegistry);

            EnvironmentHelper.StartUpdate();
        }

        private void modeDev_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isDuringStartup) EnvironmentHelper.DeveloperModeEnabled = true;
        }

        private void modeDev_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!_isDuringStartup) EnvironmentHelper.DeveloperModeEnabled = false;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}