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

        private static string RegistryKeyGame = "Software\\illusion\\AI-Shoujo\\AI-Shoujo\\";
        private static string RegistryKeyStudio = "Software\\illusion\\AI-Syoujyo\\StudioNEOV2";
        private string ExecutableGame = "AI-Shoujo.exe";
        private static string ExecutableStudio = "StudioNEOV2.exe";
        private static string SupportDiscord = "https://discord.gg/illusionsoft";
        // Languages built into the game itself
        private static readonly string[] _builtinLanguages = { "ja-JP","en-US","zh-CN","zh-TW" };
        private static readonly string[] _builtinLanguagesConvert = { "ja-JP" };

        // Normal fields, don't fill in --------------------------------------------------------------
        private bool _suppressEvents;
        private readonly bool _mainGameExists;
        private readonly bool _studioExists;

        public MainWindow()
        {
            

            try
            {
                _suppressEvents = true;

                if (!File.Exists(EnvironmentHelper.GameRootDirectory + ExecutableGame))
                    ExecutableGame = "AI-Syoujyo.exe";

                // Initialize code -------------------------------------
                string tempgamedir =
                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                bool isGameConverted = File.Exists($"{tempgamedir}/abdata/BRConvert/OK.txt");
                
                if (isGameConverted)
                {
                    RegistryKeyGame = "Software\\illusion\\AI-Syoujyo\\AI-Syoujyo\\";
                    RegistryKeyStudio = "Software\\illusion\\AI-Syoujyo\\StudioNEOV2";
                    ExecutableGame = "AI-Syoujyo.exe";
                }

                EnvironmentHelper.Initialize(!isGameConverted ? _builtinLanguages : _builtinLanguagesConvert);

                _mainGameExists = File.Exists(EnvironmentHelper.GameRootDirectory + ExecutableGame);
                _studioExists = File.Exists(EnvironmentHelper.GameRootDirectory + ExecutableStudio);

                SettingManager.Initialize(new SettingManagerOld(configFilePath: Path.Combine(EnvironmentHelper.GameRootDirectory, "UserData/setup.xml"),
                                                                registryConfigPaths: _studioExists ? new[] { RegistryKeyGame, RegistryKeyStudio } : new[] { RegistryKeyGame }));

                // Initialize interface --------------------------------
                InitializeComponent();

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

                var primaryDisplay = Localizable.PrimaryDisplay;
                var subDisplay = Localizable.SubDisplay;
                for (var i = 0; i < Screen.AllScreens.Length; i++)
                {
                    // 0 is primary
                    var newItem = i == 0 ? primaryDisplay : $"{subDisplay} : " + i;
                    dropDisplay.Items.Add(newItem);
                }

                PluginToggleManager.CreatePluginToggles(Toggleables);

                _suppressEvents = false;

                UpdateDisplaySettings(SettingManager.Current.CurrentSettings.FullScreen);

                Closed += (sender, args) => SettingManager.Current.SaveSettings();
                MouseDown += (sender, args) => { if (args.ChangedButton == MouseButton.Left) DragMove(); };
                buttonClose.Click += (sender, args) => Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to start the launcher, please consider reporting this error to the developers.\n\nError that caused the crash: " + e, "Launcher crash", MessageBoxButtons.OK, MessageBoxIcon.Error);
                File.WriteAllText(Path.Combine(EnvironmentHelper.GameRootDirectory, "launcher_crash.log"), e.ToString());
                Close();
            }
        }

        #region Display settings

        private void ResolutionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (-1 == dropRes.SelectedIndex) return;

            var comboBoxCustomItem = (ComboBoxCustomItem)dropRes.SelectedItem;
            SettingManager.Current.CurrentSettings.Size = comboBoxCustomItem.text;
            SettingManager.Current.CurrentSettings.Width = comboBoxCustomItem.width;
            SettingManager.Current.CurrentSettings.Height = comboBoxCustomItem.height;

            if (!_suppressEvents) EnvironmentHelper.WarnRes(comboBoxCustomItem.text);
        }

        private void QualityChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingManager.Current.CurrentSettings.Quality = dropQual.SelectedIndex;
        }

        private void FullscreenUnChecked(object sender, RoutedEventArgs e)
        {
            UpdateDisplaySettings(false);
        }

        private void FullscreenChecked(object sender, RoutedEventArgs e)
        {
            UpdateDisplaySettings(true);
        }

        private void DisplayChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dropDisplay.SelectedIndex < 0) return;

            SettingManager.Current.CurrentSettings.Display = dropDisplay.SelectedIndex;
            UpdateDisplaySettings(SettingManager.Current.CurrentSettings.FullScreen);
        }

        private void UpdateDisplaySettings(bool bFullScreen)
        {
            if (_suppressEvents) return;
            _suppressEvents = true;

            toggleFullscreen.IsChecked = bFullScreen;
            if (!SettingManager.Current.SetFullScreen(bFullScreen))
            {
                toggleFullscreen.IsChecked = false;
                MessageBox.Show("This monitor doesn't support fullscreen.");
            }

            dropRes.Items.Clear();
            foreach (var displayMode in SettingManager.Current.GetCurrentDisplayModes())
            {
                var newItem = new ComboBoxCustomItem
                {
                    text = displayMode.text,
                    width = displayMode.Width,
                    height = displayMode.Height
                };
                dropRes.Items.Add(newItem);
            }

            dropRes.Text = SettingManager.Current.CurrentSettings.Size;

            dropDisplay.SelectedIndex = SettingManager.Current.CurrentSettings.Display;
            dropQual.SelectedIndex = Math.Max(Math.Min(SettingManager.Current.CurrentSettings.Quality, dropQual.Items.Count), 0);

            _suppressEvents = false;
        }

        #endregion

        #region Start game buttons and manuals

        private void StartGame(string strExe)
        {
            SettingManager.Current.SaveSettings();
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

        private void buttonManual_Click(object sender, RoutedEventArgs e)
        {
            switch (EnvironmentHelper.Language.ToString())
            {
                case "en":
                    EnvironmentHelper.ShowManual($"{EnvironmentHelper.GameRootDirectory}\\manual\\English\\");
                    break;
                case "zh-CN":
                    EnvironmentHelper.ShowManual($"{EnvironmentHelper.GameRootDirectory}\\manual\\Simplified Chinese\\");
                    break;
                case "zh-TW":
                    EnvironmentHelper.ShowManual($"{EnvironmentHelper.GameRootDirectory}\\manual\\Traditional Chinese\\");
                    break;
                case "ja-JP":
                    EnvironmentHelper.ShowManual($"{EnvironmentHelper.GameRootDirectory}\\manual\\Japanese\\");
                    break;
                default:
                    EnvironmentHelper.ShowManual($"{EnvironmentHelper.GameRootDirectory}\\manual\\");
                    break;
            }
        }

        private void buttonManualS_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.ShowManual($"{EnvironmentHelper.GameRootDirectory}\\manual_s\\");
        }

        private void buttonManualV_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.ShowManual($"{EnvironmentHelper.GameRootDirectory}\\manual_vr\\");
        }

        #endregion

        #region Discord button block

        private void discord_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.StartProcess(SupportDiscord);
        }

        private void patreon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.StartProcess(EnvironmentHelper.PatreonUrl);
        }

        private void update_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.StartUpdate();
        }

        private void manager_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.StartManager();
        }

        #endregion

        #region Language buttons

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

        private void LangChineseTW(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.SetLanguage("zh-TW");
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

        private void LangRussian(object sender, MouseButtonEventArgs e)
        {
            EnvironmentHelper.SetLanguage("ru-RU");
        }

        #endregion

        #region Directory open buttons

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

        #endregion
    }
}