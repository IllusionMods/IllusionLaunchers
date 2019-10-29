using System;
using System.Xml.Serialization;

namespace InitDialog
{
	[XmlRoot("Setting")]
	public class ConfigSetting
	{
        internal bool m_nFullscreenChoose;

        [XmlElement("Size")]
		public string m_strSizeChoose { get; set; }

		[XmlElement("Width")]
		public int m_nWidthChoose { get; set; }

		[XmlElement("Height")]
		public int m_nHeightChoose { get; set; }

		[XmlElement("Quality")]
		public int m_nQualityChoose { get; set; }

		[XmlElement("FullScreen")]
		public bool m_bFullScreen { get; set; }

		[XmlElement("Display")]
		public int m_nDisplay { get; set; }

		[XmlElement("Language")]
		public int m_nLangChoose { get; set; }
	}
}
