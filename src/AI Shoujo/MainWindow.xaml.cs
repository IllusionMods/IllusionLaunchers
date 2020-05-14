using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Windows.Input;

namespace InitSetting
{
    public partial class MainWindow : Window
    {
        private const string m_strGameRegistry = "Software\\illusion\\AI-Syoujyo\\AI-Syoujyo\\";
        private const string m_strStudioRegistry = "Software\\illusion\\AI-Syoujyo\\StudioNEOV2";
        private string[] m_astrQuality;

        private const string m_strGameExe = "AI-Syoujyo.exe";
        private const string m_strStudioExe = "StudioNEOV2.exe";
        private const string m_strManualDir = "/manual/お読み下さい.html";
        private const string m_strStudioManualDir = "/manual_s/お読み下さい.html";
        private const string m_strVRManualDir = "/manual_vr/お読み下さい.html";

        private bool isStudio;
        private bool isMainGame;

        public SettingManager SettingManager;

        public MainWindow()
        {
            InitializeComponent();
            //if (!DoubleStartCheck())
            //{
            //    System.Windows.Application.Current.MainWindow.Close();
            //    return;
            //}

            startup = true;

            SettingManager = new SettingManager();

            EnvironmentHelper.Initialize();

            EnvironmentHelper.CheckDuplicateStartup();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            CustomRes.Visibility = Visibility.Hidden;
            if (!EnvironmentHelper.kkmanExist) gridUpdate.Visibility = Visibility.Hidden;

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

            // Mod settings
            if (File.Exists($"{EnvironmentHelper.GameRootDirectory}\\BepInEx\\Plugins\\DHH_AI4.dll"))
            {
                toggleDHH.IsChecked = true;
            }
            if (!File.Exists($"{EnvironmentHelper.GameRootDirectory}\\BepInEx\\Plugins\\DHH_AI4.dl_") && !File.Exists($"{EnvironmentHelper.GameRootDirectory}\\BepInEx\\Plugins\\DHH_AI4.dll"))
            {
                toggleDHH.IsEnabled = false;
            }
            if (File.Exists($"{EnvironmentHelper.GameRootDirectory}\\BepInEx\\Plugins\\AIGraphics\\AI_Graphics.dll"))
            {
                toggleAIGraphics.IsChecked = true;
            }
            if (!File.Exists($"{EnvironmentHelper.GameRootDirectory}\\BepInEx\\Plugins\\AIGraphics\\AI_Graphics.dl_") && !File.Exists($"{EnvironmentHelper.GameRootDirectory}\\BepInEx\\Plugins\\AIGraphics\\AI_Graphics.dll"))
            {
                toggleAIGraphics.IsEnabled = false;
            }
            if (File.Exists($"{EnvironmentHelper.GameRootDirectory}\\BepInEx\\Plugins\\AIGraphics\\AI_Graphics.dll") && File.Exists($"{EnvironmentHelper.GameRootDirectory}\\BepInEx\\Plugins\\DHH_AI4.dll"))
            {
                toggleDHH.IsChecked = false;
                toggleAIGraphics.IsChecked = false;
            }

            startup = false;

            SetupUiLanguage();

            // Launcher Customization: Defining Warning, background and character

            if (!string.IsNullOrEmpty(EnvironmentHelper.VersionString))
                labelDist.Content = EnvironmentHelper.VersionString;

            isStudio = File.Exists(EnvironmentHelper.GameRootDirectory + m_strStudioExe);
            isMainGame = File.Exists(EnvironmentHelper.GameRootDirectory + m_strGameExe);

            if (!string.IsNullOrEmpty(EnvironmentHelper.WarningString))
                warningText.Text = EnvironmentHelper.WarningString;

            if (EnvironmentHelper.CustomCharacterImage != null)
                PackChara.Source = EnvironmentHelper.CustomCharacterImage;
            if (EnvironmentHelper.CustomBgImage != null)
                appBG.ImageSource = EnvironmentHelper.CustomBgImage;

            if (string.IsNullOrEmpty(EnvironmentHelper.patreonURL))
            {
                linkPatreon.Visibility = Visibility.Collapsed;
                patreonBorder.Visibility = Visibility.Collapsed;
                patreonIMG.Visibility = Visibility.Collapsed;
            }

            var num = Screen.AllScreens.Length;
            if (num == 2)
            {
                dropDisplay.Items.Add(s_primarydisplay);
                dropDisplay.Items.Add($"{s_subdisplay} : 1");
            }
            else
            {
                for (int i = 0; i < num; i++)
                {
                    string newItem = (i == 0) ? s_primarydisplay : ($"{s_subdisplay} : " + i);
                    dropDisplay.Items.Add(newItem);
                }
            }
            foreach (string newItem2 in m_astrQuality)
            {
                dropQual.Items.Add(newItem2);
            }

            string configFilePath = EnvironmentHelper.GetConfigFilePath();
            CheckConfigFile:
            if (File.Exists(configFilePath))
            {
                try
                {
                    SettingManager.LoadSettings(configFilePath);

                    SettingManager.m_Setting.m_nDisplay = Math.Min(SettingManager.m_Setting.m_nDisplay, num - 1);
                    setDisplayComboBox(SettingManager.m_Setting.m_bFullScreen);
                    var flag = false;
                    for (var k = 0; k < dropRes.Items.Count; k++)
                    {
                        if (dropRes.Items[k].ToString() == SettingManager.m_Setting.m_strSizeChoose)
                            flag = true;
                    }
                    dropRes.Text = flag ? SettingManager.m_Setting.m_strSizeChoose : "1280 x 720 (16 : 9)";
                    toggleFullscreen.IsChecked = SettingManager.m_Setting.m_bFullScreen;
                    dropQual.Text = m_astrQuality[SettingManager.m_Setting.m_nQualityChoose];
                    string text = SettingManager.m_Setting.m_nDisplay == 0 ? s_primarydisplay : $"{s_subdisplay} : " + SettingManager.m_Setting.m_nDisplay;
                    if (num == 2)
                    {
                        text = new[]
                        {
                        s_primarydisplay,
                        $"{s_subdisplay} : 1"
                        }[SettingManager.m_Setting.m_nDisplay];
                    }
                    if (dropDisplay.Items.Contains(text))
                        dropDisplay.Text = text;
                    else
                    {
                        dropDisplay.Text = s_primarydisplay;
                        SettingManager.m_Setting.m_nDisplay = 0;
                    }
                }
                catch (Exception)
                {
                    System.Windows.Forms.MessageBox.Show("/UserData/setup.xml file was corrupted, settings will be reset.");
                    File.Delete(configFilePath);
                    goto CheckConfigFile;
                }
            }
            else
            {
                setDisplayComboBox(false);
                dropRes.Text = SettingManager.m_Setting.m_strSizeChoose;
                dropQual.Text = m_astrQuality[SettingManager.m_Setting.m_nQualityChoose];
                dropDisplay.Text = s_primarydisplay;
            }
        }

