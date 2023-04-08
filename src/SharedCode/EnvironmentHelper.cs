using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;

namespace InitSetting
{
    public static class EnvironmentHelper
    {
        private static readonly string _mStrSaveDir = "/UserData/setup.xml";
        private static readonly string _mCustomDir = "/BepInEx/LauncherEN";
        private static readonly string _decideLang = "/lang";
        private static readonly string _versioningLoc = "/version";
        private static readonly string _warningLoc = "/warning.txt";
        private static readonly string _charLoc = "/Chara.png";
        private static readonly string _backgLoc = "/LauncherBG.png";
        private static readonly string _patreonLoc = "/patreon.txt";
        private static readonly string _kkmdir = "/kkman.txt";
        private static readonly string _updateLoc = "/updater.txt";
        private static bool _langExists;
        /// <summary>
        /// Absolute path to kkmanager
        /// </summary>
        private static string _kkmanagerDirectory;
        private static string _updateSourcesOverride;
        private static string[] _builtinLanguages;

        public static CultureInfo Language { get; private set; }

        public static bool KKmanExist => !string.IsNullOrEmpty(_kkmanagerDirectory);
        public static bool IsIpa { get; private set; }
        public static bool IsBepIn { get; private set; }
        public static string PatreonUrl { get; private set; }
        public static string VersionString { get; private set; }
        public static string WarningString { get; private set; }
        public static BitmapFrame CustomBgImage { get; private set; }
        public static BitmapFrame CustomCharacterImage { get; private set; }

        public static string BepinPluginsDir { get; private set; }
        public static string IPAPluginsDir { get; private set; }
        public static string GameRootDirectory { get; private set; }
        public static string ILikeBleeding { get; private set; }
        public static bool SideloaderMaintainerMode { get; private set; }

