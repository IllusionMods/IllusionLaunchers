using System.Windows;

namespace InitSetting
{
    public partial class MainWindow
    {
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
            if (EnvironmentHelper.Language == "ja")
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
                _qPerformance = "パフォーマンス";
                _qNormal = "ノーマル";
                _qQuality = "クオリティ";
                _sPrimarydisplay = "メインディスプレイ";
                _sSubdisplay = "サブディスプレイ";
                labelDiscord.Content = "Discordを訪問";
                labelPatreon.Content = "Patreonを訪問";
                labelUpdate.Content = "ゲームを更新する";

                // AIS Exclusive
                buttonHousing.Content = "家";
                toggleDHH.Content = "DHHを有効にする";
            }
            else if (EnvironmentHelper.Language == "zh-CN") // By @Madevil#1103 & @𝐄𝐀𝐑𝐓𝐇𝐒𝐇𝐈𝐏 💖#4313 
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
                _qPerformance = "性能";
                _qNormal = "标准";
                _qQuality = "高画质";
                _sPrimarydisplay = "主显示器";
                _sSubdisplay = "次显示器";
                labelDiscord.Content = "前往Discord";
                labelPatreon.Content = "前往Patreon";
                labelUpdate.Content = "更新游戏";

                // AIS Exclusive
                buttonHousing.Content = "房子";
                toggleDHH.Content = "激活DHH";
            }
            else if (EnvironmentHelper.Language == "ko") // By @Keris-#1903 
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
                _qPerformance = "퍼포먼스";
                _qNormal = "일반";
                _qQuality = "퀄리티";
                _sPrimarydisplay = "주 디스플레이";
                _sSubdisplay = "서브 디스플레이";
                labelDiscord.Content = "Discord 방문";
                labelPatreon.Content = "Patreon 방문";
                labelUpdate.Content = "게임 업데이트";

                // AIS Exclusive
                buttonHousing.Content = "하우징 폴더";
                toggleDHH.Content = "DHH 활성화";
            }
            else if (EnvironmentHelper.Language == "es") // By @Heroine Nisa#3207
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
                _qPerformance = "Rendimiento";
                _qNormal = "Normal";
                _qQuality = "Calidad";
                _sPrimarydisplay = "Pantalla Primaria";
                _sSubdisplay = "Pantalla Secundaria";
                labelDiscord.Content = "visita la Discord";
                labelPatreon.Content = "visita la Patreon";
                labelUpdate.Content = "Actualizar";

                // AIS Exclusive
                buttonHousing.Content = "Casas";
                toggleDHH.Content = "Activar DHH";
            }
            else if (EnvironmentHelper.Language == "pt") // By @Neptune#1989 
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
                _qPerformance = "Baixo";
                _qNormal = "Normal";
                _qQuality = "Alto";
                _sPrimarydisplay = "Display Primário";
                _sSubdisplay = "Display Secundário";
                labelDiscord.Content = "Visitar Discord";
                labelPatreon.Content = "Visitar Patreon";
                labelUpdate.Content = "Atualizar";

                // AIS Exclusive
                buttonHousing.Content = "Casas";
                toggleDHH.Content = "Ativar DHH";
            }
            else if (EnvironmentHelper.Language == "fr") // By VaizravaNa#2315
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
                _qPerformance = "Performance";
                _qNormal = "Normal";
                _qQuality = "Qualité";
                _sPrimarydisplay = "Ecran principal";
                _sSubdisplay = "Ecran secondaire";
                labelDiscord.Content = "Visiter la Discord";
                labelPatreon.Content = "Visiter la Patreon";
                labelUpdate.Content = "Mise à jour";

                // AIS Exclusive
                buttonHousing.Content = "Plans des maisons";
                toggleDHH.Content = "Activer DHH";
            }
            else if (EnvironmentHelper.Language == "de") // By @DONTFORGETME#6198 
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
                _qPerformance = "Leistung";
                _qNormal = "Normal";
                _qQuality = "Qualität";
                _sPrimarydisplay = "Primär Bildschirm";
                _sSubdisplay = "Neben Bildschrim";
                labelDiscord.Content = "Besuche die Discord";
                labelPatreon.Content = "Besuche die Patreon";
                labelUpdate.Content = "Aktualisieren";

                // AIS Exclusive
                buttonHousing.Content = "Häuser";
                toggleDHH.Content = "DHH umschalten";
            }
            else if (EnvironmentHelper.Language == "no") // By @SmokeOfC|女神様の兄様#1984
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
                _qPerformance = "Ytelsesmodus";
                _qNormal = "Normalmodus";
                _qQuality = "Kvalitetsmodus";
                _sPrimarydisplay = "Hovedskjerm";
                _sSubdisplay = "Subskjerm";
                labelDiscord.Content = "Besøk Discord";
                labelPatreon.Content = "Besøk Patreon";
                labelUpdate.Content = "Oppdater";

                // AIS Exclusive
                buttonHousing.Content = "Hus";
                toggleDHH.Content = "Aktiver DHH";
            }

            _mAstrQuality = new[]
            {
                _qPerformance,
                _qNormal,
                _qQuality
            };
        }
    }
}