using System;
using System.Xml.Serialization;

namespace InitSetting
{
    public class Preferences
    {
        public Graphic Graphic { get; set; }
    }

    public class Graphic
    {
        public string ScrSize { get; set; }
        public int ScrWidth { get; set; }
        public int ScrHeight { get; set; }
        public bool FullScreen { get; set; }
        public int TargetDisplay { get; set; }
        public bool Bloom { get; set; }
        public bool DepthOfField { get; set; }
        public bool Vignette { get; set; }
        public bool SSAO { get; set; }
        public bool Fog { get; set; }
        public int Quality { get; set; }
        public bool Map { get; set; }
        public bool Shield { get; set; }
        public bool AmbientLight { get; set; }
        public string BackColor { get; set; }
    }
}