        private void SetupUiLanguage()
        {
            labelTranslated.Visibility = Visibility.Hidden;
            labelTranslatedBorder.Visibility = Visibility.Hidden;

            // MessageBox.Show($"Chinese is {chnActive}", "Debug");

            // Template for new translations
            //if (lang == "en-US")
            //{
            //    MainWindow.Title = "PH Launcher";
            //    warnBox.Header = "Notice!";
            //    warningText.Text = "This game is intended for adult audiences, no person under the age of 18 (or equivalent according to local law) are supposed to play or be in possession of this game.\n\nThis game contains content of a sexual nature, and some of the actions depicted within may be illegal to replicate in real life. Aka, it's all fun and games in the game, let's keep it that way shall we? (~.~)v";
            //    GameFBox.Header = "Game folders";
            //    buttonInst.Content = "Install";
            //    buttonFemaleCard.Content = "Character Cards";
            //    buttonScenes.Content = "Scenes";
            //    buttonScreenshot.Content = "ScreenShots";
            //    AISHousingDirectory.Content = "Hus";
            //    GameSBox.Header = "Game Startup";
            //    labelStart.Content = "Start PH";
            //    labelM.Content = "PH Manual";
            //    labelStartS.Content = "Start Studio";
            //    labelMS.Content = "Studio Manual";
            //    labelStartVR.Content = "Start PH VR";
            //    labelMV.Content = "VR Manual";
            //    SettingsBox.Header = "Settings";
            //    toggleFullscreen.Content = "Run Game in Fullscreen";
            //    modeDev.Content = "Developer Mode";
            //    SystemInfo.Content = "System Info";
            //    buttonClose.Content = "Exit";
            //    labelDist.Content = "Unknown Install Method";
            //    labelTranslated.Content = "Launcher translated by: <Insert Name>";
            //    translationString = "Do you want to restore Japanese language in-game?";
            //    q_performance = "Performance";
            //    q_normal = "Normal";
            //    q_quality = "Quality";
            //    s_primarydisplay = "PrimaryDisplay";
            //    s_subdisplay = "SubDisplay";
            //}

            // Translations
            if (EnvironmentHelper.lang == "ja")
            {
                labelTranslated.Visibility = Visibility.Visible;
                labelTranslatedBorder.Visibility = Visibility.Visible;

                warningText.Text =
                    "このゲームは成人向けので、18歳未満（または地域の法律によりと同等の年齢）がこのゲームをプレイまたは所有しているができない。\n\nこのゲームには性的内容の内容が含まれます。内に描かれている行動は、実生活で複製することは違法です。つまり、これは面白いゲームです、そうしましょう？(~.~)v";
                buttonInst.Content = "インストール";
                buttonFemaleCard.Content = "キャラカード (女性)";
                buttonMaleCard.Content = "キャラカード (男性)";
                buttonScenes.Content = "シーン";
                buttonScreenshot.Content = "SS";
                buttonUserData.Content = "UserData";
                labelStart.Content = "ゲーム開始";
                labelStartS.Content = "スタジオ開始";
                labelM.Content = "ゲーム";
                labelMS.Content = "スタジオ";
                toggleFullscreen.Content = "全画面表示";
                toggleConsole.Content = "コンソールを有効にする";
                labelDist.Content = "不明バージョン";
                labelTranslated.Content = "初期設定翻訳者: Earthship";
                q_performance = "パフォーマンス";
                q_normal = "ノーマル";
                q_quality = "クオリティ";
                s_primarydisplay = "メインディスプレイ";
                s_subdisplay = "サブディスプレイ";
                labelDiscord.Content = "Discordを訪問";
                labelPatreon.Content = "Patreonを訪問";
                labelUpdate.Content = "ゲームを更新する";

                // AIS Exclusive
                buttonHousing.Content = "家";
                toggleDHH.Content = "DHHを有効にする";
            }
            else if (EnvironmentHelper.lang == "zh-CN") // By @Madevil#1103 & @𝐄𝐀𝐑𝐓𝐇𝐒𝐇𝐈𝐏 💖#4313 
            {
                labelTranslated.Visibility = Visibility.Visible;
                labelTranslatedBorder.Visibility = Visibility.Visible;

                warningText.Text =
                    "此游戏适用于成人用户，任何未满18岁的人（或根据当地法律规定的同等人）都不得遊玩或拥有此游戏。\n\n这个游戏包含性相关的内容，某些行为在现实生活中可能是非法的。所以，游戏中的所有乐趣请保留在游戏中，让我们保持这种方式吧? (~.~)v";
                buttonInst.Content = "游戏主目录";
                buttonFemaleCard.Content = "人物卡 (女)";
                buttonMaleCard.Content = "人物卡 (男)";
                buttonScenes.Content = "工作室场景";
                buttonScreenshot.Content = "截图";
                buttonUserData.Content = "UserData";
                labelStart.Content = "开始游戏";
                labelStartS.Content = "开始工作室";
                labelM.Content = "游戏手册";
                labelMS.Content = "工作室手册";
                toggleFullscreen.Content = "全屏执行";
                toggleConsole.Content = "激活控制台";
                labelDist.Content = "未知版本";
                labelTranslated.Content = "翻译： Madevil & Earthship";
                q_performance = "性能";
                q_normal = "标准";
                q_quality = "高画质";
                s_primarydisplay = "主显示器";
                s_subdisplay = "次显示器";
                labelDiscord.Content = "前往Discord";
                labelPatreon.Content = "前往Patreon";
                labelUpdate.Content = "更新游戏";

                // AIS Exclusive
                buttonHousing.Content = "房子";
                toggleDHH.Content = "激活DHH";
            }
            else if (EnvironmentHelper.lang == "ko") // By @Keris-#1903 
            {
                labelTranslated.Visibility = Visibility.Visible;
                labelTranslatedBorder.Visibility = Visibility.Visible;

                warningText.Text =
                    "이게임은 성인용입니다 18세 미만의 사람(또는 법에따라 동등한사람)은 게임을 하거나 해당게임을 소유하면 안됩니다\n\n이게임에는 성적인 내용이 포함되어있으며 그안에 묘사된 행동중 일부는 실제에서 행동하면 법적인 처벌을 받습니다";
                buttonInst.Content = "설치된폴더";
                buttonFemaleCard.Content = "캐릭터 카드 (여자)";
                buttonMaleCard.Content = "캐릭터 카드 (남성)";
                buttonScenes.Content = "장면";
                buttonScreenshot.Content = "스크린샷 폴더";
                buttonUserData.Content = "UserData";
                labelStart.Content = "플레이 시작";
                labelStartS.Content = "스튜디오 시작";
                labelM.Content = "플레이 메뉴얼";
                labelMS.Content = "스튜디오 메뉴얼";
                toggleFullscreen.Content = "전체화면으로 시작";
                toggleConsole.Content = "콘솔 활성화";
                labelDist.Content = "알수 없는 설치 메소드";
                labelTranslated.Content = "런쳐 번역 by: Keris";
                q_performance = "퍼포먼스";
                q_normal = "일반";
                q_quality = "퀄리티";
                s_primarydisplay = "주 디스플레이";
                s_subdisplay = "서브 디스플레이";
                labelDiscord.Content = "Discord 방문";
                labelPatreon.Content = "Patreon 방문";
                labelUpdate.Content = "게임 업데이트";

                // AIS Exclusive
                buttonHousing.Content = "하우징 폴더";
                toggleDHH.Content = "DHH 활성화";
            }
            else if (EnvironmentHelper.lang == "es") // By @Heroine Nisa#3207
            {
                labelTranslated.Visibility = Visibility.Visible;
                labelTranslatedBorder.Visibility = Visibility.Visible;

                warningText.Text =
                    "Este juego está dirigido hacia un público adulto, ninguna persona bajo 18 años (o equivalente según las leyes locales) no deberían de jugar o estar en posesión de este juego. \n\nEste juego contiene escenas de carácter sexual, y algunas de las acciones representadas en el mismo pueden ser ilegales de hacerlas en la vida real.  También conocido como, todo es diversión y risas dentro del juego, así que mantengámoslo así, ¿vale? (~.~)v";
                buttonInst.Content = "Instalar";
                buttonFemaleCard.Content = "Cartas de Personaje (M)";
                buttonMaleCard.Content = "Cartas de Personaje (F)";
                buttonScenes.Content = "Escenas";
                buttonScreenshot.Content = "Capturas";
                buttonUserData.Content = "UserData";
                labelStart.Content = "Iniciar juego";
                labelStartS.Content = "Iniciar Studio";
                labelM.Content = "Manual de juego";
                labelMS.Content = "Manual de studio";
                toggleFullscreen.Content = "Lanzar Juego en Pantalla Completa";
                toggleConsole.Content = "Activar consola";
                labelDist.Content = "Método de Instalación Desconocido";
                labelTranslated.Content = "Traducido por: Heroine Nisa";
                q_performance = "Rendimiento";
                q_normal = "Normal";
                q_quality = "Calidad";
                s_primarydisplay = "Pantalla Primaria";
                s_subdisplay = "Pantalla Secundaria";
                labelDiscord.Content = "visita la Discord";
                labelPatreon.Content = "visita la Patreon";
                labelUpdate.Content = "Actualizar";

                // AIS Exclusive
                buttonHousing.Content = "Casas";
                toggleDHH.Content = "Activar DHH";
            }
            else if (EnvironmentHelper.lang == "pt") // By @Neptune#1989 
            {
                labelTranslated.Visibility = Visibility.Visible;
                labelTranslatedBorder.Visibility = Visibility.Visible;

                warningText.Text =
                    "Este jogo, por apresentar conteúdo adulto, é voltado para maiores de 18 anos (ou equivalente perante a lei local), menores de idade não devem jogar ou possuí-lo.\n\nAlgumas das ações presentes nessa obra de ficção podem ser ilegais ao serem realizadas no mundo real. Deixe essas coisas somente para o mundo fictício, combinado? (~.~)v";
                buttonInst.Content = "Instalar";
                buttonFemaleCard.Content = "Cards de Personagens (F)";
                buttonMaleCard.Content = "Cards de Personagens (M)";
                buttonScenes.Content = "Cenas";
                buttonScreenshot.Content = "Capturas de Tela";
                buttonUserData.Content = "UserData";
                labelStart.Content = "Iniciar Jogo";
                labelStartS.Content = "Iniciar Studio";
                labelM.Content = "Manual do Jogo";
                labelMS.Content = "Manual do Studio";
                toggleFullscreen.Content = "Iniciar Jogo em Tela Cheia";
                toggleConsole.Content = "Ativar console";
                labelDist.Content = "Método de Instalação Desconhecido";
                labelTranslated.Content = "Launcher traduzido por: Neptune";
                q_performance = "Baixo";
                q_normal = "Normal";
                q_quality = "Alto";
                s_primarydisplay = "Display Primário";
                s_subdisplay = "Display Secundário";
                labelDiscord.Content = "Visitar Discord";
                labelPatreon.Content = "Visitar Patreon";
                labelUpdate.Content = "Atualizar";

                // AIS Exclusive
                buttonHousing.Content = "Casas";
                toggleDHH.Content = "Ativar DHH";
            }
            else if (EnvironmentHelper.lang == "fr") // By VaizravaNa#2315
            {
                labelTranslated.Visibility = Visibility.Visible;
                labelTranslatedBorder.Visibility = Visibility.Visible;

                warningText.Text =
                    "Ce jeu est destiné à un public adulte, aucun mineur en dessous de 18 ans (ou l'équivalent selon les lois locales) ne doit pas jouer ou posséder ce jeu. \n\nCe jeu contient des scènes matures, et certaines actions du jeu peuvent être considéré comme illégales, à ne pas reproduire dans la vraie vie. Ce n'est que de la fiction, du moment que cela reste dans le jeu. Amusez-vous bien!";
                buttonInst.Content = "Installation";
                buttonFemaleCard.Content = "Personnages (Femme)";
                buttonMaleCard.Content = "Personnages (Mâle)";
                buttonScenes.Content = "Scènes";
                buttonScreenshot.Content = "Captures d'écran";
                buttonUserData.Content = "UserData";
                labelStart.Content = "Lancer le jeu";
                labelStartS.Content = "Lancer le Studio";
                labelM.Content = "Manuel de jeu";
                labelMS.Content = "Manuel du Studio";
                toggleFullscreen.Content = "Lancer le jeu en pleins écran";
                toggleConsole.Content = "Activer la console";
                labelDist.Content = "Méthode d'installation inconnue";
                labelTranslated.Content = "Lanceur traduit par: VaizravaNa";
                q_performance = "Performance";
                q_normal = "Normal";
                q_quality = "Qualité";
                s_primarydisplay = "Ecran principal";
                s_subdisplay = "Ecran secondaire";
                labelDiscord.Content = "Visiter la Discord";
                labelPatreon.Content = "Visiter la Patreon";
                labelUpdate.Content = "Mise à jour";

                // AIS Exclusive
                buttonHousing.Content = "Plans des maisons";
                toggleDHH.Content = "Activer DHH";
            }
            else if (EnvironmentHelper.lang == "de") // By @DONTFORGETME#6198 
            {
                labelTranslated.Visibility = Visibility.Visible;
                labelTranslatedBorder.Visibility = Visibility.Visible;

                warningText.Text =
                    "Dieses Spiel ist ausschließlich für erwachsenes Publikum vorgesehen. Niemand unter 18 Jahren ( Oder entsprechend deiner örtlichen Gesetze ) ist vorgesehen dieses Spiel zu spielen, oder es zu besitzen.\n\nDieses Spiel enthällt sexuelle Inhalte welche bei Ausführung im realen Leben strafbar sein könnten. Dinge die im Spiel geschehen sollten also auch im Spiel bleiben in Ordnung? (~.~)v";
                buttonInst.Content = "Installieren";
                buttonFemaleCard.Content = "Charakter Karten (F)";
                buttonMaleCard.Content = "Charakter Karten (M)";
                buttonScenes.Content = "Scenen";
                buttonScreenshot.Content = "ScreenShots";
                buttonUserData.Content = "UserData";
                labelStart.Content = "Starte Spiel";
                labelStartS.Content = "Starte Studio";
                labelM.Content = "Spiel Bedienungsanleitung";
                labelMS.Content = "Studio Bedienungsanleitung";
                toggleFullscreen.Content = "Starte Spiel in Vollbildmodus";
                toggleConsole.Content = "Konsole umschalten";
                labelDist.Content = "Unbekannte Installationsmethode";
                labelTranslated.Content = "Übersetzt von: <HyD>";
                q_performance = "Leistung";
                q_normal = "Normal";
                q_quality = "Qualität";
                s_primarydisplay = "Primär Bildschirm";
                s_subdisplay = "Neben Bildschrim";
                labelDiscord.Content = "Besuche die Discord";
                labelPatreon.Content = "Besuche die Patreon";
                labelUpdate.Content = "Aktualisieren";

                // AIS Exclusive
                buttonHousing.Content = "Häuser";
                toggleDHH.Content = "DHH umschalten";
            }
            else if (EnvironmentHelper.lang == "no") // By @SmokeOfC|女神様の兄様#1984
            {
                labelTranslated.Visibility = Visibility.Visible;
                labelTranslatedBorder.Visibility = Visibility.Visible;

                warningText.Text =
                    "Dette spillet er ment for voksne spillere, og ingen person under 18 år (Eller tilsvarende iht lokal lov) er tiltenkt å være i besittelse av dette spillet.\n\nDette spillet inneholder innhold av en seksuell natur, og noen av handlingene avbildet i dette spillet kan være ulovlig å replikere i virkeligheten. Altså, det er lek og artig i spillet, la oss holde det slik, eller hva? (~.~)v";
                buttonInst.Content = "Installasjon";
                buttonFemaleCard.Content = "Kort (Kvinner)";
                buttonMaleCard.Content = "Kort (Menn)";
                buttonScenes.Content = "Scener";
                buttonScreenshot.Content = "Skjermbilder";
                buttonUserData.Content = "UserData";
                labelStart.Content = "Start Spill";
                labelStartS.Content = "Start Studio";
                labelM.Content = "Spill manual";
                labelMS.Content = "Studio manual";
                toggleFullscreen.Content = "Start spill med fullskjerm";
                toggleConsole.Content = "Aktiver Konsoll";
                labelDist.Content = "Ukjent distribusjon";
                labelTranslated.Content = "Oversatt av: SmokeOfC";
                q_performance = "Ytelsesmodus";
                q_normal = "Normalmodus";
                q_quality = "Kvalitetsmodus";
                s_primarydisplay = "Hovedskjerm";
                s_subdisplay = "Subskjerm";
                labelDiscord.Content = "Besøk Discord";
                labelPatreon.Content = "Besøk Patreon";
                labelUpdate.Content = "Oppdater";

                // AIS Exclusive
                buttonHousing.Content = "Hus";
                toggleDHH.Content = "Aktiver DHH";
            }

            m_astrQuality = new[]
            {
                q_performance,
                q_normal,
                q_quality
            };
        }


