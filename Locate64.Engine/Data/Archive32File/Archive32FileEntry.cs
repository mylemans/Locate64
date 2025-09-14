using Locate64.Engine.Exceptions;
using Locate64.Engine.IO;
using Locate64.Engine.Utils;

namespace Locate64.Engine.Data.Archive32File
{
	public class Archive32FileEntry : Archive32Entry
	{
		private string? _fullName;

		public long DataLength { get; internal set; }

		public byte Flags { get; internal set; }

		public Archive32Entry? Parent { get; internal set; }

		public byte ExtensionIndex { get; internal set; } = 0;
		public uint ModifiedDosDateTime { get; internal set; } = 0;
		public uint CreatedDosDateTime { get; internal set; } = 0;
		public uint LastAccessedDosDateTime { get; internal set; } = 0;


		public string ExtensionBasedOnIndex
		{
			get
			{
				return Name.Substring(ExtensionIndex).TrimStart('.');
			}
		}

		public DateTime ModifiedDateTime
		{
			get
			{
				return DosDateTimeDecoder.DecodeDosDateTime(ModifiedDosDateTime, 0);
			}
		}

		public DateTime CreatedDateTime
		{
			get
			{
				return DosDateTimeDecoder.DecodeDosDateTime(CreatedDosDateTime, 0);
			}
		}

		public DateTime LastAccessedDateTime
		{
			get
			{
				return DosDateTimeDecoder.DecodeDosDateTime(LastAccessedDosDateTime, 0);
			}
		}

		public string FullName
		{
			get
			{
				if (_fullName != null)
					return _fullName;

				var segments = new List<string>();
				segments.Add(Name);

				var parent = Parent;

				while (parent != null)
				{

					if (parent is Archive32RootDirectoryEntry rootEntry)
					{
						segments.Add(rootEntry.Name);
						parent = null;
					}
					else if (parent is Archive32DirectoryEntry dirEntry)
					{
						segments.Add(dirEntry.Name);
						parent = dirEntry.Parent;
					} else {
	 					throw new LocateException("File entry has a parent that is neither a directory nor a root directory.");
					}
				}

				segments.Reverse();
				return _fullName = Path.Combine(segments.ToArray());
			}
		}

		public static Archive32FileEntry ReadFrom(byte flags, BinaryReader reader, Archive32Entry? parent)
		{
			var entry = new Archive32FileEntry
			{
				Flags = flags,
				Parent = parent
			};

			var nameLength = reader.ReadByte();
			entry.ExtensionIndex = reader.ReadByte();
			entry.Name = reader.ReadNullTerminatedUtf16String(nameLength);
			uint sizeLo = reader.ReadUInt32();
			ushort sizeHi = reader.ReadUInt16();
			entry.DataLength = (long)sizeHi << 32 | sizeLo;
			entry.ModifiedDosDateTime = reader.ReadUInt32();
			entry.CreatedDosDateTime = reader.ReadUInt32();
			entry.LastAccessedDosDateTime = reader.ReadUInt32();

			return entry;
		}

		public override string ToString()
		{
			return string.Join(", ",
				$"{nameof(Name)}: {Name}",
				$"{nameof(FullName)}: {FullName}",
				$"{nameof(DataLength)}: {DataLength}",
				$"{nameof(ExtensionIndex)}: {ExtensionIndex} ({ExtensionBasedOnIndex})",
				$"{nameof(Flags)}: {Flags} (0x{Flags:X2})",
				$"{nameof(ModifiedDateTime)}: {ModifiedDateTime} ({ModifiedDosDateTime})",
				$"{nameof(CreatedDateTime)}: {CreatedDateTime} ({CreatedDosDateTime})",
				$"{nameof(LastAccessedDateTime)}: {LastAccessedDateTime} ({LastAccessedDosDateTime})"
			);
		}
	}
}
