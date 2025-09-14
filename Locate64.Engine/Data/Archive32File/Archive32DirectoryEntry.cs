using Locate64.Core.Data.Archive32File;
using Locate64.Engine.IO;

namespace Locate64.Engine.Data.Archive32File
{
	public class Archive32DirectoryEntry : Archive32Entry
	{
		public DWord DataLength { get; internal set; }

		public byte Flags { get; internal set; }

		public Archive32Entry? Parent { get; internal set; }

		public string FullName
		{
			get
			{
				var segments = new List<string>
				{
					Name
				};

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
					}

				}

				segments.Reverse();
				return Path.Combine(segments.ToArray());
			}
		}


		public static Archive32DirectoryEntry ReadFrom(byte flags, BinaryReader reader, Archive32Entry? parent)
		{
			var entry = new Archive32DirectoryEntry
			{
				Flags = flags,
				Parent = parent
			};

			var length = reader.ReadUInt32();
			var mysteryByte1 = reader.ReadByte();
			entry.Name = reader.ReadNullTerminatedUtf16String();
			var mysteryBytes2 = reader.ReadBytes(12);

			return entry;
		}


		public override string ToString()
		{
			return string.Join(", ",
				$"{nameof(Name)}: {Name}",
				$"{nameof(FullName)}: {FullName}",
				$"{nameof(DataLength)}: {DataLength}",
				$"{nameof(Flags)}: {Flags} (0x{Flags:X2})"
			);
		}
	}
}
