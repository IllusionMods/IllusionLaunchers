using System;

// Token: 0x02000003 RID: 3
[Flags]
public enum DisplayDeviceStateFlags
{
	// Token: 0x04000008 RID: 8
	None = 0,
	// Token: 0x04000009 RID: 9
	AttachedToDesktop = 1,
	// Token: 0x0400000A RID: 10
	MultiDriver = 2,
	// Token: 0x0400000B RID: 11
	PrimaryDevice = 4,
	// Token: 0x0400000C RID: 12
	MirroringDriver = 8,
	// Token: 0x0400000D RID: 13
	VGACompatible = 22,
	// Token: 0x0400000E RID: 14
	Removable = 32,
	// Token: 0x0400000F RID: 15
	ModesPruned = 134217728,
	// Token: 0x04000010 RID: 16
	Remote = 67108864,
	// Token: 0x04000011 RID: 17
	Disconnect = 33554432
}
