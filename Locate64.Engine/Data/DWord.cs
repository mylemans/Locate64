using System.Runtime.InteropServices;

namespace Locate64.Engine.Data
{
	[StructLayout(LayoutKind.Explicit)]
	public struct DWord
	{
		[FieldOffset(0)]
		public uint Value;

		[FieldOffset(0)]
		public ushort Low;

		[FieldOffset(2)]
		public ushort High;

		public override readonly string ToString()
		{
			return $"{Value}";
		}

		public static implicit operator DWord(uint value) => new() { Value = value };
		public static implicit operator uint(DWord value) => value.Value;
	}
}
