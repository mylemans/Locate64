using System;
using System.Collections.Generic;
using System.Text;

namespace Locate64.Engine.Data.Archive32File
{
	public enum Archive32RootType : byte
	{
		Unknown=0,
		Fixed=0x10,
		Removable=0x20,
		CdRom=0x30,
		Remote=0x40,
		Ramdisk=0x50,
		Directory=0xF0
	}
}
