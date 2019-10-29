using System;
using System.Xml.Serialization;

namespace InitDialog
{
	// Token: 0x02000009 RID: 9
	[XmlRoot("Setting")]
	public class ConfigSetting
	{
        internal bool m_nFullscreenChoose;

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000006 RID: 6 RVA: 0x0000208E File Offset: 0x0000028E
        // (set) Token: 0x06000007 RID: 7 RVA: 0x00002096 File Offset: 0x00000296
        [XmlElement("Size")]
		public string m_strSizeChoose { get; set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000008 RID: 8 RVA: 0x0000209F File Offset: 0x0000029F
		// (set) Token: 0x06000009 RID: 9 RVA: 0x000020A7 File Offset: 0x000002A7
		[XmlElement("Width")]
		public int m_nWidthChoose { get; set; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600000A RID: 10 RVA: 0x000020B0 File Offset: 0x000002B0
		// (set) Token: 0x0600000B RID: 11 RVA: 0x000020B8 File Offset: 0x000002B8
		[XmlElement("Height")]
		public int m_nHeightChoose { get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600000C RID: 12 RVA: 0x000020C1 File Offset: 0x000002C1
		// (set) Token: 0x0600000D RID: 13 RVA: 0x000020C9 File Offset: 0x000002C9
		[XmlElement("Quality")]
		public int m_nQualityChoose { get; set; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600000E RID: 14 RVA: 0x000020D2 File Offset: 0x000002D2
		// (set) Token: 0x0600000F RID: 15 RVA: 0x000020DA File Offset: 0x000002DA
		[XmlElement("FullScreen")]
		public bool m_bFullScreen { get; set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000010 RID: 16 RVA: 0x000020E3 File Offset: 0x000002E3
		// (set) Token: 0x06000011 RID: 17 RVA: 0x000020EB File Offset: 0x000002EB
		[XmlElement("Display")]
		public int m_nDisplay { get; set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000012 RID: 18 RVA: 0x000020F4 File Offset: 0x000002F4
		// (set) Token: 0x06000013 RID: 19 RVA: 0x000020FC File Offset: 0x000002FC
		[XmlElement("Language")]
		public int m_nLangChoose { get; set; }
	}
}
