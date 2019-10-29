using System;

// Token: 0x02000005 RID: 5
[Flags]
internal enum DevmodeFields
{
	// Token: 0x04000033 RID: 51
	Orientation = 1,
	// Token: 0x04000034 RID: 52
	PaperSize = 2,
	// Token: 0x04000035 RID: 53
	PaperLength = 4,
	// Token: 0x04000036 RID: 54
	PaperWidth = 8,
	// Token: 0x04000037 RID: 55
	Scale = 16,
	// Token: 0x04000038 RID: 56
	Position = 32,
	// Token: 0x04000039 RID: 57
	NUP = 64,
	// Token: 0x0400003A RID: 58
	DisplayOrientation = 128,
	// Token: 0x0400003B RID: 59
	Copies = 256,
	// Token: 0x0400003C RID: 60
	DefaultSource = 512,
	// Token: 0x0400003D RID: 61
	PrintQuality = 1024,
	// Token: 0x0400003E RID: 62
	Color = 2048,
	// Token: 0x0400003F RID: 63
	Duplex = 4096,
	// Token: 0x04000040 RID: 64
	YResolution = 8192,
	// Token: 0x04000041 RID: 65
	TTOption = 16384,
	// Token: 0x04000042 RID: 66
	Collate = 32768,
	// Token: 0x04000043 RID: 67
	FormName = 65536,
	// Token: 0x04000044 RID: 68
	LogPixels = 131072,
	// Token: 0x04000045 RID: 69
	BitsPerPixel = 262144,
	// Token: 0x04000046 RID: 70
	PelsWidth = 524288,
	// Token: 0x04000047 RID: 71
	PelsHeight = 1048576,
	// Token: 0x04000048 RID: 72
	DisplayFlags = 2097152,
	// Token: 0x04000049 RID: 73
	DisplayFrequency = 4194304,
	// Token: 0x0400004A RID: 74
	ICMMethod = 8388608,
	// Token: 0x0400004B RID: 75
	ICMIntent = 16777216,
	// Token: 0x0400004C RID: 76
	MediaType = 33554432,
	// Token: 0x0400004D RID: 77
	DitherType = 67108864,
	// Token: 0x0400004E RID: 78
	PanningWidth = 134217728,
	// Token: 0x0400004F RID: 79
	PanningHeight = 268435456,
	// Token: 0x04000050 RID: 80
	DisplayFixedOutput = 536870912
}
