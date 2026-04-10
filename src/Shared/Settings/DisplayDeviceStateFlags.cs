using System;

[Flags]
public enum DisplayDeviceStateFlags
{
	None = 0,
	AttachedToDesktop = 1,
	MultiDriver = 2,
	PrimaryDevice = 4,
	MirroringDriver = 8,
	VGACompatible = 22,
	Removable = 32,
	ModesPruned = 134217728,
	Remote = 67108864,
	Disconnect = 33554432
}
