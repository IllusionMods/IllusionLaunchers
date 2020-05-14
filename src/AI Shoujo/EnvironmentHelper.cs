using System;
using System.Diagnostics;
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
        private static string m_strSaveDir = "/UserData/setup.xml";
        private static string m_customDir = "/UserData/LauncherEN";
        private static string decideLang = "/lang";
        private static string versioningLoc = "/version";
        private static string warningLoc = "/warning.txt";
        private static string charLoc = "/Chara.png";
        private static string backgLoc = "/LauncherBG.png";
        private static string patreonLoc = "/patreon.txt";
        private static string kkmdir = "/kkman.txt";
        private static string updateLoc = "/updater.txt";
        private static bool is64bitOS;
        public static string lang = "en";
        private static bool WarningExists;
        private static bool CharExists;
        private static bool BackgExists;
        private static bool PatreonExists;
        private static bool LangExists;
        public static bool kkmanExist;
        public static bool updatelocExists;
        public static bool isIPA;
        private static bool isBepIn;
        public static string kkman;
        public static string updateSources = "placeholder";
        public static string patreonURL;
        private static Mutex mutex;
        public static string GameRootDirectory { get; private set; }
        public static string VersionString;
        public static string WarningString;


        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] IntPtr hProcess, out bool lpSystemInfo);

        private static bool DoubleStartCheck()
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

        public static bool ReleaseMutex()
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

        public static bool IsWow64()
        {
            bool flag;
            return EnvironmentHelper.GetProcAddress(EnvironmentHelper.GetModuleHandle("Kernel32.dll"), "IsWow64Process") != IntPtr.Zero && EnvironmentHelper.IsWow64Process(Process.GetCurrentProcess().Handle, out flag) && flag;
        }

        public static bool Is64BitOS()
        {
            if (IntPtr.Size == 4)
            {
                return IsWow64();
            }
            return IntPtr.Size == 8;
        }

        public static void SetLanguage(string langstring, bool alsoXua)
        {
            if (alsoXua)
            {
                WriteLangIni(langstring);
                //deactivateTL(1);
            }

            if (File.Exists(GameRootDirectory + m_customDir + decideLang))
            {
                File.Delete(GameRootDirectory + m_customDir + decideLang);
            }
            using (StreamWriter writetext = new StreamWriter(GameRootDirectory + m_customDir + decideLang))
            {
                writetext.WriteLine(langstring);
            }
            Application.Restart();
        }


        static void WriteLangIni(string language)
        {
            if (File.Exists(EnvironmentHelper.GameRootDirectory + "BepInEx/Config/AutoTranslatorConfig.ini"))
            {
                if (System.Windows.MessageBox.Show("Do you want the ingame language to reflect this language choice?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    helvete(language);
                }
                // Borrowed from Marco
            }
            //MessageBox.Show($"{curAutoTLOut}", "Debug");
        }
        static void helvete(string language)
        {
            if (File.Exists("BepInEx/Config/AutoTranslatorConfig.ini"))
            {
                var ud = Path.Combine(EnvironmentHelper.GameRootDirectory, @"BepInEx/Config/AutoTranslatorConfig.ini");

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

        //todo doesn't work, remove?
        //void deactivateTL(int i)
        //{
        //    var s_EnglishTL = new string[]
        //    {
        //        "BepInEx/XUnity.AutoTranslator.Plugin.BepIn",
        //        "BepInEx/XUnity.AutoTranslator.Plugin.Core",
        //        "BepInEx/XUnity.AutoTranslator.Plugin.ExtProtocol",
        //        "BepInEx/XUnity.RuntimeHooker.Core",
        //        "BepInEx/XUnity.RuntimeHooker",
        //        "BepInEx/KK_Subtitles",
        //        "BepInEx/ExIni"
        //    };
        //
        //    if (i == 0)
        //    {
        //        foreach (var file in s_EnglishTL)
        //        {
        //            if (File.Exists(EnvironmentHelper.GameRootDirectory + file + ".dll"))
        //            {
        //                File.Move(EnvironmentHelper.GameRootDirectory + file + ".dll", EnvironmentHelper.GameRootDirectory + file + "._ll");
        //            }
        //        }
        //    }
        //    else
        //    {
        //        foreach (var file in s_EnglishTL)
        //        {
        //            if (File.Exists(EnvironmentHelper.GameRootDirectory + file + "._ll"))
        //            {
        //                File.Move(EnvironmentHelper.GameRootDirectory + file + "._ll", EnvironmentHelper.GameRootDirectory + file + ".dll");
        //            }
        //            helvete("en");
        //        }
        //    }
        //}

        public static void CheckDuplicateStartup()
        {
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
        }

        public static bool? DeveloperModeEnabled
        {
            get
            {
                if (File.Exists(GameRootDirectory + "/Bepinex/config/BepInEx.cfg"))
                {
                    var ud = Path.Combine(GameRootDirectory, @"BepInEx\config\BepInEx.cfg");

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
                                return true;
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
                    return null;
                }

                return false;
            }
            set
            {
                if (value == null) return;

                var ud = Path.Combine(GameRootDirectory, @"BepInEx\config\BepInEx.cfg");

                try
                {
                    var contents = File.ReadAllLines(ud).ToList();

                    var setToLanguage = contents.FindIndex(s => s.ToLower().Contains("[Logging.Console]".ToLower()));
                    if (setToLanguage >= 0 && value.Value)
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
        }

        public static void Initialize()
        {
            var currentDirectory = Path.GetDirectoryName(typeof(MainWindow).Assembly.Location) ?? Environment.CurrentDirectory;
            GameRootDirectory = currentDirectory + "\\";

            Directory.CreateDirectory(GameRootDirectory + m_customDir);

            BepinPluginsDir = $"{GameRootDirectory}\\BepInEx\\Plugins\\";

            //if (!File.Exists(GameRootDirectory + m_customDir + kkmdir))
            //{
            //
            //}

            // Framework test
            isIPA = File.Exists($"{GameRootDirectory}\\IPA.exe");
            isBepIn = Directory.Exists($"{GameRootDirectory}\\BepInEx");

            if (isIPA && isBepIn)
                MessageBox.Show("Both BepInEx and IPA is detected in the game folder!\n\nApplying both frameworks may lead to problems when running the game!", "Warning!");


            // Updater stuffs

            kkmanExist = File.Exists(GameRootDirectory + m_customDir + kkmdir);
            updatelocExists = File.Exists(GameRootDirectory + m_customDir + updateLoc);
            if (kkmanExist)
            {
                var kkmanFileStream = new FileStream(GameRootDirectory + m_customDir + kkmdir, FileMode.Open, FileAccess.Read);
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
                    var updFileStream = new FileStream(GameRootDirectory + m_customDir + updateLoc, FileMode.Open, FileAccess.Read);
                    using (var streamReader = new StreamReader(updFileStream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            updateSources = line;
                        }
                    }
                    updFileStream.Close();
                }
                else
                {
                    updateSources = "";
                }
            }

            //if (!File.Exists(GameRootDirectory + m_customDir + kkmdir))
            //{
            //
            //}

            LangExists = File.Exists(GameRootDirectory + m_customDir + decideLang);
            if (LangExists)
            {
                var verFileStream = new FileStream(GameRootDirectory + m_customDir + decideLang, FileMode.Open, FileAccess.Read);
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


            is64bitOS = Is64BitOS();

            if (GameRootDirectory.Length >= 75)
                MessageBox.Show("The game is installed deep in the file system!\n\nThis can cause a variety of errors, so it's recommended that you move it to a shorter path, something like:\n\nC:\\Illusion\\AI.Shoujo", "Critical warning!");


            // Customization options

            CharExists = File.Exists(GameRootDirectory + m_customDir + charLoc);
            BackgExists = File.Exists(GameRootDirectory + m_customDir + backgLoc);
            WarningExists = File.Exists(GameRootDirectory + m_customDir + warningLoc);
            PatreonExists = File.Exists(GameRootDirectory + m_customDir + patreonLoc);

            // Launcher Customization: Grabbing versioning of install method

            var versionAvail = File.Exists(GameRootDirectory + "version");
            if (versionAvail)
            {
                var verFileStream = new FileStream(GameRootDirectory + "version", FileMode.Open, FileAccess.Read);
                using (var streamReader = new StreamReader(verFileStream, Encoding.UTF8))
                {
                    string line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        VersionString = line;
                    }
                }
                verFileStream.Close();
            }

            if (WarningExists)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(GameRootDirectory + m_customDir + warningLoc))
                    {
                        String line = sr.ReadToEnd();
                        WarningString = line;
                    }
                }
                catch (IOException e)
                {
                    WarningString = e.Message;
                }
            }
            if (CharExists)
            {
                Uri urich = new Uri(GameRootDirectory + m_customDir + charLoc, UriKind.RelativeOrAbsolute);
                CustomCharacterImage = BitmapFrame.Create(urich);
            }
            if (BackgExists)
            {
                Uri uribg = new Uri(GameRootDirectory + m_customDir + backgLoc, UriKind.RelativeOrAbsolute);
                CustomBgImage = BitmapFrame.Create(uribg);
            }
            if (PatreonExists)
            {
                var verFileStream = new FileStream(GameRootDirectory + m_customDir + patreonLoc, FileMode.Open, FileAccess.Read);
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
        }

        public static BitmapFrame CustomBgImage { get; set; }

        public static BitmapFrame CustomCharacterImage { get; set; }

        public static string GetConfigFilePath()
        {
            return GameRootDirectory + m_strSaveDir;
        }

        public static string BepinPluginsDir;

        public static void DisablePlugin(string enabledPath)
        {
            var disabledPath = enabledPath.Substring(0, enabledPath.LastIndexOf(".dll", StringComparison.OrdinalIgnoreCase)) + ".dl_";
            if (File.Exists(enabledPath))
            {
                if (File.Exists(disabledPath)) File.Delete(disabledPath);
                File.Move(enabledPath, disabledPath);
            }
        }

        public static void EnablePlugin(string enabledPath)
        {
            var disabledPath = enabledPath.Substring(0, enabledPath.LastIndexOf(".dll", StringComparison.OrdinalIgnoreCase)) + ".dl_";
            if (File.Exists(disabledPath))
            {
                if (File.Exists(enabledPath)) File.Delete(enabledPath);
                File.Move(disabledPath, enabledPath);
            }
        }
    }
}