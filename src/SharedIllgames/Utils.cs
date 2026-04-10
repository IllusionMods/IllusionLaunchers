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

            var regValue = Registry.CurrentUser.OpenSubKey(RegistryKeyStudio)?.GetValue("INSTALLDIR")?.ToString();
            if (!string.IsNullOrEmpty(regValue))
            {
                var standalone = regValue + StudioRelativePath;
                if (File.Exists(standalone)) return standalone;
            }

            regValue = Registry.CurrentUser.OpenSubKey(RegistryKeyHoneyCome)?.GetValue("INSTALLDIR")?.ToString();
            if (!string.IsNullOrEmpty(regValue))
            {
                var dolce = regValue + StudioRelativePath;
                if (File.Exists(dolce)) return dolce;
            }

            return null;
        }
    }
}
