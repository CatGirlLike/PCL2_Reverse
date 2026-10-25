using System;

namespace PCLCS;

[Flags]
public enum ResourceTypes
{
	Mod = 1,
	ModPack = 2,
	ResourcePack = 4,
	Shader = 8,
	DataPack = 0x10,
	Plugin = 0x20,
	Map = 0x40,
	ModOrDataPack = 0x11,
	Any = 0x7F
}
