using System;
using System.Diagnostics;
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
        private const string RegistryKeyGame = "Software\\illusion\\PlayHome";
        private const string RegistryKeyStudio = "Software\\illusion\\PlayHomeStudio";
        private const string RegistryKeyVR = "Software\\illusion\\VR Gedo";
        private const string ExecutableGame = "PlayHome64bit.exe";
        private const string ExecutableGame32 = "PlayHome32bit.exe";
        private const string ExecutableStudio = "PlayHomeStudio64bit.exe";
        private const string ExecutableStudio32 = "PlayHomeStudio32bit.exe";
        private const string ExecutableVR = "VR GEDOU.exe";
        private const string SupportDiscord = "https://discord.gg/F3bDEFE";
        // Languages built into the game itself
        private static readonly string[] _builtinLanguages = {"ja-JP"};
        private readonly bool _mainGameExists;
        private readonly bool _studioExists;
        private bool _is32;

        // Normal fields, don't fill in --------------------------------------------------------------
        private bool _suppressEvents;

        public MainWindow()
        {
            try
            {
                _suppressEvents = true;

                // Initialize code -------------------------------------
                EnvironmentHelper.Initialize(_builtinLanguages);

                _mainGameExists = File.Exists(EnvironmentHelper.GameRootDirectory + ExecutableGame);
                _studioExists = File.Exists(EnvironmentHelper.GameRootDirectory + ExecutableStudio);

                if (_studioExists)
                    SettingManager.Initialize(EnvironmentHelper.GetConfigFilePath(), RegistryKeyGame,
                        RegistryKeyStudio, RegistryKeyVR);
                else
                    SettingManager.Initialize(EnvironmentHelper.GetConfigFilePath(), RegistryKeyGame);

                SettingManager.LoadSettings();


                // Initialize interface --------------------------------
                InitializeComponent();

                var tooltip = new System.Windows.Controls.ToolTip
                {
                    Content = Localizable.TooltipBoneMod

                };

                toggleBoneMod.ToolTip = tooltip;

                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                CustomRes.Visibility = Visibility.Hidden;

                if (string.IsNullOrEmpty((string) labelTranslated.Content))
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

                PlayHomeStartup();

                PluginToggleManager.CreatePluginToggles(Toggleables);

                _suppressEvents = false;

                UpdateDisplaySettings(SettingManager.CurrentSettings.FullScreen);

                Closed += (sender, args) => SettingManager.SaveSettings();
                MouseDown += (sender, args) =>
                {
                    if (args.ChangedButton == MouseButton.Left) DragMove();
                };
                buttonClose.Click += (sender, args) => Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    "Failed to start the launcher, please consider reporting this error to the developers.\n\nError that caused the crash: " +
                    e, "Launcher crash", MessageBoxButtons.OK, MessageBoxIcon.Error);
                File.WriteAllText(Path.Combine(EnvironmentHelper.GameRootDirectory, "launcher_crash.log"),
                    e.ToString());
                Close();
            }
        }

        #region PlayHome Speciffic code

        private void toggle32_Checked(object sender, RoutedEventArgs e)
        {
            _is32 = true;
            if (File.Exists($"{EnvironmentHelper.GameRootDirectory}/UserData/LauncherEN/toggle32.txt")) return;
            using (var writetext = new StreamWriter("/UserData/LauncherEN/toggle32.txt"))
            {
                writetext.WriteLine("x86");
            }
        }

        private void toggle32_Unchecked(object sender, RoutedEventArgs e)
        {
            _is32 = false;
            if (File.Exists($"{EnvironmentHelper.GameRootDirectory}/UserData/LauncherEN/toggle32.txt"))
                File.Delete($"{EnvironmentHelper.GameRootDirectory}/UserData/LauncherEN/toggle32.txt");
        }

        private void HoneyPotInspector_Run(object sender, RoutedEventArgs e)
        {
            if (File.Exists($"{EnvironmentHelper.GameRootDirectory}\\HoneyPot\\HoneyPotInspector.exe"))
                Process.Start($"{EnvironmentHelper.GameRootDirectory}\\HoneyPot\\HoneyPotInspector.exe");
        }

        private void bonemod_Checked(object sender, RoutedEventArgs e)
        {
            if (!_suppressEvents)
            {
                if (File.Exists(
                        $"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\modfiles\\resources.assets-bone") &&
                    File.Exists(
                        $"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\modfiles\\resources.assets-vanilla")
                )
                    if (File.Exists(
                        $"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\modfiles\\vanillaactive.txt"))
                    {
                        if (File.Exists($"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\resources.assets"))
                            File.Delete($"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\resources.assets");
                        File.Copy(
                            $"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\modfiles\\resources.assets-bone",
                            $"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\resources.assets");

                        File.Delete(
                            $"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\modfiles\\vanillaactive.txt");

                        using (var writetext =
                            new StreamWriter(
                                $"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\modfiles\\boneactive.txt"))
                        {
                            writetext.WriteLine("enabled");
                        }
                    }

                if (File.Exists(
                        $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\modfiles\\resources.assets-bone") &&
                    File.Exists(
                        $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\modfiles\\resources.assets-vanilla")
                )
                    if (File.Exists(
                        $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\modfiles\\vanillaactive.txt")
                    )
                    {
                        if (File.Exists(
                            $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\resources.assets"))
                            File.Delete(
                                $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\resources.assets");
                        File.Copy(
                            $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\modfiles\\resources.assets-bone",
                            $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\resources.assets");

                        File.Delete(
                            $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\modfiles\\vanillaactive.txt");

                        using (var writetext =
                            new StreamWriter(
                                $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\modfiles\\boneactive.txt")
                        )
                        {
                            writetext.WriteLine("enabled");
                        }
                    }
            }
        }

        private void bonemod_Unchecked(object sender, RoutedEventArgs e)
        {
            if (File.Exists(
                $"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\modfiles\\resources.assets-vanilla"))
                if (!File.Exists(
                    $"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\modfiles\\vanillaactive.txt"))
                {
                    if (File.Exists($"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\resources.assets"))
                        File.Delete($"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\resources.assets");
                    File.Copy(
                        $"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\modfiles\\resources.assets-vanilla",
                        $"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\resources.assets");

                    File.Delete($"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\modfiles\\boneactive.txt");

                    using (var writetext =
                        new StreamWriter(
                            $"{EnvironmentHelper.GameRootDirectory}\\PlayHome64bit_Data\\modfiles\\vanillaactive.txt"))
                    {
                        writetext.WriteLine("enabled");
                    }
                }

            if (File.Exists(
                $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\modfiles\\resources.assets-vanilla"))
                if (!File.Exists(
                    $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\modfiles\\vanillaactive.txt"))
                {
                    if (File.Exists(
                        $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\resources.assets"))
                        File.Delete(
                            $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\resources.assets");
                    File.Copy(
                        $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\modfiles\\resources.assets-vanilla",
                        $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\resources.assets");

                    File.Delete(
                        $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\modfiles\\boneactive.txt");

                    using (var writetext =
                        new StreamWriter(
                            $"{EnvironmentHelper.GameRootDirectory}\\PlayHomeStudio64bit_Data\\modfiles\\vanillaactive.txt")
                    )
                    {
                        writetext.WriteLine("enabled");
                    }
                }
        }

        private void PlayHomeStartup()
        {
            if (!File.Exists($"{EnvironmentHelper.GameRootDirectory}/{ExecutableGame32}"))
                toggle32.Visibility = Visibility.Collapsed;
            if (File.Exists($"{EnvironmentHelper.GameRootDirectory}/UserData/LauncherEN/toggle32.txt") &&
                File.Exists($"{EnvironmentHelper.GameRootDirectory}/{ExecutableGame32}"))
                toggle32.IsChecked = true;
            if (!File.Exists($"{EnvironmentHelper.GameRootDirectory}HoneyPot\\HoneyPotInspector.exe"))
                HPIButton.Visibility = Visibility.Collapsed;
            if (!File.Exists(
                    $"{EnvironmentHelper.GameRootDirectory}PlayHome64bit_Data\\modfiles\\resources.assets-bone") ||
                !File.Exists(
                    $"{EnvironmentHelper.GameRootDirectory}PlayHome64bit_Data\\modfiles\\resources.assets-vanilla"))
                Toggleables.Children.Remove(toggleBoneMod);
        }

        #endregion

        #region Display settings

        private void ResolutionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (-1 == dropRes.SelectedIndex) return;

            var comboBoxCustomItem = (ComboBoxCustomItem) dropRes.SelectedItem;
            SettingManager.CurrentSettings.Size = comboBoxCustomItem.text;
            SettingManager.CurrentSettings.Width = comboBoxCustomItem.width;
            SettingManager.CurrentSettings.Height = comboBoxCustomItem.height;

            if (!_suppressEvents) EnvironmentHelper.WarnRes(comboBoxCustomItem.text);
        }

        private void QualityChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingManager.CurrentSettings.Quality = dropQual.SelectedIndex;
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

            SettingManager.CurrentSettings.Display = dropDisplay.SelectedIndex;
            UpdateDisplaySettings(SettingManager.CurrentSettings.FullScreen);
        }

        private void UpdateDisplaySettings(bool bFullScreen)
        {
            if (_suppressEvents) return;
            _suppressEvents = true;

            toggleFullscreen.IsChecked = bFullScreen;
            if (!SettingManager.SetFullScreen(bFullScreen))
            {
                toggleFullscreen.IsChecked = false;
                MessageBox.Show("This monitor doesn't support fullscreen.");
            }

            dropRes.Items.Clear();
            foreach (var displayMode in SettingManager.GetCurrentDisplayModes())
            {
                var newItem = new ComboBoxCustomItem
                {
                    text = displayMode.text,
                    width = displayMode.Width,
                    height = displayMode.Height
                };
                dropRes.Items.Add(newItem);
            }

            dropRes.Text = SettingManager.CurrentSettings.Size;

            dropDisplay.SelectedIndex = SettingManager.CurrentSettings.Display;
            dropQual.SelectedIndex =
                Math.Max(Math.Min(SettingManager.CurrentSettings.Quality, dropQual.Items.Count), 0);

            _suppressEvents = false;
        }

        #endregion

        #region Start game buttons and manuals

        private void StartGame(string strExe)
        {
            SettingManager.SaveSettings();
            if (EnvironmentHelper.StartGame(strExe))
                Close();
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            if (_is32)
                StartGame(ExecutableGame32);
            else
                StartGame(ExecutableGame);
        }

        private void buttonStartS_Click(object sender, RoutedEventArgs e)
        {
            if (_is32)
                StartGame(ExecutableStudio32);
            else
                StartGame(ExecutableStudio);
        }

        private void buttonStartV_Click(object sender, RoutedEventArgs e)
        {
            StartGame(ExecutableVR);
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