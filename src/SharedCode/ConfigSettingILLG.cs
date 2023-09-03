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
        [XmlElement("ScrSize")]
        public string ScrSize { get; set; }

        [XmlElement("ScrWidth")]
        public int ScrWidth { get; set; }

        [XmlElement("ScrHeight")]
        public int ScrHeight { get; set; }

        [XmlElement("FullScreen")]
        public bool FullScreen { get; set; }

        [XmlElement("TargetDisplay")]
        public int TargetDisplay { get; set; }

        [XmlElement("Bloom")]
        public bool Bloom { get; set; }

        [XmlElement("DepthOfField")]
        public bool DepthOfField { get; set; }

        [XmlElement("Vignette")]
        public bool Vignette { get; set; }

        [XmlElement("SSAO")]
        public bool SSAO { get; set; }

        [XmlElement("Fog")]
        public bool Fog { get; set; }

        [XmlElement("Quality")]
        public int Quality { get; set; }

        [XmlElement("Map")]
        public bool Map { get; set; }

        [XmlElement("Shield")]
        public bool Shield { get; set; }

        [XmlElement("AmbientLight")]
        public bool AmbientLight { get; set; }

        [XmlElement("BackColor")]
        public string BackColor { get; set; }
    }
}
