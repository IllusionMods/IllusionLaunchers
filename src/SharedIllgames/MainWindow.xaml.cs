using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace InitSetting
{
    public partial class MainWindow : Window
    {
        // Normal fields, don't fill in --------------------------------------------------------------
        private bool _suppressEvents;
        private readonly string _studioPath;

        public MainWindow()
        {
            try
            {
                _suppressEvents = true;

                // Initialize code -------------------------------------
                EnvironmentHelper.Initialize(_builtinLanguages);

                var mainGameExists = File.Exists(EnvironmentHelper.GameRootDirectory + ExecutableGame);
                _studioPath = Utils.FindDigitalCraftPath();
                var studioExists = !string.IsNullOrEmpty(_studioPath);
                var vrExists = File.Exists(EnvironmentHelper.GameRootDirectory + ExecutableVR);
                var userDataExists = Directory.Exists(EnvironmentHelper.GameRootDirectory + "UserData");

                SettingManager.Initialize(new SettingManagerNew(configFilePath: Path.Combine(EnvironmentHelper.GameRootDirectory, "UserData/config.xml"),
                                                                setupFilePath: Path.Combine(EnvironmentHelper.GameRootDirectory, "UserData/setup.xml"),
                                                                registryConfigPaths: studioExists ? new[] { RegistryKeyGame, RegistryKeyStudio } : new[] { RegistryKeyGame }));

                // Initialize interface --------------------------------
                InitializeComponent();

                if (!mainGameExists) buttonGameStart.Visibility = Visibility.Collapsed;
                if (!studioExists) buttonStudioStart.Visibility = Visibility.Collapsed;
                if (!vrExists) buttonVrStart.Visibility = Visibility.Collapsed;

                if (mainGameExists && !userDataExists)
                {
                    createUserData();
                }

                if (!mainGameExists && studioExists)
                {
                    appBG.ImageSource = new BitmapImage(new Uri("pack://application:,,,/InitSetting;component/Images/DC-Background.png", UriKind.Absolute));
                    Image_Logo.Source = new BitmapImage(new Uri("Images/DC-Logo.png", UriKind.RelativeOrAbsolute));
                    Image_PackChara.Source = null;
                }

                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                CustomRes.Visibility = Visibility.Hidden;

                if (string.IsNullOrEmpty((string)labelTranslated.Content))
                {
                    labelTranslated.Visibility = Visibility.Hidden;
                    labelTranslatedBorder.Visibility = Visibility.Hidden;
                }

                if (!EnvironmentHelper.KKmanExist)
                {
                    gridUpdate.Visibility = Visibility.Hidden;
                    gridManager.Visibility = Visibility.Hidden;
                }

                // Launcher Customization: Defining Warning, background and character
                if (!string.IsNullOrEmpty(EnvironmentHelper.VersionString))
                    labelDist.Content = EnvironmentHelper.VersionString;

                if (!string.IsNullOrEmpty(EnvironmentHelper.WarningString))
                    warningText.Text = EnvironmentHelper.WarningString;

                if (EnvironmentHelper.CustomCharacterImage != null)
                    Image_PackChara.Source = EnvironmentHelper.CustomCharacterImage;
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

                CheckAndHideStudioWarnings();

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

        #region UserData Creation

        private void createUserData()
        {
            Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\bg");
            Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\cap");
            Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\cardframe");
            Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\chara\\female");
            Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\chara\\male");
            Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\chara\\navi");
            Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\navi");
            Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\coordinate\\female");
            Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\coordinate\\male");
            Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\custom");
            Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\save\\game");
            Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\ScreenEffect\\preset");
            Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\system");

            //if (_studioExists)
            //{
            //    Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\Studio\\scene");
            //    Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\Studio\\pose");
            //}

        }

        #endregion

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
            StartGame(_studioPath);
        }

        private void buttonStartV_Click(object sender, RoutedEventArgs e)
        {
            StartGame(ExecutableVR);
        }

        private void buttonManual_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.ShowManual("manual", ManualUrlGame);
        }

        private void buttonManualS_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.ShowManual("manual_s", ManualUrlStudio);
        }

        private void buttonManualV_Click(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.ShowManual("manual_v", ManualUrlVr);
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

        #region Studio warnings

        private void CheckAndHideStudioWarnings()
        {
            studioWarn_no_HC.Visibility = Visibility.Visible;
            studioWarn_no_SVS.Visibility = Visibility.Visible;
            studioWarn_no_AC.Visibility = Visibility.Visible;

            studioWarn_outd_HC.Visibility = Visibility.Collapsed;
            studioWarn_outd_SVS.Visibility = Visibility.Collapsed;
            studioWarn_outd_AC.Visibility = Visibility.Collapsed;

            studioWarn_outdated.Visibility = CheckVersion(_studioPath + "/../../DefaultData/craft/version.dat", new Version(3, 1, 0)) ? Visibility.Collapsed : Visibility.Visible;

            using (var hcRegistryKey = Registry.CurrentUser.OpenSubKey(@"Software\ILLGAMES\HoneyCome"))
            {
                var hcInstallDir = hcRegistryKey?.GetValue("INSTALLDIR")?.ToString();
                if (!string.IsNullOrEmpty(hcInstallDir) && File.Exists(hcInstallDir + "/abdata/add010_00"))
                {
                    studioWarn_no_HC.Visibility = Visibility.Collapsed;
                    if (!CheckVersion(hcInstallDir + "/DefaultData/system/version.dat", new Version(2, 0, 7)))
                        studioWarn_outd_HC.Visibility = Visibility.Visible;
                }
            }

            using (var svsRegistryKey = Registry.CurrentUser.OpenSubKey(@"Software\ILLGAMES\SamabakeScramble"))
            {
                var svsInstallDir = svsRegistryKey?.GetValue("INSTALLDIR")?.ToString();
                if (!string.IsNullOrEmpty(svsInstallDir) && Directory.Exists(svsInstallDir))
                {
                    studioWarn_no_SVS.Visibility = Visibility.Collapsed;
                    if (!CheckVersion(svsInstallDir + "/DefaultData/system/version.dat", new Version(1, 1, 6)))
                        studioWarn_outd_SVS.Visibility = Visibility.Visible;
                }
            }

            using (var acRegistryKey = Registry.CurrentUser.OpenSubKey(@"Software\ILLGAMES\Aicomi"))
            {
                var acInstallDir = acRegistryKey?.GetValue("INSTALLDIR")?.ToString();
                if (!string.IsNullOrEmpty(acInstallDir) && Directory.Exists(acInstallDir))
                {
                    studioWarn_no_AC.Visibility = Visibility.Collapsed;
                    if (!CheckVersion(acInstallDir + "/DefaultData/system/version.dat", new Version(2, 0, 5)))
                        studioWarn_outd_AC.Visibility = Visibility.Visible;
                }
            }
        }

        private static bool CheckVersion(string versionFilePath, Version minVersion)
        {
            try
            {
                if (File.Exists(versionFilePath))
                {
                    var versionText = File.ReadAllText(versionFilePath).Trim();
                    var version = new Version(versionText);

                    if (version >= minVersion)
                        return true;
                }
            }
            catch
            {
                // If version parsing fails, keep the warning visible
            }

            return false;
        }

        private void ShowRegKeyMissingMessage(string gameName, string filter, string regKey)
        {
            try
            {
                if (MessageBox.Show($"{gameName}'s registry key is missing and because of that will not be detected by DigitalCraft. Do you want to fix this by selecting the game's executable?",
                                    "Registry Key Missing", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
                {
                    using (var openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.Filter = filter;
                        openFileDialog.Title = $"Select {gameName}'s executable";

                        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            var exePath = openFileDialog.FileName;
                            var installDir = Path.GetDirectoryName(exePath) ?? throw new ArgumentException(exePath + " is invalid");

                            using (var registryKey = Registry.CurrentUser.CreateSubKey(regKey))
                            {
                                if (registryKey != null)
                                {
                                    registryKey.SetValue("INSTALLDIR", installDir);
                                    MessageBox.Show("Registry key successfully created. DigitalCraft should now detect this game.\n\nWarning: You may still need to update the game before it is detected! You can see if DigitalCraft detected the game on DC's title screen in top right corner, all of the detected games and expansions are listed there.",
                                                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    CheckAndHideStudioWarnings();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Failed to set registry key: " + exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void buttonWarn_HC_Click(object sender, RoutedEventArgs e) =>
            ShowRegKeyMissingMessage("HoneyCome Dolce", "HoneyCome Executable|HoneyCome.exe;HoneyComeccp.exe", @"Software\ILLGAMES\HoneyCome");
        private void buttonWarn_SVS_Click(object sender, RoutedEventArgs e) =>
            ShowRegKeyMissingMessage("SamabakeScramble", "SamabakeScramble Executable|SamabakeScramble.exe", @"Software\ILLGAMES\SamabakeScramble");
        private void buttonWarn_AC_Click(object sender, RoutedEventArgs e) =>
            ShowRegKeyMissingMessage("Aicomi", "Aicomi Executable|Aicomi.exe", @"Software\ILLGAMES\Aicomi");

        private static void ShowOutdatedMessage(string message, string url)
        {
            try
            {
                if (MessageBox.Show(message + "\n\nDo you want to go to the official website to look for an update?",
                                    "DigitalCraft warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        private void buttonWarn_outdated_Click(object sender, RoutedEventArgs e) =>
            ShowOutdatedMessage("Content from some of the games might not appear in DigitalCraft because DigitalCraft is outdated.", "https://www.illgames.jp/product/digitalcraft_plain/");
        private void buttonWarn_HCout_Click(object sender, RoutedEventArgs e) =>
            ShowOutdatedMessage("Content from HoneyCome Dolce might not appear in DigitalCraft because HoneyCome Dolce is outdated.", "https://www.illgames.jp/product/honeycome_dolce/download.php");
        private void buttonWarn_SVSout_Click(object sender, RoutedEventArgs e) =>
            ShowOutdatedMessage("Content from SamabakeScramble might not appear in DigitalCraft because SamabakeScramble is outdated.", "https://www.illgames.jp/product/svs/download-add/");
        private void buttonWarn_ACout_Click(object sender, RoutedEventArgs e) =>
            ShowOutdatedMessage("Content from Aicomi might not appear in DigitalCraft because Aicomi is outdated.", "https://www.illgames.jp/product/aicomi/download.php");

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