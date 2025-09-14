using System.Text;

namespace Locate64.Engine.Data.Archive32File;

public class Archive32Reader : IDisposable
{
	private readonly Stream _inputStream;
	private readonly bool _leaveOpen;
	private readonly long _initialPosition;
	private Archive32Header? _header;

	/// <summary>
	/// On first access, the header is read and cached. Subsequent accesses return the cached header.
	///
	/// <exception cref="LegacyLocateFormatException">Thrown if the header could not be read or contains invalid data.</exception>
	/// </summary>
	public Archive32Header Header => _header ??= Archive32Header.ReadFromStream(_inputStream);

	public Archive32Reader(Stream inputStream, bool leaveOpen = false)
	{
		_inputStream = inputStream;
		_leaveOpen = leaveOpen;
		_initialPosition = inputStream.Position;
	}

	public void Reset()
	{
		_inputStream.Position = _initialPosition;
		_header = null;
	}

	private void ReleaseUnmanagedResources()
	{
		// No unmanaged resources to release
	}

	protected virtual void Dispose(bool disposing)
	{
		ReleaseUnmanagedResources();

		if (disposing && !_leaveOpen) _inputStream?.Dispose();
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}


	/// <summary>
	///     Traverse the tree map, returning each individually found entry (root dir, dir or file)
	/// </summary>
	/// <param name="resetAfterwards">true (default) to reset the reader afterwards; false to not do this.</param>
	/// <returns>Enumerable tree map.</returns>
	public IEnumerable<Archive32Entry> Traverse(bool resetAfterwards = true)
	{
		var header = Header;

		// If there are no directories and no files just return 
		if (header.NumberOfDirectories == 0 && header.NumberOfFiles == 0) {
			yield break;
		}

		var reader = new BinaryReader(_inputStream, Encoding.Unicode, true);

		var positionBeforeRead = _inputStream.Position;
		var currentRootDirectory = Archive32RootDirectoryEntry.ReadFrom(reader);
		var readLength = _inputStream.Position - positionBeforeRead;
		var depth = 0;

		var dirStack = new Stack<Archive32DirectoryEntry>();

		while (currentRootDirectory != null)
		{
			yield return currentRootDirectory;

			var type = reader.ReadByte();

			while (type != 0 || depth > 0)
			{

				if ((type & 0x80) == 0x80)
				{
					var dirEntry = Archive32DirectoryEntry.ReadFrom(type, reader, dirStack.Count == 0 ? currentRootDirectory : dirStack.Peek());

					dirStack.Push(dirEntry);
					depth++;
					yield return dirEntry;
				}
				else if (type != 0)
				{
					yield return Archive32FileEntry.ReadFrom(type, reader, dirStack.Count == 0 ? currentRootDirectory : dirStack.Peek());
				}
				else
				{
					depth--;
					dirStack.Pop();
				}

				type = reader.ReadByte();
			}

			var ignored = reader.ReadByte();
			positionBeforeRead = _inputStream.Position;
			currentRootDirectory = Archive32RootDirectoryEntry.ReadFrom(reader);
			readLength = _inputStream.Position - positionBeforeRead;
		}
	}

	~Archive32Reader()
	{
		Dispose(false);
	}
}