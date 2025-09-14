using Locate64.Engine.Exceptions;
using Locate64.Engine.IO;
using System.Text;

namespace Locate64.Engine.Data.Archive32File
{
	public class Archive32Header
	{
		public const string ArchiveMarker = "LOCATEDB";

		public string Marker { get; internal set; } // 8xBYTE, LOCATEDB (ANSI)

		public string Version { get; internal set; } // 2xBYTE, 20

		public byte Flags { get; internal set; } // BYTE, 01h = long file names, 0h = OEM charset, 10h = ANSI charset, 20h = Unicode charset

		public uint RemainingExtraBytes { get; internal set; }   // DWORD, how many bytes should ignored to skip the rest of header
														// (i.e., the combined size of the next seven fields, in bytes)

		public string Creator { get; internal set; } // null-terminated utf16 or ansi string

		public string Description { get; internal set; } // null-terminated utf16 or ansi string

		public string ExtraInformation1 { get; internal set; } // null-terminated utf16 or ansi string

		public string ExtraInformation2 { get; internal set; } // null-terminated utf16 or ansi string

		public DWord CreationTime { get; internal set; } // DWORD, FILETIME structure

		public DWord NumberOfFiles { get; internal set; } // DWORD

		public DWord NumberOfDirectories { get; internal set; } // DWORD

		internal Archive32Header()
		{
			Marker = string.Empty;
			Version = string.Empty;
			Creator = string.Empty;
			Description = string.Empty;
			ExtraInformation1 = string.Empty;
			ExtraInformation2 = string.Empty;
		}

		public static Archive32Header ReadFromStream(Stream inputStream)
		{
			try
			{
				using var reader = new BinaryReader(inputStream, Encoding.UTF8, true);

				var marker = Encoding.UTF8.GetString(reader.ReadBytes(8));

				if (marker != ArchiveMarker)
					throw new LocateHeaderFormatException("Archive marker not found.");

				var header = new Archive32Header
				{
					Marker = marker,
					Version = Encoding.UTF8.GetString(reader.ReadBytes(2)),
					Flags = reader.ReadByte(),
					RemainingExtraBytes = reader.ReadUInt32(),
					Creator = reader.ReadNullTerminatedUtf16String(),
					Description = reader.ReadNullTerminatedUtf16String(),
					ExtraInformation1 = reader.ReadNullTerminatedUtf16String(),
					ExtraInformation2 = reader.ReadNullTerminatedUtf16String(),
					CreationTime = reader.ReadUInt32(),
					NumberOfFiles = reader.ReadUInt32(),
					NumberOfDirectories = reader.ReadUInt32(),
				};

				return header;
			}
			catch (LocateHeaderFormatException)
			{
				throw;
			}
			catch (Exception innerException)
			{
				throw new LocateFormatException("An exception occurred while trying to read the header.", innerException);
			}
		}

		public override string ToString()
		{
			return $"{nameof(Marker)}: {Marker}, {nameof(Version)}: {Version}, {nameof(Flags)}: {Flags}, {nameof(RemainingExtraBytes)}: {RemainingExtraBytes}, {nameof(Creator)}: {Creator}, {nameof(Description)}: {Description}, {nameof(ExtraInformation1)}: {ExtraInformation1}, {nameof(ExtraInformation2)}: {ExtraInformation2}, {nameof(CreationTime)}: {CreationTime}, {nameof(NumberOfFiles)}: {NumberOfFiles}, {nameof(NumberOfDirectories)}: {NumberOfDirectories}";
		}
	}
}
