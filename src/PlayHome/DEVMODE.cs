using System;
using System.Runtime.InteropServices;

// Token: 0x02000004 RID: 4
[StructLayout(LayoutKind.Explicit)]
internal struct DEVMODE
{
	// Token: 0x04000012 RID: 18
	public const int CCHDEVICENAME = 32;

	// Token: 0x04000013 RID: 19
	public const int CCHFORMNAME = 32;

	// Token: 0x04000014 RID: 20
	[FieldOffset(0)]
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
	public string dmDeviceName;

	// Token: 0x04000015 RID: 21
	[FieldOffset(32)]
	public short dmSpecVersion;

	// Token: 0x04000016 RID: 22
	[FieldOffset(34)]
	public short dmDriverVersion;

	// Token: 0x04000017 RID: 23
	[FieldOffset(36)]
	public short dmSize;

	// Token: 0x04000018 RID: 24
	[FieldOffset(38)]
	public short dmDriverExtra;

	// Token: 0x04000019 RID: 25
	[FieldOffset(40)]
	public DevmodeFields dmFields;

	// Token: 0x0400001A RID: 26
	[FieldOffset(44)]
	private short dmOrientation;

	// Token: 0x0400001B RID: 27
	[FieldOffset(46)]
	private short dmPaperSize;

	// Token: 0x0400001C RID: 28
	[FieldOffset(48)]
	private short dmPaperLength;

	// Token: 0x0400001D RID: 29
	[FieldOffset(50)]
	private short dmPaperWidth;

	// Token: 0x0400001E RID: 30
	[FieldOffset(52)]
	private short dmScale;

	// Token: 0x0400001F RID: 31
	[FieldOffset(54)]
	private short dmCopies;

	// Token: 0x04000020 RID: 32
	[FieldOffset(56)]
	private short dmDefaultSource;

	// Token: 0x04000021 RID: 33
	[FieldOffset(58)]
	private short dmPrintQuality;

	// Token: 0x04000022 RID: 34
	[FieldOffset(44)]
	public POINTL dmPosition;

	// Token: 0x04000023 RID: 35
	[FieldOffset(52)]
	public int dmDisplayOrientation;

	// Token: 0x04000024 RID: 36
	[FieldOffset(56)]
	public int dmDisplayFixedOutput;

	// Token: 0x04000025 RID: 37
	[FieldOffset(60)]
	public short dmColor;

	// Token: 0x04000026 RID: 38
	[FieldOffset(62)]
	public short dmDuplex;

	// Token: 0x04000027 RID: 39
	[FieldOffset(64)]
	public short dmYResolution;

	// Token: 0x04000028 RID: 40
	[FieldOffset(66)]
	public short dmTTOption;

	// Token: 0x04000029 RID: 41
	[FieldOffset(68)]
	public short dmCollate;

	// Token: 0x0400002A RID: 42
	[FieldOffset(72)]
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
	public string dmFormName;

	// Token: 0x0400002B RID: 43
	[FieldOffset(102)]
	public short dmLogPixels;

	// Token: 0x0400002C RID: 44
	[FieldOffset(104)]
	public int dmBitsPerPel;

	// Token: 0x0400002D RID: 45
	[FieldOffset(108)]
	public int dmPelsWidth;

	// Token: 0x0400002E RID: 46
	[FieldOffset(112)]
	public int dmPelsHeight;

	// Token: 0x0400002F RID: 47
	[FieldOffset(116)]
	public int dmDisplayFlags;

	// Token: 0x04000030 RID: 48
	[FieldOffset(116)]
	public int dmNup;

	// Token: 0x04000031 RID: 49
	[FieldOffset(120)]
	public int dmDisplayFrequency;
}
