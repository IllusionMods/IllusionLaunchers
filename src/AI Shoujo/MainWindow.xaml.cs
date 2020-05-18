using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;

namespace InitSetting
{
    public partial class MainWindow : Window
    {
        // Game-specific constants -------------------------------------------------------------------
        private const string RegistryKeyGame = "Software\\illusion\\AI-Syoujyo\\AI-Syoujyo";
        private const string RegistryKeyStudio = "Software\\illusion\\AI-Syoujyo\\StudioNEOV2";
        private const string ExecutableGame = "AI-Syoujyo.exe";
        private const string ExecutableStudio = "StudioNEOV2.exe";
        // Languages built into the game itself
        private static readonly string[] _builtinLanguages = { "ja-JP" };

        // Normal fields, don't fill in --------------------------------------------------------------
        private readonly bool _isDuringStartup;
        private readonly bool _mainGameExists;
        private readonly bool _studioExists;
        private readonly string[] _qLevelNames;
        private readonly string _qNormal;
        private readonly string _qPerformance;
        private readonly string _qQuality;
        private readonly string _sPrimarydisplay;
        private readonly string _sSubdisplay;

        public MainWindow()
        {
            try
            {
                _isDuringStartup = true;

                // Initialize code -------------------------------------
                EnvironmentHelper.Initialize(_builtinLanguages);

                _mainGameExists = File.Exists(EnvironmentHelper.GameRootDirectory + ExecutableGame);
                _studioExists = File.Exists(EnvironmentHelper.GameRootDirectory + ExecutableStudio);

                if (_studioExists)
                    SettingManager.Initialize(EnvironmentHelper.GetConfigFilePath(), RegistryKeyGame, RegistryKeyStudio);
                else
                    SettingManager.Initialize(EnvironmentHelper.GetConfigFilePath(), RegistryKeyGame);

                // Initialize interface --------------------------------
                InitializeComponent();

                _qPerformance = Localizable.QualityPerformance;
                _qNormal = Localizable.QualityNormal;
                _qQuality = Localizable.QualityQuality;
                _qLevelNames = new[] { _qPerformance, _qNormal, _qQuality };
                _sPrimarydisplay = Localizable.PrimaryDisplay;
                _sSubdisplay = Localizable.SubDisplay;
                foreach (var qalName in _qLevelNames) dropQual.Items.Add(qalName);

                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                CustomRes.Visibility = Visibility.Hidden;

                if (string.IsNullOrEmpty((string)labelTranslated.Content))
                {
                    labelTranslated.Visibility = Visibility.Hidden;
                    labelTranslatedBorder.Visibility = Visibility.Hidden;
                }

                if (!EnvironmentHelper.KKmanExist)
                    gridUpdate.Visibility = Visibility.Hidden;

                // Launcher Customization: Defining Warning, background and character
                if (!string.IsNullOrEmpty(EnvironmentHelper.VersionString))
                    labelDist.Content = EnvironmentHelper.VersionString;

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

                PluginToggleManager.CreatePluginToggles(Toggleables);

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

                Closed += (sender, args) => SettingManager.SaveSettings();

                _isDuringStartup = false;
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to start the launcher, please consider reporting this error to the developers.\n\nError that caused the crash: " + e, "Launcher crash", MessageBoxButtons.OK, MessageBoxIcon.Error);
                File.WriteAllText(Path.Combine(EnvironmentHelper.GameRootDirectory, "launcher_crash.log"), e.ToString());
                Close();
            }
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
                    SettingManager.LoadSettings();

                    SettingManager.CurrentSettings.Display =
                        Math.Min(SettingManager.CurrentSettings.Display, num - 1);
                    SetDisplayComboBox(SettingManager.CurrentSettings.FullScreen);
                    var flag = false;
                    foreach (var resItem in dropRes.Items)
                        if (resItem.ToString() == SettingManager.CurrentSettings.Size)
                            flag = true;

                    dropRes.Text = flag ? SettingManager.CurrentSettings.Size : "1280 x 720 (16 : 9)";
                    toggleFullscreen.IsChecked = SettingManager.CurrentSettings.FullScreen;
                    dropQual.Text = _qLevelNames[SettingManager.CurrentSettings.Quality];
                    var text = SettingManager.CurrentSettings.Display == 0
                        ? _sPrimarydisplay
                        : $"{_sSubdisplay} : " + SettingManager.CurrentSettings.Display;

                    // todo ?? is this necessary?
                    if (num == 2)
                        text = new[]
                        {
                            _sPrimarydisplay,
                            $"{_sSubdisplay} : 1"
                        }[SettingManager.CurrentSettings.Display];

                    if (dropDisplay.Items.Contains(text))
                    {
                        dropDisplay.Text = text;
                    }
                    else
                    {
                        dropDisplay.Text = _sPrimarydisplay;
                        SettingManager.CurrentSettings.Display = 0;
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
                dropRes.Text = SettingManager.CurrentSettings.Size;
                dropQual.Text = _qLevelNames[SettingManager.CurrentSettings.Quality];
                dropDisplay.Text = _sPrimarydisplay;
            }
        }

