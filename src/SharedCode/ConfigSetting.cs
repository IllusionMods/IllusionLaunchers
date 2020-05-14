using System;
using System.Xml.Serialization;

namespace InitSetting
{
	[XmlRoot("Setting")]
	public class ConfigSetting
	{
        [XmlElement("Size")]
		public string Size { get; set; }

		[XmlElement("Width")]
		public int Width { get; set; }

		[XmlElement("Height")]
		public int Height { get; set; }

		[XmlElement("Quality")]
		public int Quality { get; set; }

		[XmlElement("FullScreen")]
		public bool FullScreen { get; set; }

		[XmlElement("Display")]
		public int Display { get; set; }

		[XmlElement("Language")]
		public int Language { get; set; }
	}
}
