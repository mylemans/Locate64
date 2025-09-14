using Locate64.Engine.Data;
using Locate64.Engine.IO;

namespace Locate64.Engine.Data.Archive32File
{
	public class Archive32RootDirectoryEntry : Archive32Entry
	{
		public DWord DataLength { get; internal set; }

		public Archive32RootType RootType { get; internal set; }

		public string VolumeName { get; internal set; } = "";

		public DWord VolumeSerial { get; internal set; }

		public string FileSystem { get; internal set; } = "";

		public DWord NumberOfFiles { get; internal set; }

		public DWord NumberOfDirectories { get; internal set; }


		public static Archive32RootDirectoryEntry? ReadFrom(BinaryReader reader)
		{
			var dataLength = reader.ReadUInt32();

			if (dataLength == 0)
			{
				return null;
			}

			var entry = new Archive32RootDirectoryEntry
			{
				DataLength = dataLength,
				RootType = (Archive32RootType)reader.ReadByte(),
				Name = reader.ReadNullTerminatedUtf16String(),
				VolumeName = reader.ReadNullTerminatedUtf16String(),
				VolumeSerial = reader.ReadUInt32(),
				FileSystem = reader.ReadNullTerminatedUtf16String(),
				NumberOfFiles = reader.ReadUInt32(),
				NumberOfDirectories = reader.ReadUInt32()
			};

			return entry;
		}

		public override string ToString()
		{
			return $"{nameof(DataLength)}: {DataLength}, {nameof(RootType)}: {RootType}, {nameof(Name)}: {Name}, {nameof(VolumeName)}: {VolumeName}, {nameof(VolumeSerial)}: {VolumeSerial}, {nameof(FileSystem)}: {FileSystem}, {nameof(NumberOfFiles)}: {NumberOfFiles}, {nameof(NumberOfDirectories)}: {NumberOfDirectories}";
		}
	}
}