        private void LangEnglish(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.SetLanguage("en-US");
        }

        private void LangJapanese(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.SetLanguage("ja-JP");
        }

        private void LangChinese(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.SetLanguage("zh-CN");
        }

        private void LangKorean(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.SetLanguage("ko-KR");
        }

        private void LangSpanish(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.SetLanguage("es-ES");
        }

        private void LangBrazil(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.SetLanguage("pt-PT");
        }

        private void LangFrench(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.SetLanguage("fr-FR");
        }

        private void LangGerman(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.SetLanguage("de-DE");
        }

        private void LangNorwegian(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.SetLanguage("no-NB");
        }

        private void StartGame(string strExe)
        {
            SettingManager.SaveSettings();
            if (EnvironmentHelper.StartGame(strExe))
                Close();
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            StartGame(ExecutableGame);
        }

        private void buttonStartS_Click(object sender, RoutedEventArgs e)
        {
            StartGame(ExecutableStudio);
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ResolutionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (-1 == dropRes.SelectedIndex) return;

            var comboBoxCustomItem = (ComboBoxCustomItem)dropRes.SelectedItem;
            SettingManager.CurrentSettings.Size = comboBoxCustomItem.text;
            SettingManager.CurrentSettings.Width = comboBoxCustomItem.width;
            SettingManager.CurrentSettings.Height = comboBoxCustomItem.height;
        }

        private void QualityChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = dropQual.SelectedItem.ToString();
            if (a == _qPerformance)
            {
                SettingManager.CurrentSettings.Quality = 0;
                return;
            }

            if (a == _qNormal)
            {
                SettingManager.CurrentSettings.Quality = 1;
                return;
            }

            if (a != _qQuality) return;

            SettingManager.CurrentSettings.Quality = 2;
        }

        private void FullscreenUnChecked(object sender, RoutedEventArgs e)
        {
            SetDisplayComboBox(false);
            dropRes.Text = SettingManager.CurrentSettings.Size;
            SettingManager.CurrentSettings.FullScreen = false;
        }

        private void FullscreenChecked(object sender, RoutedEventArgs e)
        {
            SetDisplayComboBox(true);
            if (!SettingManager.SetFullScreen(SettingManager.CurrentSettings.FullScreen))
                toggleFullscreen.IsChecked = false;
            dropRes.Text = SettingManager.CurrentSettings.Size;
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

        private void DisplayChanged(object sender, SelectionChangedEventArgs e)
        {
            if (-1 == dropDisplay.SelectedIndex) return;

            SettingManager.CurrentSettings.Display = dropDisplay.SelectedIndex;
            if (SettingManager.CurrentSettings.FullScreen)
            {
                SetDisplayComboBox(true);
                if (!SettingManager.SetFullScreen(true))
                {
                    toggleFullscreen.IsChecked = false;
                    MessageBox.Show("This monitor doesn't support fullscreen.");
                }
                else
                {
                    dropRes.Text = SettingManager.CurrentSettings.Size;
                }
            }
        }

        private void SetDisplayComboBox(bool bFullScreen)
        {
            dropRes.Items.Clear();
            var nDisplay = SettingManager.CurrentSettings.Display;
            foreach (var displayMode in bFullScreen
                ? SettingManager.GetDisplayModes(nDisplay).list
                : SettingManager.DefaultSettingList)
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
            EnvironmentHelper.StartUpdate();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}