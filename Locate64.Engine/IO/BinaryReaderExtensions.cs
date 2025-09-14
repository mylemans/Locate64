using System.Buffers;
using System.Text;

namespace Locate64.Engine.IO
{
	internal static class BinaryReaderExtensions
	{
		public static string ReadNullTerminatedUtf16StringAlt(this BinaryReader reader)
		{
			// Keep Alt version, but make it less inefficient
			var sb = new StringBuilder(16);
			short c;
			while ((c = reader.ReadInt16()) != 0)
			{
				sb.Append((char)c);
			}
			return sb.ToString();
		}

		public static string ReadNullTerminatedUtf16String(this BinaryReader reader, int initialCapacity = 16)
		{
			// Rent a char[] from the pool to avoid growing StringBuilder repeatedly
			char[] buffer = ArrayPool<char>.Shared.Rent(initialCapacity);
			int count = 0;

			try
			{
				short c;
				while ((c = reader.ReadInt16()) != 0)
				{
					if (count == buffer.Length)
					{
						// grow buffer
						char[] newBuffer = ArrayPool<char>.Shared.Rent(buffer.Length * 2);
						buffer.AsSpan(0, count).CopyTo(newBuffer);
						ArrayPool<char>.Shared.Return(buffer);
						buffer = newBuffer;
					}
					buffer[count++] = (char)c;
				}

				// Construct string directly from the buffer
				return new string(buffer, 0, count);
			}
			finally
			{
				ArrayPool<char>.Shared.Return(buffer);
			}
		}
	}
}
