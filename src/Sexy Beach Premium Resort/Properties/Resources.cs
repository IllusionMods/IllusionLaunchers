using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace InitDialog.Properties
{
	// Token: 0x0200000C RID: 12
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		// Token: 0x06000045 RID: 69 RVA: 0x00002086 File Offset: 0x00000286
		internal Resources()
		{
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000046 RID: 70 RVA: 0x00003ADC File Offset: 0x00001CDC
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					Resources.resourceMan = new ResourceManager("InitDialog.Properties.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000047 RID: 71 RVA: 0x00003B08 File Offset: 0x00001D08
		// (set) Token: 0x06000048 RID: 72 RVA: 0x00003B0F File Offset: 0x00001D0F
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x0400008C RID: 140
		private static ResourceManager resourceMan;

		// Token: 0x0400008D RID: 141
		private static CultureInfo resourceCulture;
	}
}
