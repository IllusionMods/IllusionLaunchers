using System;
using System.Runtime.InteropServices;

// Token: 0x02000002 RID: 2
public struct DISPLAY_DEVICE
{
	// Token: 0x04000001 RID: 1
	[MarshalAs(UnmanagedType.U4)]
	public int cb;

	// Token: 0x04000002 RID: 2
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
	public string DeviceName;

	// Token: 0x04000003 RID: 3
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string DeviceString;

	// Token: 0x04000004 RID: 4
	[MarshalAs(UnmanagedType.U4)]
	public DisplayDeviceStateFlags StateFlags;

	// Token: 0x04000005 RID: 5
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string DeviceID;

	// Token: 0x04000006 RID: 6
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string DeviceKey;
}
