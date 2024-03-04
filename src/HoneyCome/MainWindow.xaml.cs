﻿using System;
using System.Drawing;
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
        private const string RegistryKeyGame = "Software\\ILLGAMES\\HoneyCome";
        private const string RegistryKeyStudio = "Software\\ILLGAMES\\DigitalCraft";
        private string ExecutableGame = "HoneyCome.exe";
        private const string ExecutableStudio = "DigitalCraft\\DigitalCraft.exe";
        private const string ExecutableVR = "HoneyVR.exe";
        private const string SupportDiscord = "https://discord.gg/hevygx6";
        // Languages built into the game itself
        private static readonly string[] _builtinLanguages = { "ja-JP", "en-US" };

        // Normal fields, don't fill in --------------------------------------------------------------
        private bool _suppressEvents;
        private readonly bool _mainGameExists;
        private readonly bool _studioExists;
        private readonly bool _vrExists;
        private readonly bool _userDataExists;

        public MainWindow()
        {
            try
            {
                _suppressEvents = true;

                if (!File.Exists(EnvironmentHelper.GameRootDirectory + ExecutableGame))
                    ExecutableGame = "HoneyComeccp.exe";

                // Initialize code -------------------------------------
                EnvironmentHelper.Initialize(_builtinLanguages);

                _mainGameExists = File.Exists(EnvironmentHelper.GameRootDirectory + ExecutableGame);
                _studioExists = File.Exists(EnvironmentHelper.GameRootDirectory + ExecutableStudio);
                _vrExists = File.Exists(EnvironmentHelper.GameRootDirectory + ExecutableVR);
                _userDataExists = Directory.Exists(EnvironmentHelper.GameRootDirectory + "UserData");

                SettingManager.Initialize(new SettingManagerNew(configFilePath: Path.Combine(EnvironmentHelper.GameRootDirectory, "UserData/config.xml"),
                                                                setupFilePath: Path.Combine(EnvironmentHelper.GameRootDirectory, "UserData/setup.xml"),
                                                                registryConfigPaths: _studioExists ? new[] { RegistryKeyGame, RegistryKeyStudio } : new[] { RegistryKeyGame }));

                // Initialize interface --------------------------------
                InitializeComponent();

                if (_mainGameExists && !_userDataExists)
                {
                    createUserData();
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

                if (!_studioExists)
                {
                    buttonStartS.Visibility = Visibility.Collapsed;
                    idmage.Visibility = Visibility.Collapsed;
                    labelStartS.Visibility = Visibility.Collapsed;
                    StudiBtnBG.Visibility = Visibility.Collapsed;
                    SMBtnBG.Visibility = Visibility.Collapsed;
                    labelMS.Visibility = Visibility.Collapsed;
                    buttonManualS.Visibility = Visibility.Collapsed;
                    buttonScenes.Visibility = Visibility.Collapsed;
                }

                if (!_vrExists)
                {
                    // TODO: Add VR hide
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

            if (_studioExists)
            {
                Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\Studio\\scene");
                Directory.CreateDirectory(EnvironmentHelper.GameRootDirectory + "UserData\\Studio\\pose");
            }

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
            EnvironmentHelper.ShowManual($"{EnvironmentHelper.GameRootDirectory}\\manual_v\\");
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