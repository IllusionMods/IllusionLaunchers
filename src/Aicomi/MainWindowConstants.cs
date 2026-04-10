namespace InitSetting
{
    public partial class MainWindow
    {
        // Game-specific constants -------------------------------------------------------------------
        private const string RegistryKeyGame = "Software\\ILLGAMES\\Aicomi";
        private const string RegistryKeyStudio = "Software\\ILLGAMES\\DigitalCraft";
        private const string ExecutableGame = "Aicomi.exe";
        private const string ExecutableVR = "AicomiVR\\AicomiVR.exe";

        private const string ManualUrlGame = "https://download.illgames.jp/product/Aicomi_nt/manual/jp.php";
        private const string ManualUrlStudio = "https://download.illgames.jp/product/digitalcraft/manual/jp.php";
        private const string ManualUrlVr = "https://download.illgames.jp/product/Aicomi/manual_vr/";

        private const string SupportDiscord = "https://discord.gg/hevygx6";
        // Languages built into the game itself
        private static readonly string[] _builtinLanguages = { "ja-JP" };
    }
}