        public static bool SideloaderMaintainerEnabled
        {
            get
            {
                return Directory.Exists($"{GameRootDirectory}\\mods.prod");
            }
            set
            {
                try
                {
                    var NormMods = $"{GameRootDirectory}\\mods";
                    var ProdMods = $"{GameRootDirectory}\\mods.prod";
                    var TestMods = $"{GameRootDirectory}\\mods.test";
                    if (value)
                    {
                        Directory.Move(NormMods, ProdMods);
                        Directory.Move(TestMods, NormMods);
                    }
                    else
                    {
                        Directory.Move(NormMods, TestMods);
                        Directory.Move(ProdMods, NormMods);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(Localizable.SomethingWentWrong + " " + e);
                }
            }
        }

        public static bool BleedingModeEnabled
        {
            get => File.Exists(ILikeBleeding);
            set
            {
                var productionModsDir = $"{EnvironmentHelper.GameRootDirectory}\\mods\\";
                var experimentalDir = $"{EnvironmentHelper.GameRootDirectory}\\mods.experimental\\";
                try
                {
                    if (value)
                    {
                        var bleedingWriter = new StreamWriter(ILikeBleeding);
                        bleedingWriter.WriteLine("This file indicates that this game accepts experimental updates.\nPlease delete this file to stop accepting experimental updates.");
                        bleedingWriter.Close();
                        if (Directory.Exists($"{experimentalDir}Sideloader Modpack - Bleeding Edge"))
                        {
                            if (System.Windows.MessageBox.Show(
                                    Localizable.WarningBleeding,
                                    Localizable.QuestionBleeding, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                                MessageBoxResult.Yes)
                            {
                                if (!Directory.Exists($"{experimentalDir}"))
                                    Directory.CreateDirectory(experimentalDir);
                                Directory.Move(
                                    $"{experimentalDir}Sideloader Modpack - Bleeding Edge",
                                    $"{productionModsDir}Sideloader Modpack - Bleeding Edge");
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show(
                                Localizable.InfoBleeding,
                                Localizable.TypeInfo, MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        File.Delete(ILikeBleeding);
                        if (Directory.Exists($"{productionModsDir}Sideloader Modpack - Bleeding Edge"))
                        {
                            if (System.Windows.MessageBox.Show(
                                    Localizable.QuestionBleedingDisable,
                                    Localizable.QuestionBleedingDisableTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                                MessageBoxResult.Yes)
                            {
                                if (!Directory.Exists($"{experimentalDir}"))
                                    Directory.CreateDirectory(experimentalDir);
                                Directory.Move(
                                    $"{productionModsDir}Sideloader Modpack - Bleeding Edge",
                                    $"{experimentalDir}Sideloader Modpack - Bleeding Edge");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(Localizable.SomethingWentWrong + " " + e);
                }
            }
        }

        public static bool DeveloperModeEnabled
        {
            get
            {
                var configPath = Path.Combine(GameRootDirectory, @"BepInEx\config\BepInEx.cfg");
                if (!File.Exists(configPath)) return false;
                try
                {
                    var contents = File.ReadAllLines(configPath).ToList();

                    var devmodeCatIndex = contents.FindIndex(s => s.ToLower().Contains("[Logging.Console]".ToLower()));
                    if (devmodeCatIndex >= 0)
                    {
                        var toCheck = contents.Skip(devmodeCatIndex);
                        var nextCatIndex = contents.FindIndex(devmodeCatIndex + 1, s => s.StartsWith("["));
                        if (nextCatIndex > 0) toCheck = toCheck.Take(nextCatIndex - devmodeCatIndex);
                        return toCheck.Any(s => s.StartsWith("Enabled = true", StringComparison.OrdinalIgnoreCase));
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(Localizable.SomethingWentWrong + " " + e);
                }

                return false;
            }
            set
            {
                try
                {
                    var configPath = Path.Combine(GameRootDirectory, @"BepInEx\config\BepInEx.cfg");
                    var contents = File.Exists(configPath) ? File.ReadAllLines(configPath).ToList() : new List<string>();

                    var devmodeCatIndex = contents.FindIndex(s => s.ToLower().Contains("[Logging.Console]".ToLower()));
                    if (devmodeCatIndex >= 0)
                    {
                        var nextCatIndex = contents.FindIndex(devmodeCatIndex + 1, s => s.StartsWith("["));
                        int enabledIndex;
                        if (nextCatIndex > 0)
                            enabledIndex = contents.FindIndex(devmodeCatIndex, nextCatIndex - devmodeCatIndex, s => s.StartsWith("Enabled"));
                        else
                            enabledIndex = contents.FindIndex(devmodeCatIndex, s => s.StartsWith("Enabled"));

                        if (enabledIndex > 0)
                            contents[enabledIndex] = "Enabled = " + (value ? "true" : "false");
                        else
                            contents.Insert(devmodeCatIndex + 1, "Enabled = " + (value ? "true" : "false"));
                    }
                    else
                    {
                        contents.Add("[Logging.Console]");
                        contents.Add("Enabled = " + (value ? "true" : "false"));
                    }

                    File.WriteAllLines(configPath, contents.ToArray());
                }
                catch (Exception e)
                {
                    MessageBox.Show(Localizable.SomethingWentWrong + " " + e);
                }
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] IntPtr hProcess, out bool lpSystemInfo);

        public static bool IsWow64()
        {
            return GetProcAddress(GetModuleHandle("Kernel32.dll"), "IsWow64Process") != IntPtr.Zero &&
                   IsWow64Process(Process.GetCurrentProcess().Handle, out var flag) && flag;
        }

        public static bool Is64BitOs()
        {
            if (IntPtr.Size == 4) return IsWow64();
            return IntPtr.Size == 8;
        }

        public static void WarnRes(string resText)
        {
            if (!resText.Contains("(16 : 9)"))
                MessageBox.Show(Localizable.RatioWarning, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void SetLanguage(string language)
        {
            try
            {
                using (var writetext = new StreamWriter(GameRootDirectory + _mCustomDir + _decideLang, false))
                    writetext.WriteLine(language);

                string LangQ_a = "Do you want to set the ingame language to the selected language as well?";
                string LangQ_b = "Question";

                switch (language)
                {
                    case "ja-JP":
                        LangQ_a = "ゲームにこの言語の選択を反映させたいですか？";
                        LangQ_b = "質問";
                        break;
                    case "zh-CN":
                        LangQ_a = "您是否希望游戏中的语言反映这项语言选择？";
                        LangQ_b = "问题";
                        break;
                    case "zh-TW":
                        LangQ_a = "您是否希望遊戲中的語言反映這項語言選擇？";
                        LangQ_b = "問題";
                        break;
                    case "ko-KR":
                        LangQ_a = "게임 언어를 선택한 언어로 설정 하시겠습니까?";
                        LangQ_b = "질문";
                        break;
                    case "es-ES":
                        LangQ_a = "¿Desea configurar el idioma del juego al idioma seleccionado también?";
                        LangQ_b = "Pregunta";
                        break;
                    case "pt-PT":
                        LangQ_a = "Deseja também definir o idioma do jogo para o idioma selecionado?";
                        LangQ_b = "Pergunta";
                        break;
                    case "fr-FR":
                        LangQ_a = "Voulez-vous également définir la langue du jeu sur la langue sélectionnée?";
                        LangQ_b = "Question";
                        break;
                    case "de-DE":
                        LangQ_a = "Möchten Sie die Spielsprache auch auf die ausgewählte Sprache einstellen?";
                        LangQ_b = "Frage";
                        break;
                    case "no-NB":
                        LangQ_a = "Ønsker du å endre språket i spillet også?";
                        LangQ_b = "Spørsmål";
                        break;
                    case "ru":
                        LangQ_a = "Установить в игре язык, который выбран в лаунчере?";
                        LangQ_b = "вопрос";
                        break;
                    default:
                        LangQ_a = "Do you want to set the ingame language to the selected language as well?";
                        LangQ_b = "Question";
                        break;
                }

                if (EnvironmentHelper.IsBepIn)
                {

                    if (System.Windows.MessageBox.Show(LangQ_a,
                        LangQ_b, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        var builtinIndex = _builtinLanguages.ToList()
                            .FindIndex(x => language.Equals(x, StringComparison.OrdinalIgnoreCase));

                        // Set built-in game language if supported
                        if (builtinIndex >= 0)
                        {
                            SettingManager.CurrentSettings.Language = builtinIndex;
                            SettingManager.SaveSettings();
                        }
                        else if(!File.Exists(
                                    $"{EnvironmentHelper.GameRootDirectory}/BepInEx/Translation/{language}/DisableGoogleWarn.txt") &&
                                !File.Exists(
                                    $"{EnvironmentHelper.GameRootDirectory}/BepInEx/Translation/{language}/DisableGoogle.txt"))
                        {
                            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(language);
                            MessageBox.Show(Localizable.InstructDecideLang);
                        }

                        WriteAutoTranslatorLangIni(language);
                    }
                }

                Application.Restart();
            }
            catch (Exception e)
            {
                MessageBox.Show(Localizable.SomethingWentWrongLang + " " + e);
            }
        }

        private static void WriteAutoTranslatorLangIni(string language)
        {
            var configPath = Path.Combine(GameRootDirectory, @"BepInEx/Config/AutoTranslatorConfig.ini");

            var disable = language.Equals("jp-JP", StringComparison.OrdinalIgnoreCase);

            if (language != "zh-CN" && language != "zh-TW")
                language = language.Split('-')[0];

            if (File.Exists($"{EnvironmentHelper.GameRootDirectory}/BepInEx/Translation/{language}/DisableGoogle.txt"))
                disable = true;

            try
            {
                var contents = (File.Exists(configPath) ? File.ReadAllLines(configPath) : Enumerable.Empty<string>()).ToList();
                string TransService = String.Empty;
                string Font = String.Empty;
                string TextMeshFont = String.Empty;

                TransService = language == "ko" ? "PapagoTranslate" : "GoogleTranslateV2";

                if (language == "ru") Font = $@"Times New Roman";
                else if (language == "zh-CN" || language == "zh-TW") Font = $@"MS Gothic";
                else Font = String.Empty;

                // TextMeshFont = (language == "ko" || language == "zh-CN" || language == "zh-TW") 
                //               && File.Exists($@"{EnvironmentHelper.GameRootDirectory}\BepInEx\Translation\fonts\arialuni_sdf_u2018") ? $@"BepInEx\Translation\fonts\arialuni_sdf_u2018" : String.Empty;

                if (File.Exists($"{EnvironmentHelper.GameRootDirectory}/BepInEx/Translation/{language}/UseFont.txt"))
                {
                    string currentFont = File.ReadAllText($"{EnvironmentHelper.GameRootDirectory}/BepInEx/Translation/{language}/UseFont.txt");
                    TextMeshFont = File.Exists($@"{EnvironmentHelper.GameRootDirectory}\BepInEx\Translation\fonts\{currentFont}") ? $@"BepInEx\Translation\fonts\{currentFont}" : String.Empty;
                }

                // Setting language
                {
                    var categoryIndex = contents.FindIndex(s => s.ToLower().Contains("[General]".ToLower()));
                    if (categoryIndex >= 0)
                    {
                        var i = contents.FindIndex(categoryIndex, s => s.StartsWith("Language"));
                        if (i > categoryIndex)
                            contents[i] = $"Language={language}";
                        else
                            contents.Insert(categoryIndex + 1, $"Language={language}");
                    }
                    else
                    {
                        contents.Add("");
                        contents.Add("[General]");
                        contents.Add($"Language={language}");
                    }
                }
                // Setting Translation Service
                {
                    var categoryIndex = contents.FindIndex(s => s.ToLower().Contains("[Service]".ToLower()));
                    if (categoryIndex >= 0)
                    {
                        var i = contents.FindIndex(categoryIndex, s => s.StartsWith("Endpoint"));
                        if (i > categoryIndex)
                            contents[i] = disable ? "Endpoint=" : $"Endpoint={TransService}";
                        else
                            contents.Insert(categoryIndex + 1, disable ? "Endpoint=" : "Endpoint=GoogleTranslate");
                    }
                    else
                    {
                        contents.Add("");
                        contents.Add("[Service]");
                        contents.Add(disable ? "Endpoint=" : $"Endpoint={TransService}");
                    }
                }
                // Setting font
                {
                    var categoryIndex = contents.FindIndex(s => s.ToLower().Contains("[Behaviour]".ToLower()));
                    if (categoryIndex >= 0)
                    {
                        var i = contents.FindIndex(categoryIndex, s => s.StartsWith("OverrideFont"));
                        if (i > categoryIndex)
                            contents[i] = $"OverrideFont={Font}";
                    }
                    else
                    {
                        contents.Add("");
                        contents.Add("[Behaviour]");
                        contents.Add(disable ? "OverrideFont=" : $"OverrideFont={Font}");
                    }
                }
                // Setting TextMeshfont
                {
                    var categoryIndex = contents.FindIndex(s => s.ToLower().Contains("[Behaviour]".ToLower()));
                    if (categoryIndex >= 0)
                    {
                        var i = contents.FindIndex(categoryIndex, s => s.StartsWith("OverrideFontTextMeshPro"));
                        if (i > categoryIndex)
                            contents[i] = $"OverrideFontTextMeshPro={TextMeshFont}";
                    }
                    else
                    {
                        contents.Add("");
                        contents.Add("[Behaviour]");
                        contents.Add(disable ? "OverrideFontTextMeshPro=" : $"OverrideFontTextMeshPro={TextMeshFont}");
                    }
                }

                Directory.CreateDirectory(Path.GetDirectoryName(configPath));
                File.WriteAllLines(configPath, contents.ToArray());
            }
            catch (Exception e)
            {
                MessageBox.Show(Localizable.SomethingWentWrongLang + " " + e);
            }
        }

        private static void CheckDuplicateStartup()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var dupl = Process.GetProcessesByName(process.ProcessName);
                if (true)
                    foreach (var p in dupl)
                        if (p.Id != process.Id)
                            p.Kill();
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception occurred:", e);
            }
        }

        public static void Initialize(string[] builtinLanguages)
        {
            _builtinLanguages = builtinLanguages ?? throw new ArgumentNullException(nameof(builtinLanguages));

            InitializeDirectories();
            InitializeLanguage();
            CheckDuplicateStartup();

            // Sideloader Modpack Maintainer Mode
            SideloaderMaintainerMode = Directory.Exists($"{GameRootDirectory}\\mods.prod") || Directory.Exists($"{GameRootDirectory}\\mods.test");

            // Framework test
            IsIpa = File.Exists($"{GameRootDirectory}\\IPA.exe");
            IsBepIn = Directory.Exists($"{GameRootDirectory}\\BepInEx\\core");

            if (IsIpa && IsBepIn)
            {
                MessageBox.Show(
                    Localizable.WarningFramework,
                    Localizable.TypeCritical);

                System.Windows.Application.Current.Shutdown();
            }

            // Updater / kkmanager
            try
            {
                var kkmanFileDir = Path.GetFullPath(GameRootDirectory + _mCustomDir + _kkmdir);
                // If config file doesn't exist try to find kkmanager inside of game directory
                if (!File.Exists(kkmanFileDir))
                {
                    var f = Directory.GetFiles(GameRootDirectory, "KKManager.exe", SearchOption.AllDirectories)
                        .Select(Path.GetDirectoryName)
                        .Select(x => x.Substring(GameRootDirectory.Length).Trim('\\', '/'))
                        .FirstOrDefault();
                    File.WriteAllText(kkmanFileDir, f ?? string.Empty, Encoding.UTF8);
                }

                // Figure out where kkmanager is installed
                var kkmanpath = File.ReadAllLines(kkmanFileDir, Encoding.UTF8)
                    .FirstOrDefault(x => !string.IsNullOrEmpty(x));
                if (!string.IsNullOrEmpty(kkmanpath))
                {
                    kkmanpath = kkmanpath.Trim('\\', '/');
                    if (!Path.IsPathRooted(kkmanpath))
                    {
                        var rootedPath = Path.GetFullPath(GameRootDirectory + '\\' + kkmanpath);
                        kkmanpath = Directory.Exists(rootedPath) ? rootedPath : Path.GetFullPath(kkmanpath);
                    }
                }

                if (Directory.Exists(kkmanpath))
                    _kkmanagerDirectory = kkmanpath;
                else
                    File.Delete(kkmanFileDir);

                if (KKmanExist)
                {
                    var updatecfgPath = Path.Combine(GameRootDirectory, _mCustomDir + _updateLoc);
                    _updateSourcesOverride = File.Exists(updatecfgPath)
                        ? File.ReadAllLines(updatecfgPath, Encoding.UTF8).FirstOrDefault(x => !string.IsNullOrEmpty(x))
                        : null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    Localizable.WarningKKM + ex,
                    Localizable.TypeCritical, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            if (GameRootDirectory.Length >= 75)
                MessageBox.Show(
                    Localizable.WarningLenght,
                    Localizable.TypeCritical);

            // Customization options

            var versionPath = Path.GetFullPath(GameRootDirectory + _versioningLoc);
            if (File.Exists(versionPath))
            {
                var verFileStream = new FileStream(versionPath, FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(verFileStream, Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null) VersionString = line;
                }

                verFileStream.Close();
            }

            var warningExists = File.Exists(GameRootDirectory + _mCustomDir + _warningLoc);
            if (warningExists)
                try
                {
                    using (var sr = new StreamReader(GameRootDirectory + _mCustomDir + _warningLoc))
                    {
                        var line = sr.ReadToEnd();
                        WarningString = line;
                    }
                }
                catch (IOException e)
                {
                    WarningString = e.Message;
                }

            var charExists = File.Exists(GameRootDirectory + _mCustomDir + _charLoc);
            if (charExists)
            {
                var urich = new Uri(GameRootDirectory + _mCustomDir + _charLoc, UriKind.RelativeOrAbsolute);
                CustomCharacterImage = BitmapFrame.Create(urich);
            }

            var backgExists = File.Exists(GameRootDirectory + _mCustomDir + _backgLoc);
            if (backgExists)
            {
                var uribg = new Uri(GameRootDirectory + _mCustomDir + _backgLoc, UriKind.RelativeOrAbsolute);
                CustomBgImage = BitmapFrame.Create(uribg);
            }

            var patreonExists = File.Exists(GameRootDirectory + _mCustomDir + _patreonLoc);
            if (patreonExists)
            {
                var verFileStream = new FileStream(GameRootDirectory + _mCustomDir + _patreonLoc, FileMode.Open,
                    FileAccess.Read);
                using (var streamReader = new StreamReader(verFileStream, Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null) PatreonUrl = line;
                }

                verFileStream.Close();
            }
        }

        private static void InitializeDirectories()
        {
            var currentDirectory = Path.GetDirectoryName(typeof(MainWindow).Assembly.Location) ??
                                   Environment.CurrentDirectory;
            GameRootDirectory = currentDirectory + "\\";
            ILikeBleeding = $"{EnvironmentHelper.GameRootDirectory}\\BepInEx\\LauncherEN\\ilikebleeding.txt";

            Directory.CreateDirectory(GameRootDirectory + _mCustomDir);

            BepinPluginsDir = $"{GameRootDirectory}\\BepInEx\\Plugins\\";
            IPAPluginsDir = $"{GameRootDirectory}\\Plugins\\";
        }

        private static void InitializeLanguage()
        {
            var launcherPath = GameRootDirectory + _mCustomDir + _decideLang;
            _langExists = File.Exists(launcherPath);
            if (_langExists)
            {
                try { Language = CultureInfo.GetCultureInfo(File.ReadAllLines(launcherPath)[0]); }
                catch { }

            }

            if (Language == null) Language = CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = Language;
            Thread.CurrentThread.CurrentCulture = Language;
        }

        public static string GetConfigFilePath()
        {
            return GameRootDirectory + _mStrSaveDir;
        }

        public static void ShowManual(string manualRoot)
        {
            var manualEn = manualRoot + "manual_en.html";
            var manualLang = manualRoot + $"manual_{Language}.html";
            var manualJa = manualRoot + "お読み下さい.html";

            Exception ex = null;
            if (File.Exists(manualLang))
            {
                try { Process.Start(manualLang); }
                catch (Exception e)
                {
                    ex = e;
                }
                return;
            }
            if (File.Exists(manualEn))
            {
                try
                {
                    Process.Start(manualEn);
                    return;
                }
                catch (Exception e) { ex = e; }
            }
            if (File.Exists(manualJa))
            {
                try
                {
                    Process.Start(manualJa);
                    return;
                }
                catch (Exception e) { ex = e; }
            }

            try
            {
                var x = Directory.GetFiles(manualRoot, "*.html", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (x != null)
                {
                    Process.Start(x);
                    return;
                }
            }
            catch (Exception e) { ex = e; }

            MessageBox.Show(Localizable.WarningManual + (ex != null ? "\n\n" + ex.Message : ""), Localizable.TypeWarn);
        }

        public static void OpenDirectory(string subDirectory)
        {
            try
            {
                var fullPath = Path.Combine(GameRootDirectory, subDirectory ?? "");
                fullPath = Path.GetFullPath(fullPath);
                if (!Directory.Exists(fullPath)) throw new DirectoryNotFoundException("Could not find directory");
                Process.Start("explorer.exe", fullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Localizable.WarningFolder + subDirectory + "\n\n" + ex.Message, Localizable.TypeWarn);
            }
        }

        public static bool StartUpdate()
        {
            try
            {
                var gameRoot = Path.GetFullPath(GameRootDirectory).TrimEnd('\\', '/', ' ');
                var updaterPath = Path.Combine(_kkmanagerDirectory, "StandaloneUpdater.exe");

                if (!File.Exists(updaterPath))
                    throw new FileNotFoundException("Coult not find the updater", updaterPath);

                var args = $"\"{gameRoot}\" {_updateSourcesOverride}";

                return StartProcess(new ProcessStartInfo(updaterPath) { WorkingDirectory = $@"{_kkmanagerDirectory}", Arguments = args }) != null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start the updater: " + ex, Localizable.TypeWarn, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool StartManager()
        {
            try
            {
                var gameRoot = Path.GetFullPath(GameRootDirectory).TrimEnd('\\', '/', ' ');
                var updaterPath = Path.Combine(_kkmanagerDirectory, "KKManager.exe");

                if (!File.Exists(updaterPath))
                    throw new FileNotFoundException("Could not find KKManager", updaterPath);

                var args = $"\"{gameRoot}\" {_updateSourcesOverride}";

                return StartProcess(new ProcessStartInfo(updaterPath) { WorkingDirectory = _kkmanagerDirectory, Arguments = args }) != null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start the KKManager: " + ex, Localizable.TypeWarn, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool StartGame(string gameExeRelativePath)
        {
            var exePath = Path.GetFullPath(GameRootDirectory + gameExeRelativePath);
            if (IsIpa)
            {
                if (File.Exists(exePath))
                {
                    var ipa = "\u0022" + GameRootDirectory + "IPA.exe" + "\u0022";
                    var ipaArgs = "\u0022" + exePath + "\u0022" + " --launch";
                    return StartProcess(new ProcessStartInfo(ipa) { WorkingDirectory = GameRootDirectory, Arguments = ipaArgs }) != null;
                }
            }
            else if (File.Exists(exePath))
            {
                return StartProcess(new ProcessStartInfo(exePath) { WorkingDirectory = GameRootDirectory }) != null;
            }
            MessageBox.Show("Executable can't be located", Localizable.TypeWarn);
            return false;
        }

        //todo readd?
        //private void SystemInfo_Open(object sender, RoutedEventArgs e)
        //{
        //    var text = Environment.ExpandEnvironmentVariables("%windir%") + "/System32/dxdiag.exe";
        //    if (File.Exists(text))
        //    {
        //        Process.Start(text);
        //        return;
        //    }
        //    MessageBox.Show("Folder could not be found, please launch the game at least once.", Localizable.TypeWarn);
        //}
        public static Process StartProcess(string execString)
        {
            try
            {
                return Process.Start(execString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start the command: {execString}\n\nError: {ex}");
                return null;
            }
        }

        public static Process StartProcess(ProcessStartInfo startInfo)
        {
            try
            {
                return Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start the command: {startInfo.FileName + " " + startInfo.Arguments}\n\nError: {ex}");
                return null;
            }
        }
    }
}