        void PlayFunc(string strExe)
        {
            SettingManager.saveConfigFile(EnvironmentHelper.GetConfigFilePath());

            SettingManager.SaveRegistry(m_strGameRegistry);
            if (isStudio) SettingManager.SaveRegistry(m_strStudioRegistry);

            string text = EnvironmentHelper.GameRootDirectory + strExe;
            string ipa = "\u0022" + EnvironmentHelper.GameRootDirectory + "IPA.exe" + "\u0022";
            string ipaArgs = "\u0022" + text + "\u0022" + " --launch";
            if (File.Exists(text) && EnvironmentHelper.isIPA)
            {
                Process.Start(new ProcessStartInfo(ipa) { WorkingDirectory = EnvironmentHelper.GameRootDirectory, Arguments = ipaArgs });
                System.Windows.Application.Current.MainWindow.Close();
                return;
            }
            else if (File.Exists(text))
            {
                Process.Start(new ProcessStartInfo(text) { WorkingDirectory = EnvironmentHelper.GameRootDirectory });
                System.Windows.Application.Current.MainWindow.Close();
                return;
            }
            MessageBox.Show("Executable can't be located", "Warning!");
        }

        void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            PlayFunc(m_strGameExe);
        }

        void buttonStartS_Click(object sender, RoutedEventArgs e)
        {
            PlayFunc(m_strStudioExe);
        }

