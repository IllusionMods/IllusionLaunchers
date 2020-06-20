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
using System.Windows.Media.Imaging;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;

namespace InitSetting
{
    public static class EnvironmentHelper
    {
        private static readonly string _mStrSaveDir = "/UserData/setup.xml";
        private static readonly string _mCustomDir = "/UserData/LauncherEN";
        private static readonly string _decideLang = "/lang";
        private static readonly string _versioningLoc = "/version";
        private static readonly string _warningLoc = "/warning.txt";
        private static readonly string _charLoc = "/Chara.png";
        private static readonly string _backgLoc = "/LauncherBG.png";
        private static readonly string _patreonLoc = "/patreon.txt";
        private static readonly string _kkmdir = "/kkman.txt";
        private static readonly string _updateLoc = "/updater.txt";
        private static bool _langExists;
        private static bool _updatelocExists;
        private static string _kkman;
        private static string _updateSources = "placeholder";
        private static string[] _builtinLanguages;

        public static CultureInfo Language { get; private set; }

        public static bool KKmanExist { get; private set; }
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
                    MessageBox.Show("Something went wrong: " + e);
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
                    MessageBox.Show("Something went wrong: " + e);
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
                        LangQ_b = "Questão";
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
                    default:
                        LangQ_a = "Do you want to set the ingame language to the selected language as well?";
                        LangQ_b = "Question";
                        break;
                }

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

                    //if (!Directory.Exists($"{GameRootDirectory}\\BepInEx\\Translation\\{language}") && !_builtinLanguages.Contains(language))
                    //{
                    //    MessageBox.Show(Localizable.InstructDecideLang, "Warning!");
                    //}

                    MessageBox.Show(Localizable.InstructDecideLang);

                    WriteAutoTranslatorLangIni(language);
                }

                Application.Restart();
            }
            catch (Exception e)
            {
                MessageBox.Show("Something went wrong when changing language: " + e);
            }
        }

        private static void WriteAutoTranslatorLangIni(string language)
        {
            var configPath = Path.Combine(GameRootDirectory, @"BepInEx/Config/AutoTranslatorConfig.ini");

            var disable = language.Equals("jp-JP", StringComparison.OrdinalIgnoreCase);

            language = language.Split('-')[0];

            try
            {
                var contents = (File.Exists(configPath) ? File.ReadAllLines(configPath) : Enumerable.Empty<string>()).ToList();

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

                {
                    var categoryIndex = contents.FindIndex(s => s.ToLower().Contains("[Service]".ToLower()));
                    if (categoryIndex >= 0)
                    {
                        var i = contents.FindIndex(categoryIndex, s => s.StartsWith("Endpoint"));
                        if (i > categoryIndex)
                            contents[i] = disable ? "Endpoint=" : "Endpoint=GoogleTranslate";
                        else
                            contents.Insert(categoryIndex + 1, disable ? "Endpoint=" : "Endpoint=GoogleTranslate");
                    }
                    else
                    {
                        contents.Add("");
                        contents.Add("[Service]");
                        contents.Add(disable ? "Endpoint=" : "Endpoint=GoogleTranslate");
                    }
                }

                Directory.CreateDirectory(Path.GetDirectoryName(configPath));
                File.WriteAllLines(configPath, contents.ToArray());
            }
            catch (Exception e)
            {
                MessageBox.Show("Something went wrong when writing AutoTranslator config file: " + e);
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

            // Framework test
            IsIpa = File.Exists($"{GameRootDirectory}\\IPA.exe");
            IsBepIn = Directory.Exists($"{GameRootDirectory}\\BepInEx");

            if (IsIpa && IsBepIn)
            {
                MessageBox.Show(
                    "Both BepInEx and IPA is detected in the game folder!\n\nApplying both frameworks may lead to problems when running the game. Consider uninstalling IPA and using the BepInEx.IPALoader plugin to run your IPA plugins instead.",
                    "Warning!");
            }


            // Updater stuffs

            KKmanExist = File.Exists(GameRootDirectory + _mCustomDir + _kkmdir);
            _updatelocExists = File.Exists(GameRootDirectory + _mCustomDir + _updateLoc);
            if (KKmanExist)
            {
                var kkmanFileStream = new FileStream(GameRootDirectory + _mCustomDir + _kkmdir, FileMode.Open,
                    FileAccess.Read);
                using (var streamReader = new StreamReader(kkmanFileStream, Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null) _kkman = line;
                }

                kkmanFileStream.Close();
                if (_updatelocExists)
                {
                    var updFileStream = new FileStream(GameRootDirectory + _mCustomDir + _updateLoc, FileMode.Open,
                        FileAccess.Read);
                    using (var streamReader = new StreamReader(updFileStream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null) _updateSources = line;
                    }

                    updFileStream.Close();
                }
                else
                {
                    _updateSources = "";
                }
            }

            if (GameRootDirectory.Length >= 75)
                MessageBox.Show(
                    "The game is installed deep in the file system!\n\nThis can cause a variety of errors, so it's recommended that you move it to a shorter path, something like:\n\nC:\\Illusion\\AI.Shoujo",
                    "Critical warning!");

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

            MessageBox.Show("Manual could not be found." + (ex != null ? "\n\n" + ex.Message : ""), "Warning!");
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
                MessageBox.Show($"Could not open the folder: {subDirectory}\n\n{ex.Message}", "Warning!");
            }
        }

        public static bool StartUpdate()
        {
            var gameRoot = GameRootDirectory.TrimEnd('\\', '/', ' ');
            var kkmanPath = _kkman.TrimEnd('\\', '/', ' ');

            var finaldir = !File.Exists($@"{kkmanPath}\StandaloneUpdater.exe")
                ? $@"{GameRootDirectory}{kkmanPath}"
                : kkmanPath;

            var text = $@"{finaldir}\StandaloneUpdater.exe";

            var argdir = $"\u0022{gameRoot}\u0022";
            var argloc = _updateSources;
            var args = $"{argdir} {argloc}";

            if (!_updatelocExists)
                args = $"{argdir}";

            return StartProcess(new ProcessStartInfo(text) { WorkingDirectory = $@"{finaldir}", Arguments = args }) != null;
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
            MessageBox.Show("Executable can't be located", "Warning!");
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
        //    MessageBox.Show("Folder could not be found, please launch the game at least once.", "Warning!");
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