using System;

// Token: 0x02000005 RID: 5
[Flags]
internal enum DevmodeFields
{
	Orientation = 1,
	PaperSize = 2,
	PaperLength = 4,
	PaperWidth = 8,
	Scale = 16,
	Position = 32,
	NUP = 64,
	DisplayOrientation = 128,
	Copies = 256,
	DefaultSource = 512,
	PrintQuality = 1024,
	Color = 2048,
	Duplex = 4096,
	YResolution = 8192,
	TTOption = 16384,
	Collate = 32768,
	FormName = 65536,
	LogPixels = 131072,
	BitsPerPixel = 262144,
	PelsWidth = 524288,
	PelsHeight = 1048576,
	DisplayFlags = 2097152,
	DisplayFrequency = 4194304,
	ICMMethod = 8388608,
	ICMIntent = 16777216,
	MediaType = 33554432,
	DitherType = 67108864,
	PanningWidth = 134217728,
	PanningHeight = 268435456,
	DisplayFixedOutput = 536870912
}