        void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            SettingManager.saveConfigFile(EnvironmentHelper.GetConfigFilePath());
            EnvironmentHelper.ReleaseMutex();
            System.Windows.Application.Current.MainWindow.Close();
        }

        void Resolution_Change(object sender, SelectionChangedEventArgs e)
        {
            if (-1 == dropRes.SelectedIndex)
            {
                return;
            }
            ComboBoxCustomItem comboBoxCustomItem = (ComboBoxCustomItem)dropRes.SelectedItem;
            SettingManager.m_Setting.m_strSizeChoose = comboBoxCustomItem.text;
            SettingManager.m_Setting.m_nWidthChoose = comboBoxCustomItem.width;
            SettingManager.m_Setting.m_nHeightChoose = comboBoxCustomItem.height;
        }

        void Quality_Change(object sender, SelectionChangedEventArgs e)
        {
            string a = dropQual.SelectedItem.ToString();
            if (a == q_performance)
            {
                SettingManager.m_Setting.m_nQualityChoose = 0;
                return;
            }
            if (a == q_normal)
            {
                SettingManager.m_Setting.m_nQualityChoose = 1;
                return;
            }
            if (!(a == q_quality))
            {
                return;
            }

            SettingManager.m_Setting.m_nQualityChoose = 2;
        }

        void windowUnChecked(object sender, RoutedEventArgs e)
        {
            setDisplayComboBox(false);
            dropRes.Text = SettingManager.m_Setting.m_strSizeChoose;
            SettingManager.m_Setting.m_bFullScreen = false;
        }

        void windowChecked(object sender, RoutedEventArgs e)
        {
            setDisplayComboBox(true);
            SettingManager.m_Setting.m_bFullScreen = true;
            SettingManager.setFullScreenDevice();
            dropRes.Text = SettingManager.m_Setting.m_strSizeChoose;
        }

        void buttonManual_Click(object sender, RoutedEventArgs e)
        {
            string manualEN = $"{EnvironmentHelper.GameRootDirectory}\\manual\\manual_en.html";
            string manualLANG = $"{EnvironmentHelper.GameRootDirectory}\\manual\\manual_{EnvironmentHelper.lang}.html";
            string manualJA = EnvironmentHelper.GameRootDirectory + m_strManualDir;

            if (File.Exists(manualEN) || File.Exists(manualLANG) || File.Exists(manualJA))
            {
                if (File.Exists(manualLANG))
                    Process.Start(manualLANG);
                else if (File.Exists(manualEN))
                    Process.Start(manualEN);
                else
                    Process.Start(manualJA);
                return;
            }
            MessageBox.Show("Manual could not be found.", "Warning!");
        }

        void buttonManualS_Click(object sender, RoutedEventArgs e)
        {
            string manualEN = $"{EnvironmentHelper.GameRootDirectory}\\manual_s\\manual_en.html";
            string manualLANG = $"{EnvironmentHelper.GameRootDirectory}\\manual_s\\manual_{EnvironmentHelper.lang}.html";
            string manualJA = EnvironmentHelper.GameRootDirectory + m_strStudioManualDir;

            if (File.Exists(manualEN) || File.Exists(manualLANG) || File.Exists(manualJA))
            {
                if (File.Exists(manualLANG))
                    Process.Start(manualLANG);
                else if (File.Exists(manualEN))
                    Process.Start(manualEN);
                else
                    Process.Start(manualJA);
                return;
            }
            MessageBox.Show("Manual could not be found.", "Warning!");
        }

        void buttonManualV_Click(object sender, RoutedEventArgs e)
        {
            string manualEN = $"{EnvironmentHelper.GameRootDirectory}\\manual_vr\\manual_en.html";
            string manualLANG = $"{EnvironmentHelper.GameRootDirectory}\\manual_vr\\manual_{EnvironmentHelper.lang}.html";
            string manualJA = EnvironmentHelper.GameRootDirectory + m_strVRManualDir;

            if (File.Exists(manualEN) || File.Exists(manualLANG) || File.Exists(manualJA))
            {
                if (File.Exists(manualLANG))
                    Process.Start(manualLANG);
                else if (File.Exists(manualEN))
                    Process.Start(manualEN);
                else
                    Process.Start(manualJA);
                return;
            }
            MessageBox.Show("Manual could not be found.", "Warning!");
        }

        void Display_Change(object sender, SelectionChangedEventArgs e)
        {
            if (-1 == dropDisplay.SelectedIndex)
            {
                return;
            }

            SettingManager.m_Setting.m_nDisplay = dropDisplay.SelectedIndex;
            if (SettingManager.m_Setting.m_bFullScreen)
            {
                setDisplayComboBox(true);
                if (!SettingManager.setFullScreenDevice())
                {
                    toggleFullscreen.IsChecked = false;
                    System.Windows.Forms.MessageBox.Show("This monitor doesn't support fullscreen.");
                }
                else
                {
                    dropRes.Text = SettingManager.m_Setting.m_strSizeChoose;
                }
            }
        }

        void buttonInst_Click(object sender, RoutedEventArgs e)
        {
            char[] trimChars = new char[]
            {
                '/'
            };
            char[] trimChars2 = new char[]
            {
                '\\'
            };
            string text = EnvironmentHelper.GameRootDirectory.TrimEnd(trimChars);
            text = text.TrimEnd(trimChars2);
            if (Directory.Exists(text))
            {
                Process.Start("explorer.exe", text);
                return;
            }
            MessageBox.Show("Folder could not be found, please launch the game at least once.", "Warning!");
        }

        void buttonScenes_Click(object sender, RoutedEventArgs e)
        {
            char[] trimChars = new char[]
            {
                '/'
            };
            char[] trimChars2 = new char[]
            {
                '\\'
            };
            string text = EnvironmentHelper.GameRootDirectory.TrimEnd(trimChars);
            text = text.TrimEnd(trimChars2) + "\\UserData\\Studio\\scene";
            if (Directory.Exists(text))
            {
                Process.Start("explorer.exe", text);
                return;
            }
            MessageBox.Show("Folder could not be found, please launch the game at least once.", "Warning!");
        }

        void buttonUserData_Click(object sender, RoutedEventArgs e)
        {
            char[] trimChars = new char[]
            {
                '/'
            };
            char[] trimChars2 = new char[]
            {
                '\\'
            };
            string text = EnvironmentHelper.GameRootDirectory.TrimEnd(trimChars);
            text = text.TrimEnd(trimChars2) + "\\UserData";
            if (Directory.Exists(text))
            {
                Process.Start("explorer.exe", text);
                return;
            }
            MessageBox.Show("Folder could not be found, please launch the game at least once.", "Warning!");
        }

        void buttonHousing_Click(object sender, RoutedEventArgs e)
        {
            char[] trimChars = new char[]
            {
                '/'
            };
            char[] trimChars2 = new char[]
            {
                '\\'
            };
            string text = EnvironmentHelper.GameRootDirectory.TrimEnd(trimChars);
            text = text.TrimEnd(trimChars2) + "\\UserData\\housing";
            if (Directory.Exists(text))
            {
                Process.Start("explorer.exe", text);
                return;
            }
            MessageBox.Show("Folder could not be found, please launch the game at least once.", "Warning!");
        }

        void buttonScreenshot_Click(object sender, RoutedEventArgs e)
        {
            char[] trimChars = new char[]
            {
                '/'
            };
            char[] trimChars2 = new char[]
            {
                '\\'
            };
            string text = EnvironmentHelper.GameRootDirectory.TrimEnd(trimChars);
            text = text.TrimEnd(trimChars2) + "\\UserData\\cap";
            if (Directory.Exists(text))
            {
                Process.Start("explorer.exe", text);
                return;
            }
            MessageBox.Show("Folder could not be found, please launch the game at least once.", "Warning!");
        }

        void buttonFemaleCard_Click(object sender, RoutedEventArgs e)
        {
            char[] trimChars = new char[]
            {
                '/'
            };
            char[] trimChars2 = new char[]
            {
                '\\'
            };
            string text = EnvironmentHelper.GameRootDirectory.TrimEnd(trimChars);
            text = text.TrimEnd(trimChars2) + "\\UserData\\chara\\female";
            if (Directory.Exists(text))
            {
                Process.Start("explorer.exe", text);
                return;
            }
            MessageBox.Show("Folder could not be found, please launch the game at least once.", "Warning!");
        }

        void buttonMaleCard_Click(object sender, RoutedEventArgs e)
        {
            char[] trimChars = new char[]
            {
                '/'
            };
            char[] trimChars2 = new char[]
            {
                '\\'
            };
            string text = EnvironmentHelper.GameRootDirectory.TrimEnd(trimChars);
            text = text.TrimEnd(trimChars2) + "\\UserData\\chara\\male";
            if (Directory.Exists(text))
            {
                Process.Start("explorer.exe", text);
                return;
            }
            MessageBox.Show("Folder could not be found, please launch the game at least once.", "Warning!");
        }

        void SystemInfo_Open(object sender, RoutedEventArgs e)
        {
            string text = Environment.ExpandEnvironmentVariables("%windir%") + "/System32/dxdiag.exe";
            if (File.Exists(text))
            {
                Process.Start(text);
                return;
            }
            MessageBox.Show("Folder could not be found, please launch the game at least once.", "Warning!");
        }

        void setDisplayComboBox(bool _bFullScreen)
        {
            dropRes.Items.Clear();
            int nDisplay = SettingManager.m_Setting.m_nDisplay;
            foreach (SettingManager.DisplayMode displayMode in (_bFullScreen ? SettingManager.m_listCurrentDisplay[nDisplay].list : SettingManager.m_listDefaultDisplay))
            {
                ComboBoxCustomItem newItem = new ComboBoxCustomItem
                {
                    text = displayMode.text,
                    width = displayMode.Width,
                    height = displayMode.Height
                };
                dropRes.Items.Add(newItem);
            }
        }

        void MenuCloseButton(object sender, EventArgs e)
        {
            SettingManager.saveConfigFile(EnvironmentHelper.GetConfigFilePath());
            EnvironmentHelper.ReleaseMutex();
        }


        const string m_strDefSizeText = "1280 x 720 (16 : 9)";


        bool noTL = false;
        bool startup;

        bool versionAvail;

        string q_performance = "Performance";
        string q_normal = "Normal";
        string q_quality = "Quality";
        string s_primarydisplay = "PrimaryDisplay";
        string s_subdisplay = "SubDisplay";

        //string updateURL;
        //string packVersion;
        //string newPackVersion;


        void discord_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://discord.gg/F3bDEFE");
        }
        void patreon_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(EnvironmentHelper.patreonURL);
        }

        void update_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SettingManager.saveConfigFile(EnvironmentHelper.GetConfigFilePath());
            SettingManager.SaveRegistry(m_strGameRegistry);
            if (isStudio) SettingManager.SaveRegistry(m_strStudioRegistry);

            string marcofix = EnvironmentHelper.GameRootDirectory.TrimEnd('\\', '/', ' ');
            EnvironmentHelper.kkman = EnvironmentHelper.kkman.TrimEnd('\\', '/', ' ');
            string finaldir;

            if (!File.Exists($@"{EnvironmentHelper.kkman}\StandaloneUpdater.exe"))
            {
                finaldir = $@"{EnvironmentHelper.GameRootDirectory}{EnvironmentHelper.kkman}";
            }
            else
            {
                finaldir = EnvironmentHelper.kkman;
            }

            string text = $@"{finaldir}\StandaloneUpdater.exe";

            string argdir = $"\u0022{marcofix}\u0022";
            string argloc = EnvironmentHelper.updateSources;
            string args = $"{argdir} {argloc}";

            if (!EnvironmentHelper.updatelocExists)
                args = $"{argdir}";

            if (File.Exists(text))
            {
                Process.Start(new ProcessStartInfo(text) { WorkingDirectory = $@"{finaldir}", Arguments = args });
                return;
            }
        }

        void langEnglish(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PartyFilter("en");
        }
        void langJapanese(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PartyFilter("ja");
        }
        void langChinese(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PartyFilter("zh-CN");
        }
        void langKorean(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PartyFilter("ko");
        }
        void langSpanish(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PartyFilter("es");
        }
        void langBrazil(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PartyFilter("pt");
        }
        void langFrench(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PartyFilter("fr");
        }
        void langGerman(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PartyFilter("de");
        }
        void langNorwegian(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PartyFilter("no");
        }

        void PartyFilter(string language)
        {
            EnvironmentHelper.SetLanguage(language, !noTL);
        }

        private void modeDev_Checked(object sender, RoutedEventArgs e)
        {
            if (!startup)
            {
                EnvironmentHelper.DeveloperModeEnabled = true;
            }
        }

        private void modeDev_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!startup)
            {
                EnvironmentHelper.DeveloperModeEnabled = false;
            }
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void HoneyPotInspector_Run(object sender, RoutedEventArgs e)
        {
            if (File.Exists($"{EnvironmentHelper.GameRootDirectory}\\HoneyPot\\HoneyPotInspector.exe"))
            {
                Process.Start($"{EnvironmentHelper.GameRootDirectory}\\HoneyPot\\HoneyPotInspector.exe");
            }
            else
            {
                MessageBox.Show("HoneyPot doesn't seem to be applied to this installation.");
            }
        }

        private void dhh_Checked(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.EnablePlugin(EnvironmentHelper.BepinPluginsDir + "DHH_AI4.dll");
            toggleAIGraphics.IsChecked = false;
        }

        private void dhh_Unchecked(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.DisablePlugin(EnvironmentHelper.BepinPluginsDir + "DHH_AI4.dll");
        }

        private void aigraphics_Checked(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.EnablePlugin(EnvironmentHelper.BepinPluginsDir + "AIGraphics\\AI_Graphics.dll");
            toggleDHH.IsChecked = false;
            if (!startup)
                MessageBox.Show("Press F5 ingame for menu.", "Information");
        }

        private void aigraphics_Unchecked(object sender, RoutedEventArgs e)
        {
            EnvironmentHelper.DisablePlugin(EnvironmentHelper.BepinPluginsDir + "AIGraphics\\AI_Graphics.dll");
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}