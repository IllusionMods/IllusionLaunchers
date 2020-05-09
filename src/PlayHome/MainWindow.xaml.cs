using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Windows.Input;

namespace InitSetting
{
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWow64Process([In] IntPtr hProcess, out bool lpSystemInfo);

        [DllImport("user32.dll")]
        static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("User32.dll")]
        static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr rect, MainWindow.EnumDisplayMonitorsCallback callback, IntPtr dwData);

        [DllImport("User32.dll")]
        static extern bool GetMonitorInfo(IntPtr hMonitor, ref MainWindow.MonitorInfoEx info);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
        IntPtr InBuffer, int nInBufferSize,
        IntPtr OutBuffer, int nOutBufferSize,
        out int pBytesReturned, IntPtr lpOverlapped);

        public MainWindow()
        {
            InitializeComponent();
            //if (!DoubleStartCheck())
            //{
            //    System.Windows.Application.Current.MainWindow.Close();
            //    return;
            //}

            // Check for duplicate launches

            Process process = Process.GetCurrentProcess();
            var dupl = (Process.GetProcessesByName(process.ProcessName));
            if (true)
            {
                foreach (var p in dupl)
                {
                    if (p.Id != process.Id)
                        p.Kill();
                }
            }

            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            startup = true;

            //temp hide unimplemented stuffs
            CustomRes.Visibility = Visibility.Hidden;

            Directory.CreateDirectory(m_strCurrentDir + m_customDir);

            if (!File.Exists(m_strCurrentDir + m_customDir + kkmdir))
            {

            }

            // Framework test
            isIPA = File.Exists($"{m_strCurrentDir}\\IPA.exe");
            isBepIn = Directory.Exists($"{m_strCurrentDir}\\BepInEx");

            if (isIPA && isBepIn)
                MessageBox.Show("Both BepInEx and IPA is detected in the game folder!\n\nApplying both frameworks may lead to problems when running the game!", "Warning!");

            // Check if console is active

            if (File.Exists(m_strCurrentDir + "/Bepinex/config/BepInEx.cfg"))
            {
                var ud = Path.Combine(m_strCurrentDir, @"BepInEx\config\BepInEx.cfg");

                try
                {
                    var contents = File.ReadAllLines(ud).ToList();

                    var devmodeEN = contents.FindIndex(s => s.ToLower().Contains("[Logging.Console]".ToLower()));
                    if (devmodeEN >= 0)
                    {
                        var i = contents.FindIndex(devmodeEN, s => s.StartsWith("Enabled = true"));
                        var n = contents.FindIndex(devmodeEN, s => s.StartsWith("[Logging.Disk]"));
                        if (i < n)
                        {
                            toggleConsole.IsChecked = true;
                        }

                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Something went wrong: " + e);
                }
            }
            else
            {
                toggleConsole.IsEnabled = false;
            }


            // Updater stuffs

            kkmanExist = File.Exists(m_strCurrentDir + m_customDir + kkmdir);
            updatelocExists = File.Exists(m_strCurrentDir + m_customDir + updateLoc);
            if (kkmanExist)
            {
                var kkmanFileStream = new FileStream(m_strCurrentDir + m_customDir + kkmdir, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(kkmanFileStream, Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        kkman = line;
                    }
                }
                kkmanFileStream.Close();
                if (updatelocExists)
                {
                    var updFileStream = new FileStream(m_strCurrentDir + m_customDir + updateLoc, FileMode.Open, FileAccess.Read);
                    using (var streamReader = new StreamReader(updFileStream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            updated = line;
                        }
                    }
                    updFileStream.Close();
                }
                else
                {
                    updated = "";
                }
            }
            else
            {
                gridUpdate.Visibility = Visibility.Hidden;
            }

            if (!File.Exists(m_strCurrentDir + m_customDir + kkmdir))
            {

            }

            // Mod settings

            if (File.Exists($"{m_strCurrentDir}\\Plugins\\HoneyPot.dll"))
            {
                toggleHoneyPot.IsChecked = true;
            }
            if (File.Exists($"{m_strCurrentDir}\\Plugins\\ProjectHighHeel.dll"))
            {
                toggleDHH.IsChecked = true;
            }
            if (!File.Exists($"{m_strCurrentDir}\\Plugins\\ProjectHighHeel.dl_") && !File.Exists($"{m_strCurrentDir}\\Plugins\\ProjectHighHeel.dll"))
            {
                toggleDHH.IsEnabled = false;
            }
            if (!File.Exists($"{m_strCurrentDir}\\Plugins\\HoneyPot.dl_") && !File.Exists($"{m_strCurrentDir}\\Plugins\\HoneyPot.dll"))
            {
                toggleHoneyPot.IsEnabled = false;
            }
            if (File.Exists($"{m_strCurrentDir}{m_customDir}/toggle32.txt"))
            {
                toggle32.IsChecked = true;
                x86 = true;
            }
            if (!File.Exists($"{m_strCurrentDir}\\PlayHome32bit.exe"))
            {
                toggle32.Visibility = Visibility.Hidden;
                x86 = false;
            }


            startup = false;

            LangExists = File.Exists(m_strCurrentDir + m_customDir + decideLang);
            if (LangExists)
            {
                var verFileStream = new FileStream(m_strCurrentDir + m_customDir + decideLang, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(verFileStream, Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        lang = line;
                    }
                }
                verFileStream.Close();
            }

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
            if (lang == "ja")
            {
                labelTranslated.Visibility = Visibility.Visible;
                labelTranslatedBorder.Visibility = Visibility.Visible;

                warningText.Text = "このゲームは成人向けので、18歳未満（または地域の法律によりと同等の年齢）がこのゲームをプレイまたは所有しているができない。\n\nこのゲームには性的内容の内容が含まれます。内に描かれている行動は、実生活で複製することは違法です。つまり、これは面白いゲームです、そうしましょう？(~.~)v";
                buttonInst.Content = "インストール";
                buttonFemaleCard.Content = "キャラカード (女性)";
                buttonMaleCard.Content = "キャラカード (男性)";
                buttonScenes.Content = "シーン";
                buttonScreenshot.Content = "SS"; buttonUserData.Content = "UserData";
                labelStart.Content = "ゲーム開始";
                labelStartS.Content = "スタジオ開始";
                labelStartVR.Content = "VR開始";
                labelM.Content = "ゲーム";
                labelMS.Content = "スタジオ";
                labelMV.Content = "VR";
                HoneyPotInspector.Text = "HoneyPot Inspectorを実行する";
                toggleFullscreen.Content = "全画面表示";
                toggleHoneyPot.Content = "HoneyPotを有効にする";
                toggleDHH.Content = "DHHを有効にする";
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
            }
            else if (lang == "zh-CN") // By @Madevil#1103 & @𝐄𝐀𝐑𝐓𝐇𝐒𝐇𝐈𝐏 💖#4313 
            {
                labelTranslated.Visibility = Visibility.Visible;
                labelTranslatedBorder.Visibility = Visibility.Visible;

                warningText.Text = "此游戏适用于成人用户，任何未满18岁的人（或根据当地法律规定的同等人）都不得遊玩或拥有此游戏。\n\n这个游戏包含性相关的内容，某些行为在现实生活中可能是非法的。所以，游戏中的所有乐趣请保留在游戏中，让我们保持这种方式吧? (~.~)v";
                buttonInst.Content = "游戏主目录";
                buttonFemaleCard.Content = "人物卡 (女)";
                buttonMaleCard.Content = "人物卡 (男)";
                buttonScenes.Content = "工作室场景";
                buttonScreenshot.Content = "截图";
                buttonUserData.Content = "UserData";
                labelStart.Content = "开始游戏";
                labelStartS.Content = "开始工作室";
                labelStartVR.Content = "开始VR";
                labelM.Content = "游戏手册";
                labelMS.Content = "工作室手册";
                labelMV.Content = "VR手册";
                HoneyPotInspector.Text = "运行 HoneyPot Inspector";
                toggleFullscreen.Content = "全屏执行";
                toggleHoneyPot.Content = "激活HoneyPot";
                toggleDHH.Content = "激活DHH";
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
            }
            else if (lang == "ko") // By @Keris-#1903 
            {
                labelTranslated.Visibility = Visibility.Visible;
                labelTranslatedBorder.Visibility = Visibility.Visible;

                warningText.Text = "이게임은 성인용입니다 18세 미만의 사람(또는 법에따라 동등한사람)은 게임을 하거나 해당게임을 소유하면 안됩니다\n\n이게임에는 성적인 내용이 포함되어있으며 그안에 묘사된 행동중 일부는 실제에서 행동하면 법적인 처벌을 받습니다";
                buttonInst.Content = "설치된폴더";
                buttonFemaleCard.Content = "캐릭터 카드 (여자)";
                buttonMaleCard.Content = "캐릭터 카드 (남성)";
                buttonScenes.Content = "장면";
                buttonScreenshot.Content = "스크린샷 폴더";
                buttonUserData.Content = "UserData";
                labelStart.Content = "플레이 시작";
                labelStartS.Content = "스튜디오 시작";
                labelStartVR.Content = "코이카츠 VR 시작";
                labelM.Content = "플레이 메뉴얼";
                labelMS.Content = "스튜디오 메뉴얼";
                labelMV.Content = "VR 메뉴얼";
                HoneyPotInspector.Text = "HoneyPot Inspector 시작";
                toggleFullscreen.Content = "전체화면으로 시작";
                toggleHoneyPot.Content = "HoneyPot 활성화";
                toggleDHH.Content = "DHH 활성화";
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
            }
            else if (lang == "es") // By @Heroine Nisa#3207
            {
                labelTranslated.Visibility = Visibility.Visible;
                labelTranslatedBorder.Visibility = Visibility.Visible;

                warningText.Text = "Este juego está dirigido hacia un público adulto, ninguna persona bajo 18 años (o equivalente según las leyes locales) no deberían de jugar o estar en posesión de este juego. \n\nEste juego contiene escenas de carácter sexual, y algunas de las acciones representadas en el mismo pueden ser ilegales de hacerlas en la vida real.  También conocido como, todo es diversión y risas dentro del juego, así que mantengámoslo así, ¿vale? (~.~)v";
                buttonInst.Content = "Instalar";
                buttonFemaleCard.Content = "Cartas de Personaje (M)";
                buttonMaleCard.Content = "Cartas de Personaje (F)";
                buttonScenes.Content = "Escenas";
                buttonScreenshot.Content = "Capturas";
                buttonUserData.Content = "UserData";
                labelStart.Content = "Iniciar juego";
                labelStartS.Content = "Iniciar Studio";
                labelStartVR.Content = "Iniciar VR";
                labelM.Content = "Manual de juego";
                labelMS.Content = "Manual de studio";
                labelMV.Content = "Manual de VR";
                HoneyPotInspector.Text = "Ejecutar la\nHoneyPot Inspector";
                toggleFullscreen.Content = "Lanzar Juego en Pantalla Completa";
                toggleHoneyPot.Content = "Activar HoneyPot";
                toggleDHH.Content = "Activar DHH";
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
            }
            else if (lang == "pt") // By @Neptune#1989 
            {
                labelTranslated.Visibility = Visibility.Visible;
                labelTranslatedBorder.Visibility = Visibility.Visible;

                warningText.Text = "Este jogo, por apresentar conteúdo adulto, é voltado para maiores de 18 anos (ou equivalente perante a lei local), menores de idade não devem jogar ou possuí-lo.\n\nAlgumas das ações presentes nessa obra de ficção podem ser ilegais ao serem realizadas no mundo real. Deixe essas coisas somente para o mundo fictício, combinado? (~.~)v";
                buttonInst.Content = "Instalar";
                buttonFemaleCard.Content = "Cards de Personagens (F)";
                buttonMaleCard.Content = "Cards de Personagens (M)";
                buttonScenes.Content = "Cenas";
                buttonScreenshot.Content = "Capturas de Tela";
                buttonUserData.Content = "UserData";
                labelStart.Content = "Iniciar Jogo";
                labelStartS.Content = "Iniciar Studio";
                labelStartVR.Content = "Iniciar VR";
                labelM.Content = "Manual do Jogo";
                labelMS.Content = "Manual do Studio";
                labelMV.Content = "Manual do VR";
                HoneyPotInspector.Text = "Executar HoneyPot Inspector";
                toggleFullscreen.Content = "Iniciar Jogo em Tela Cheia";
                toggleHoneyPot.Content = "Ativar HoneyPot";
                toggleDHH.Content = "Ativar DHH";
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
            }
            else if (lang == "fr") // By VaizravaNa#2315
            {
                labelTranslated.Visibility = Visibility.Visible;
                labelTranslatedBorder.Visibility = Visibility.Visible;

                warningText.Text = "Ce jeu est destiné à un public adulte, aucun mineur en dessous de 18 ans (ou l'équivalent selon les lois locales) ne doit pas jouer ou posséder ce jeu. \n\nCe jeu contient des scènes matures, et certaines actions du jeu peuvent être considéré comme illégales, à ne pas reproduire dans la vraie vie. Ce n'est que de la fiction, du moment que cela reste dans le jeu. Amusez-vous bien!";
                buttonInst.Content = "Installation";
                buttonFemaleCard.Content = "Personnages (Femme)";
                buttonMaleCard.Content = "Personnages (Mâle)";
                buttonScenes.Content = "Scènes";
                buttonScreenshot.Content = "Captures d'écran";
                buttonUserData.Content = "UserData";
                labelStart.Content = "Lancer le jeu";
                labelStartS.Content = "Lancer le Studio";
                labelStartVR.Content = "Lancer la VR";
                labelM.Content = "Manuel de jeu";
                labelMS.Content = "Manuel du Studio";
                labelMV.Content = "Manuel de VR";
                HoneyPotInspector.Text = "Lancer HoneyPot Inspector";
                toggleFullscreen.Content = "Lancer le jeu en pleins écran";
                toggleHoneyPot.Content = "Activer HoneyPot";
                toggleDHH.Content = "Activer DHH";
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
            }
            else if (lang == "de") // By @DONTFORGETME#6198 
            {
                labelTranslated.Visibility = Visibility.Visible;
                labelTranslatedBorder.Visibility = Visibility.Visible;

                warningText.Text = "Dieses Spiel ist ausschließlich für erwachsenes Publikum vorgesehen. Niemand unter 18 Jahren ( Oder entsprechend deiner örtlichen Gesetze ) ist vorgesehen dieses Spiel zu spielen, oder es zu besitzen.\n\nDieses Spiel enthällt sexuelle Inhalte welche bei Ausführung im realen Leben strafbar sein könnten. Dinge die im Spiel geschehen sollten also auch im Spiel bleiben in Ordnung? (~.~)v";
                buttonInst.Content = "Installieren";
                buttonFemaleCard.Content = "Charakter Karten (F)";
                buttonMaleCard.Content = "Charakter Karten (M)";
                buttonScenes.Content = "Scenen";
                buttonScreenshot.Content = "ScreenShots";
                buttonUserData.Content = "UserData";
                labelStart.Content = "Starte Spiel";
                labelStartS.Content = "Starte Studio";
                labelStartVR.Content = "Starte VR";
                labelM.Content = "Spiel Bedienungsanleitung";
                labelMS.Content = "Studio Bedienungsanleitung";
                labelMV.Content = "VR Bedienungsanleitung";
                HoneyPotInspector.Text = "";
                toggleFullscreen.Content = "Starte Spiel in Vollbildmodus";
                toggleHoneyPot.Content = "HoneyPot umschalten";
                toggleDHH.Content = "DHH umschalten";
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
            }
            else if (lang == "no") // By @SmokeOfC|女神様の兄様#1984
            {
                labelTranslated.Visibility = Visibility.Visible;
                labelTranslatedBorder.Visibility = Visibility.Visible;

                warningText.Text = "Dette spillet er ment for voksne spillere, og ingen person under 18 år (Eller tilsvarende iht lokal lov) er tiltenkt å være i besittelse av dette spillet.\n\nDette spillet inneholder innhold av en seksuell natur, og noen av handlingene avbildet i dette spillet kan være ulovlig å replikere i virkeligheten. Altså, det er lek og artig i spillet, la oss holde det slik, eller hva? (~.~)v";
                buttonInst.Content = "Installasjon";
                buttonFemaleCard.Content = "Kort (Kvinner)";
                buttonMaleCard.Content = "Kort (Menn)";
                buttonScenes.Content = "Scener";
                buttonScreenshot.Content = "Skjermbilder";
                buttonUserData.Content = "UserData";
                labelStart.Content = "Start Spill";
                labelStartS.Content = "Start Studio";
                labelStartVR.Content = "Start VR";
                labelM.Content = "Spill manual";
                labelMS.Content = "Studio manual";
                labelMV.Content = "VR manual";
                HoneyPotInspector.Text = "Start HoneyPot Inspector";
                toggleFullscreen.Content = "Start spill med fullskjerm";
                toggleHoneyPot.Content = "Aktiver HoneyPot";
                toggleDHH.Content = "Aktiver DHH";
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
            }

            m_astrQuality = new string[]
            {
                q_performance,
                q_normal,
                q_quality
            };

            // Do checks

            is64bitOS = Is64BitOS();
            isStudio = File.Exists(m_strCurrentDir + m_strStudioExe);
            isMainGame = File.Exists(m_strCurrentDir + m_strGameExe);

            if(!is64bitOS)
            {
                toggle32.IsChecked = true;
                toggle32.IsEnabled = false;
            }

            // Customization options

            CharExists = File.Exists(m_strCurrentDir + m_customDir + charLoc);
            BackgExists = File.Exists(m_strCurrentDir + m_customDir + backgLoc);
            WarningExists = File.Exists(m_strCurrentDir + m_customDir + warningLoc);
            PatreonExists = File.Exists(m_strCurrentDir + m_customDir + patreonLoc);

            // Launcher Customization: Grabbing versioning of install method

            versionAvail = File.Exists(m_strCurrentDir + "version");
            if (versionAvail)
            {
                var verFileStream = new FileStream(m_strCurrentDir + "version", FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(verFileStream, Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        labelDist.Content = line;
                    }
                }
                verFileStream.Close();
            }

            // Launcher Customization: Defining Warning, background and character

            if (WarningExists)
            {
                var verFileStream = new FileStream(m_strCurrentDir + m_customDir + warningLoc, FileMode.Open, FileAccess.Read);
                try
                {
                    using (StreamReader sr = new StreamReader(m_strCurrentDir + m_customDir + warningLoc))
                    {
                        String line = sr.ReadToEnd();
                        warningText.Text = line;
                    }
                }
                catch (IOException e)
                {
                    warningText.Text = e.Message;
                }
            }
            if (CharExists)
            {
                Uri urich = new Uri(m_strCurrentDir + m_customDir + charLoc, UriKind.RelativeOrAbsolute);
                PackChara.Source = BitmapFrame.Create(urich);
            }
            if (BackgExists)
            {
                Uri uribg = new Uri(m_strCurrentDir + m_customDir + backgLoc, UriKind.RelativeOrAbsolute);
                appBG.ImageSource = BitmapFrame.Create(uribg);
            }
            if (PatreonExists)
            {
                var verFileStream = new FileStream(m_strCurrentDir + m_customDir + patreonLoc, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(verFileStream, Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        patreonURL = line;
                    }
                }
                verFileStream.Close();
            }
            else
            {
                linkPatreon.Visibility = Visibility.Collapsed;
                patreonBorder.Visibility = Visibility.Collapsed;
                patreonIMG.Visibility = Visibility.Collapsed;
            }

            int num = Screen.AllScreens.Length;
            getDisplayMode_EnumDisplaySettings(num);
            m_Setting.m_strSizeChoose = "1280 x 720 (16 : 9)";
            m_Setting.m_nWidthChoose = 1280;
            m_Setting.m_nHeightChoose = 720;
            m_Setting.m_nQualityChoose = 1;
            m_Setting.m_nLangChoose = 0;
            m_Setting.m_nDisplay = 0;
            m_Setting.m_bFullScreen = false;
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

            SetEnableAndVisible();

            string path = m_strCurrentDir + m_strSaveDir;
        CheckConfigFile:
            if (File.Exists(path))
            {
                try
                {
                    using (FileStream fileStream = new FileStream(path, FileMode.Open))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(ConfigSetting));
                        m_Setting = (ConfigSetting)xmlSerializer.Deserialize(fileStream);
                    }

                    m_Setting.m_nDisplay = Math.Min(m_Setting.m_nDisplay, num - 1);
                    setDisplayComboBox(m_Setting.m_bFullScreen);
                    var flag = false;
                    for (var k = 0; k < dropRes.Items.Count; k++)
                    {
                        if (dropRes.Items[k].ToString() == m_Setting.m_strSizeChoose)
                            flag = true;
                    }
                    dropRes.Text = flag ? m_Setting.m_strSizeChoose : "1280 x 720 (16 : 9)";
                    toggleFullscreen.IsChecked = m_Setting.m_bFullScreen;
                    dropQual.Text = m_astrQuality[m_Setting.m_nQualityChoose];
                    string text = m_Setting.m_nDisplay == 0 ? s_primarydisplay : $"{s_subdisplay} : " + m_Setting.m_nDisplay;
                    if (num == 2)
                    {
                        text = new[]
                        {
                        s_primarydisplay,
                        $"{s_subdisplay} : 1"
                        }[m_Setting.m_nDisplay];
                    }
                    if (dropDisplay.Items.Contains(text))
                        dropDisplay.Text = text;
                    else
                    {
                        dropDisplay.Text = s_primarydisplay;
                        m_Setting.m_nDisplay = 0;
                    }
                }
                catch (Exception)
                {
                    System.Windows.Forms.MessageBox.Show("/UserData/setup.xml file was corrupted, settings will be reset.");
                    File.Delete(path);
                    goto CheckConfigFile;
                }
            }
            else
            {
                setDisplayComboBox(false);
                dropRes.Text = m_Setting.m_strSizeChoose;
                dropQual.Text = m_astrQuality[m_Setting.m_nQualityChoose];
                dropDisplay.Text = s_primarydisplay;
            }
        }

        void SetEnableAndVisible()
        {
            
        }

        void SaveRegistry()
        {
            using (RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(m_strGameRegistry))
            {
                registryKey.SetValue("Screenmanager Is Fullscreen mode_h3981298716", m_Setting.m_bFullScreen ? 1 : 0);
                registryKey.SetValue("Screenmanager Resolution Height_h2627697771", m_Setting.m_nHeightChoose);
                registryKey.SetValue("Screenmanager Resolution Width_h182942802", m_Setting.m_nWidthChoose);
                registryKey.SetValue("UnityGraphicsQuality_h1669003810", 2);
                registryKey.SetValue("UnitySelectMonitor_h17969598", m_Setting.m_nDisplay);
            }
            if (isStudio)
            {
                using (RegistryKey registryKey2 = Registry.CurrentUser.CreateSubKey(m_strStudioRegistry))
                {
                    registryKey2.SetValue("Screenmanager Is Fullscreen mode_h3981298716", m_Setting.m_bFullScreen ? 1 : 0);
                    registryKey2.SetValue("Screenmanager Resolution Height_h2627697771", m_Setting.m_nHeightChoose);
                    registryKey2.SetValue("Screenmanager Resolution Width_h182942802", m_Setting.m_nWidthChoose);
                    registryKey2.SetValue("UnityGraphicsQuality_h1669003810", 2);
                    registryKey2.SetValue("UnitySelectMonitor_h17969598", m_Setting.m_nDisplay);
                }
            }
        }

        void PlayFunc(string strExe)
        {
            saveConfigFile(m_strCurrentDir + m_strSaveDir);
            SaveRegistry();
            string text = m_strCurrentDir + strExe;
            string ipa = "\u0022" + m_strCurrentDir + "IPA.exe" + "\u0022";
            string ipaArgs = "\u0022" + text + "\u0022" + " --launch";
            if (File.Exists(text) && isIPA)
            {
                Process.Start(new ProcessStartInfo(ipa) { WorkingDirectory = m_strCurrentDir, Arguments = ipaArgs });
                System.Windows.Application.Current.MainWindow.Close();
                return;
            }
            else if (File.Exists(text))
            {
                Process.Start(new ProcessStartInfo(text) { WorkingDirectory = m_strCurrentDir });
                System.Windows.Application.Current.MainWindow.Close();
                return;
            }
            MessageBox.Show("Executable can't be located", "Warning!");
        }

        void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            if(x86 == true)
                PlayFunc(m_strGameExe32);
            else
                PlayFunc(m_strGameExe);
        }

        void buttonStartS_Click(object sender, RoutedEventArgs e)
        {
            if (x86 == true)
                PlayFunc(m_strStudioExe32);
            else
                PlayFunc(m_strStudioExe);
        }

        void buttonStartV_Click(object sender, RoutedEventArgs e)
        {
            PlayFunc(m_strVRExe);
        }

        void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            saveConfigFile(m_strCurrentDir + m_strSaveDir);
            ReleaseMutex();
            System.Windows.Application.Current.MainWindow.Close();
        }

        void Resolution_Change(object sender, SelectionChangedEventArgs e)
        {
            if (-1 == dropRes.SelectedIndex)
            {
                return;
            }
            ComboBoxCustomItem comboBoxCustomItem = (ComboBoxCustomItem)dropRes.SelectedItem;
            m_Setting.m_strSizeChoose = comboBoxCustomItem.text;
            m_Setting.m_nWidthChoose = comboBoxCustomItem.width;
            m_Setting.m_nHeightChoose = comboBoxCustomItem.height;
        }

        void Quality_Change(object sender, SelectionChangedEventArgs e)
        {
            string a = dropQual.SelectedItem.ToString();
            if (a == q_performance)
            {
                m_Setting.m_nQualityChoose = 0;
                return;
            }
            if (a == q_normal)
            {
                m_Setting.m_nQualityChoose = 1;
                return;
            }
            if (!(a == q_quality))
            {
                return;
            }
            m_Setting.m_nQualityChoose = 2;
        }

        void windowUnChecked(object sender, RoutedEventArgs e)
        {
            setDisplayComboBox(false);
            dropRes.Text = m_Setting.m_strSizeChoose;
            m_Setting.m_bFullScreen = false;
        }

        void windowChecked(object sender, RoutedEventArgs e)
        {
            setDisplayComboBox(true);
            m_Setting.m_bFullScreen = true;
            setFullScreenDevice();
        }

        void buttonManual_Click(object sender, RoutedEventArgs e)
        {
            string manualEN = $"{m_strCurrentDir}\\manual\\manual_en.html";
            string manualLANG = $"{m_strCurrentDir}\\manual\\manual_{lang}.html";
            string manualJA = m_strCurrentDir + m_strManualDir;

            if(File.Exists(manualEN) || File.Exists(manualLANG) || File.Exists(manualJA))
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
            string manualEN = $"{m_strCurrentDir}\\manual_s\\manual_en.html";
            string manualLANG = $"{m_strCurrentDir}\\manual_s\\manual_{lang}.html";
            string manualJA = m_strCurrentDir + m_strStudioManualDir;

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
            string manualEN = $"{m_strCurrentDir}\\manual_vr\\manual_en.html";
            string manualLANG = $"{m_strCurrentDir}\\manual_vr\\manual_{lang}.html";
            string manualJA = m_strCurrentDir + m_strVRManualDir;

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
            m_Setting.m_nDisplay = dropDisplay.SelectedIndex;
            if (m_Setting.m_bFullScreen)
            {
                setDisplayComboBox(true);
                setFullScreenDevice();
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
            string text = m_strCurrentDir.TrimEnd(trimChars);
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
            string text = m_strCurrentDir.TrimEnd(trimChars);
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
            string text = m_strCurrentDir.TrimEnd(trimChars);
            text = text.TrimEnd(trimChars2) + "\\UserData\\Studio\\scene";
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
            string text = m_strCurrentDir.TrimEnd(trimChars);
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
            string text = m_strCurrentDir.TrimEnd(trimChars);
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
            string text = m_strCurrentDir.TrimEnd(trimChars);
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

        bool DoubleStartCheck()
        {
            bool flag;
            mutex = new Mutex(true, "AIS", out flag);
            bool v = !flag;
            if (v)
            {
                if (mutex != null)
                {
                    mutex.Close();
                }
                mutex = null;
                return false;
            }
            return true;
        }

        bool ReleaseMutex()
        {
            if (mutex == null)
            {
                return false;
            }
            mutex.ReleaseMutex();
            mutex.Close();
            mutex = null;
            return true;
        }

        void setDisplayComboBox(bool _bFullScreen)
        {
            dropRes.Items.Clear();
            int nDisplay = m_Setting.m_nDisplay;
            foreach (MainWindow.DisplayMode displayMode in (_bFullScreen ? m_listCurrentDisplay[nDisplay].list : m_listDefaultDisplay))
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

        void saveConfigFile(string _strFilePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(_strFilePath)))
            {
                return;
            }
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(_strFilePath, FileMode.Create);
                if (fileStream != null)
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.GetEncoding("UTF-16")))
                    {
                        XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
                        xmlSerializerNamespaces.Add(string.Empty, string.Empty);
                        new XmlSerializer(typeof(ConfigSetting)).Serialize(streamWriter, m_Setting, xmlSerializerNamespaces);
                        fileStream = null;
                    }
                }
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Dispose();
                }
            }
        }

        void getDisplayMode_CIM_VideoControllerResolution()
        {
            ManagementObjectCollection instances = new ManagementClass("CIM_VideoControllerResolution").GetInstances();
            List<MainWindow.DisplayMode> list = new List<MainWindow.DisplayMode>();
            uint num = 0u;
            uint num2 = 0u;
            foreach (ManagementBaseObject managementBaseObject in instances)
            {
                ManagementObject managementObject = (ManagementObject)managementBaseObject;
                uint nXX = (uint)managementObject["HorizontalResolution"];
                uint nYY = (uint)managementObject["VerticalResolution"];
                if ((num != nXX || num2 != nYY) && (ulong)managementObject["NumberOfColors"] == 4294967296UL)
                {
                    MainWindow.DisplayMode displayMode = m_listDefaultDisplay.Find((MainWindow.DisplayMode i) => (long)i.Width == (long)((ulong)nXX) && (long)i.Height == (long)((ulong)nYY));
                    if (displayMode.Width != 0)
                    {
                        list.Add(displayMode);
                    }
                    num = nXX;
                    num2 = nYY;
                }
            }
            MainWindow.DisplayModes item = default(MainWindow.DisplayModes);
            item.list = list;
            m_listCurrentDisplay.Add(item);
            if (instances.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Failed to list screens");
                return;
            }
            if (m_listCurrentDisplay.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Failed to list supported resolutions");
            }
        }

        void getDisplayMode_EnumDisplaySettings(int numDisplay)
        {

            var display_DEVICE = default(DISPLAY_DEVICE);
            display_DEVICE.cb = Marshal.SizeOf(display_DEVICE);
            var allDisplayNames = new List<string>();
            var dispNum = 0u;
            while (EnumDisplayDevices(null, dispNum, ref display_DEVICE, 1u))
            {
                if ((display_DEVICE.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) ==
                    DisplayDeviceStateFlags.AttachedToDesktop) allDisplayNames.Add(display_DEVICE.DeviceName);
                dispNum += 1u;
            }

            var primaryIndex = -1;
            for (var currentDisp = 0; currentDisp < allDisplayNames.Count; currentDisp++)
            {
                var displayName = allDisplayNames[currentDisp];
                var num4 = 0;
                var num5 = 0;
                var devmode = default(DEVMODE);
                var list2 = new List<DisplayMode>();
                var num6 = 0;
                while (EnumDisplaySettings(displayName, num6, ref devmode))
                {
                    var nXX = devmode.dmPelsWidth;
                    var nYY = devmode.dmPelsHeight;
                    if ((num4 != nXX || num5 != nYY) && devmode.dmBitsPerPel == 32)
                    {
                        var displayMode = m_listDefaultDisplay.Find(dis => dis.Width == nXX && dis.Height == nYY);
                        if (displayMode.Width != 0) list2.Add(displayMode);
                        num4 = nXX;
                        num5 = nYY;
                    }

                    num6++;
                }

                var item = default(DisplayModes);
                foreach (var monitorInfoEx in Screen.AllScreens)
                    if (monitorInfoEx.DeviceName == displayName)
                    {
                        item.x = monitorInfoEx.WorkingArea.Left;
                        item.y = monitorInfoEx.WorkingArea.Top;
                        if (monitorInfoEx.Primary) primaryIndex = currentDisp;
                    }

                item.list = list2;
                m_listCurrentDisplay.Add(item);
            }

            if (m_listCurrentDisplay.Count == 0 || m_listCurrentDisplay.Count != numDisplay)
                MessageBox.Show("Failed to list supported resolutions");

            if (primaryIndex < 0) return;
            m_listCurrentDisplay.Insert(0, m_listCurrentDisplay[primaryIndex]);
            m_listCurrentDisplay.RemoveAt(primaryIndex + 1);
        }

        static int DisplaySort(MainWindow.DisplayModes a, MainWindow.DisplayModes b)
        {
            if (a.x < b.x)
            {
                return -1;
            }
            if (a.x > b.x)
            {
                return 1;
            }
            if (a.y < b.y)
            {
                return -1;
            }
            if (a.y > b.y)
            {
                return 1;
            }
            return 0;
        }

        static MainWindow.MonitorInfoEx[] GetMonitors()
        {
            List<MainWindow.MonitorInfoEx> list = new List<MainWindow.MonitorInfoEx>();
            MainWindow.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate (IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData)
            {
                MainWindow.MonitorInfoEx item = new MainWindow.MonitorInfoEx
                {
                    cbSize = Marshal.SizeOf(typeof(MainWindow.MonitorInfoEx))
                };
                MainWindow.GetMonitorInfo(hMonitor, ref item);
                list.Add(item);
            }, IntPtr.Zero);
            return list.ToArray();
        }

        void setFullScreenDevice()
        {
            int nDisplay = m_Setting.m_nDisplay;
            if (m_listCurrentDisplay[nDisplay].list.Count == 0)
            {
                m_Setting.m_bFullScreen = false;
                toggleFullscreen.IsChecked = new bool?(false);
                System.Windows.Forms.MessageBox.Show("This monitor doesn't support fullscreen.");
                return;
            }
            if (m_listCurrentDisplay[nDisplay].list.Find((MainWindow.DisplayMode x) => x.text.Contains(m_Setting.m_strSizeChoose)).Width == 0)
            {
                m_Setting.m_strSizeChoose = m_listCurrentDisplay[nDisplay].list[0].text;
                m_Setting.m_nWidthChoose = m_listCurrentDisplay[nDisplay].list[0].Width;
                m_Setting.m_nHeightChoose = m_listCurrentDisplay[nDisplay].list[0].Height;
            }
            dropRes.Text = m_Setting.m_strSizeChoose;
        }

        public bool IsWow64()
        {
            bool flag;
            return MainWindow.GetProcAddress(MainWindow.GetModuleHandle("Kernel32.dll"), "IsWow64Process") != IntPtr.Zero && MainWindow.IsWow64Process(Process.GetCurrentProcess().Handle, out flag) && flag;
        }

        public bool Is64BitOS()
        {
            if (IntPtr.Size == 4)
            {
                return IsWow64();
            }
            return IntPtr.Size == 8;
        }

        void MenuCloseButton(object sender, EventArgs e)
        {
            saveConfigFile(m_strCurrentDir + m_strSaveDir);
            ReleaseMutex();
        }

        const int MONITORINFOF_PRIMARY = 1;

        string[] m_astrQuality;
        string[] s_EnglishTL;

        string m_strGameRegistry = "Software\\illusion\\PlayHome\\";
        string m_strStudioRegistry = "Software\\illusion\\PlayHomeStudio\\";
        string m_strGameExe = "PlayHome64bit.exe";
        string m_strStudioExe = "PlayHomeStudio64bit.exe";
        string m_strGameExe32 = "PlayHome32bit.exe";
        string m_strStudioExe32 = "PlayHomeStudio32bit.exe";
        string m_strVRExe = "VR GEDOU.exe";
        string m_strManualDir = "/manual/お読み下さい.html";
        string m_strStudioManualDir = "/manual_s/お読み下さい.html";
        string m_strVRManualDir = "/manual_vr/お読み下さい.html";

        const string m_strSaveDir = "/UserData/setup.xml";
        const string m_customDir = "/UserData/LauncherEN";

        const string m_strDefSizeText = "1280 x 720 (16 : 9)";
        const int m_nDefQuality = 1;
        const int m_nDefWidth = 1280;
        const int m_nDefHeight = 720;
        const bool m_bDefFullScreen = false;

        string m_strCurrentDir = Environment.CurrentDirectory + "\\";

        ConfigSetting m_Setting = new ConfigSetting();

        bool is64bitOS;

        bool isStudio;
        bool isMainGame;

        string lang = "en";
        bool noTL = false;
        bool startup;

        bool versionAvail;
        bool WarningExists;
        bool CharExists;
        bool BackgExists;
        bool PatreonExists;
        bool LangExists;
        bool kkmanExist;
        bool updatelocExists;
        bool x86;

        bool isIPA;
        bool isBepIn;

        string kkman;
        string updated = "placeholder";

        string q_performance = "Performance";
        string q_normal = "Normal";
        string q_quality = "Quality";
        string s_primarydisplay = "PrimaryDisplay";
        string s_subdisplay = "SubDisplay";

        const string decideLang = "/lang";
        const string versioningLoc = "/version";
        const string warningLoc = "/warning.txt";
        const string charLoc = "/Chara.png";
        const string backgLoc = "/LauncherBG.png";
        const string patreonLoc = "/patreon.txt";
        const string kkmdir = "/kkman.txt";
        const string updateLoc = "/updater.txt";
        //string updateURL;
        //string packVersion;
        //string newPackVersion;

        string patreonURL;

        List<MainWindow.DisplayMode> m_listDefaultDisplay = new List<MainWindow.DisplayMode>
        {
            new MainWindow.DisplayMode
            {
                Width = 854,
                Height = 480,
                text = "854 x 480 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 1024,
                Height = 576,
                text = "1024 x 576 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 1136,
                Height = 640,
                text = "1136 x 640 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 1280,
                Height = 720,
                text = "1280 x 720 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 1366,
                Height = 768,
                text = "1366 x 768 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 1536,
                Height = 864,
                text = "1536 x 864 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 1600,
                Height = 900,
                text = "1600 x 900 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 1920,
                Height = 1080,
                text = "1920 x 1080 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 2048,
                Height = 1152,
                text = "2048 x 1152 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 2560,
                Height = 1440,
                text = "2560 x 1440 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 3200,
                Height = 1800,
                text = "3200 x 1800 (16 : 9)"
            },
            new MainWindow.DisplayMode
            {
                Width = 3840,
                Height = 2160,
                text = "3840 x 2160 (16 : 9)"
            }
        };

        List<MainWindow.DisplayModes> m_listCurrentDisplay = new List<MainWindow.DisplayModes>();

        const int m_nQualityCount = 3;





        Mutex mutex;

        delegate void EnumDisplayMonitorsCallback(IntPtr hMonir, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData);

        internal struct MonitorInfoEx
        {
            public int cbSize;

            public MainWindow.Rect rcMonitor;

            public MainWindow.Rect rcWork;

            public int dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        public struct Rect
        {
            public int Left;

            public int Top;

            public int Right;

            public int Bottom;
        }

        struct DisplayMode
        {
            public int Width;

            public int Height;

            public string text;
        }

        struct DisplayModes
        {
            public int x;

            public int y;

            public List<MainWindow.DisplayMode> list;
        }

        void discord_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://discord.gg/F3bDEFE");
        }
        void patreon_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start(patreonURL);
        }

        void update_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            saveConfigFile(m_strCurrentDir + m_strSaveDir);
            SaveRegistry();

            string marcofix = m_strCurrentDir.TrimEnd('\\', '/', ' ');
            kkman = kkman.TrimEnd('\\', '/', ' ');
            string finaldir;

            if (!File.Exists($@"{kkman}\StandaloneUpdater.exe"))
            {
                finaldir = $@"{m_strCurrentDir}{kkman}";
            }
            else
            {
                finaldir = kkman;
            }

            string text = $@"{finaldir}\StandaloneUpdater.exe";

            string argdir = $"\u0022{marcofix}\u0022";
            string argloc = updated;
            string args = $"{argdir} {argloc}";

            if (!updatelocExists)
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
            if (!noTL)
                ChangeTL(language);
            else
                SetupLang(language);
        }

        void ChangeTL(string language)
        {
            deactivateTL(1);
            WriteLangIni(language);
            SetupLang(language);
        }

        void WriteLangIni(string language)
        {
            if (File.Exists(m_strCurrentDir + "BepInEx/Config/AutoTranslatorConfig.ini"))
            {
                if (System.Windows.MessageBox.Show("Do you want the ingame language to reflect this language choice?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    helvete(language);
                }
                // Borrowed from Marco
            }
            //MessageBox.Show($"{curAutoTLOut}", "Debug");
        }

        void helvete(string language)
        {
            if (File.Exists("BepInEx/Config/AutoTranslatorConfig.ini"))
            {
                var ud = Path.Combine(m_strCurrentDir, @"BepInEx/Config/AutoTranslatorConfig.ini");

                try
                {
                    var contents = File.ReadAllLines(ud).ToList();

                    // Fix VMD for darkness
                    var setToLanguage = contents.FindIndex(s => s.ToLower().Contains("[General]".ToLower()));
                    if (setToLanguage >= 0)
                    {
                        var i = contents.FindIndex(setToLanguage, s => s.StartsWith("Language"));
                        if (i > setToLanguage)
                            contents[i] = $"Language={language}";
                        else
                        {
                            contents.Insert(setToLanguage + 1, $"Language={language}");
                        }
                    }
                    else
                    {
                        contents.Add("");
                        contents.Add("[General]");
                        contents.Add($"Language={language}");
                    }

                    File.WriteAllLines(ud, contents.ToArray());
                }
                catch (Exception e)
                {
                    MessageBox.Show("Something went wrong: " + e);
                }
            }
        }

        void deactivateTL(int i)
        {
            s_EnglishTL = new string[]
            {
                "BepInEx/XUnity.AutoTranslator.Plugin.BepIn",
                "BepInEx/XUnity.AutoTranslator.Plugin.Core",
                "BepInEx/XUnity.AutoTranslator.Plugin.ExtProtocol",
                "BepInEx/XUnity.RuntimeHooker.Core",
                "BepInEx/XUnity.RuntimeHooker",
                "BepInEx/KK_Subtitles",
                "BepInEx/ExIni"
            };

            if (i == 0)
            {
                foreach (var file in s_EnglishTL)
                {
                    if (File.Exists(m_strCurrentDir + file + ".dll"))
                    {
                        File.Move(m_strCurrentDir + file + ".dll", m_strCurrentDir + file + "._ll");
                    }
                }
            }
            else
            {
                foreach (var file in s_EnglishTL)
                {
                    if (File.Exists(m_strCurrentDir + file + "._ll"))
                    {
                        File.Move(m_strCurrentDir + file + "._ll", m_strCurrentDir + file + ".dll");
                    }
                    helvete("en");
                }
            }
        }

        void SetupLang(string langstring)
        {
            if (File.Exists(m_strCurrentDir + m_customDir + decideLang))
            {
                File.Delete(m_strCurrentDir + m_customDir + decideLang);
            }
            using (StreamWriter writetext = new StreamWriter(m_strCurrentDir + m_customDir + decideLang))
            {
                writetext.WriteLine(langstring);
            }
            System.Windows.Forms.Application.Restart();
        }

        private void modeDev_Checked(object sender, RoutedEventArgs e)
        {
            using (StreamWriter writetext = new StreamWriter(m_strCurrentDir + m_customDir + "/devMode"))
            {
                writetext.WriteLine("devmode: True");
            }
            if (!startup)
            {
                devMode(true);
            }
        }

        private void modeDev_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!startup)
            {
                devMode(false);
            }
        }

        void devMode(bool setDev)
        {
            var ud = Path.Combine(m_strCurrentDir, @"BepInEx\config\BepInEx.cfg");

            try
            {
                var contents = File.ReadAllLines(ud).ToList();

                var setToLanguage = contents.FindIndex(s => s.ToLower().Contains("[Logging.Console]".ToLower()));
                if (setToLanguage >= 0 && setDev)
                {
                    var i = contents.FindIndex(setToLanguage, s => s.StartsWith("Enabled"));
                    if (i > setToLanguage)
                        contents[i] = $"Enabled = true";
                }
                else
                {
                    var i = contents.FindIndex(setToLanguage, s => s.StartsWith("Enabled"));
                    if (i > setToLanguage)
                        contents[i] = $"Enabled = false";
                }

                File.WriteAllLines(ud, contents.ToArray());
            }
            catch (Exception e)
            {
                MessageBox.Show("Something went wrong: " + e);
            }
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void HoneyPotInspector_Run(object sender, RoutedEventArgs e)
        {
            if(File.Exists($"{m_strCurrentDir}\\HoneyPot\\HoneyPotInspector.exe"))
            {
                Process.Start($"{m_strCurrentDir}\\HoneyPot\\HoneyPotInspector.exe");
            }
            else
            {
                MessageBox.Show("HoneyPot doesn't seem to be applied to this installation.");
            }
        }

        private void dhh_Checked(object sender, RoutedEventArgs e)
        {
            if (File.Exists($"{m_strCurrentDir}\\Plugins\\ProjectHighHeel.dl_"))
            {
                if (File.Exists($"{m_strCurrentDir}\\Plugins\\ProjectHighHeel.dll"))
                {
                    File.Delete($"{m_strCurrentDir}\\Plugins\\ProjectHighHeel.dll");
                }
                File.Move($"{m_strCurrentDir}\\Plugins\\ProjectHighHeel.dl_", $"{m_strCurrentDir}\\Plugins\\ProjectHighHeel.dll");
            }
        }

        private void dhh_Unchecked(object sender, RoutedEventArgs e)
        {
            if(File.Exists($"{m_strCurrentDir}\\Plugins\\ProjectHighHeel.dll"))
            {
                if (File.Exists($"{m_strCurrentDir}\\Plugins\\ProjectHighHeel.dl_"))
                {
                    File.Delete($"{m_strCurrentDir}\\Plugins\\ProjectHighHeel.dl_");
                }
                File.Move($"{m_strCurrentDir}\\Plugins\\ProjectHighHeel.dll", $"{m_strCurrentDir}\\Plugins\\ProjectHighHeel.dl_");
            }
        }

        private void hp_Checked(object sender, RoutedEventArgs e)
        {
            if(File.Exists($"{m_strCurrentDir}\\Plugins\\HoneyPot.dl_"))
                MessageBox.Show("When HoneyPot is enabled, the game will use a bit longer to load in some scenes due to checking for HoneySelect assets, making it appear to be freezing for a few seconds. This is completely normal.\n\nJust disable this option again if you would rather not have that freeze.", "Information");
            if (File.Exists($"{m_strCurrentDir}\\Plugins\\HoneyPot.dl_"))
            {
                if (File.Exists($"{m_strCurrentDir}\\Plugins\\HoneyPot.dll"))
                {
                    File.Delete($"{m_strCurrentDir}\\Plugins\\HoneyPot.dll");
                }
                File.Move($"{m_strCurrentDir}\\Plugins\\HoneyPot.dl_", $"{m_strCurrentDir}\\Plugins\\HoneyPot.dll");
            }
            
        }

        private void hp_Unchecked(object sender, RoutedEventArgs e)
        {
            if (File.Exists($"{m_strCurrentDir}\\Plugins\\HoneyPot.dll"))
            {
                if (File.Exists($"{m_strCurrentDir}\\Plugins\\HoneyPot.dl_"))
                {
                    File.Delete($"{m_strCurrentDir}\\Plugins\\HoneyPot.dl_");
                }
                File.Move($"{m_strCurrentDir}\\Plugins\\HoneyPot.dll", $"{m_strCurrentDir}\\Plugins\\HoneyPot.dl_");
            }
        }

        private void toggle32_Checked(object sender, RoutedEventArgs e)
        {
            x86 = true;
            if (!File.Exists($"{m_customDir}{m_customDir}/toggle32.txt"))
            {
                using (StreamWriter writetext = new StreamWriter($"{m_strCurrentDir}{m_customDir}/toggle32.txt"))
                {
                    writetext.WriteLine("x86");
                }
            }
        }

        private void toggle32_Unchecked(object sender, RoutedEventArgs e)
        {
            x86 = false;
            if (File.Exists($"{m_strCurrentDir}{m_customDir}/toggle32.txt"))
            {
                File.Delete($"{m_strCurrentDir}{m_customDir}/toggle32.txt");
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}