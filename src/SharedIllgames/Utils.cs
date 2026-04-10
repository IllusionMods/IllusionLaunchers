using System.IO;
using Microsoft.Win32;

namespace InitSetting
{
    public class Utils
    {
        public static string FindDigitalCraftPath()
        {
            const string RegistryKeyHoneyCome = @"Software\ILLGAMES\HoneyCome";
            const string RegistryKeyStudio = @"Software\ILLGAMES\DigitalCraft";
            const string StudioRelativePath = @"\DigitalCraft\DigitalCraft.exe";

            var installed = EnvironmentHelper.GameRootDirectory + StudioRelativePath;
            if (File.Exists(installed)) return installed;

            var standalone = Registry.CurrentUser.OpenSubKey(RegistryKeyStudio)?.GetValue("INSTALLDIR") + StudioRelativePath;
            if (File.Exists(standalone)) return standalone;

            var dolce = Registry.CurrentUser.OpenSubKey(RegistryKeyHoneyCome)?.GetValue("INSTALLDIR") + StudioRelativePath;
            if (File.Exists(dolce)) return dolce;

            return null;
        }
    }
}
