namespace InitSetting
{
    public interface IConfigSetting
    {
        string Size { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        int Quality { get; set; }
        bool FullScreen { get; set; }
        int Display { get; set; }
        int Language { get; set; }
    }
}
