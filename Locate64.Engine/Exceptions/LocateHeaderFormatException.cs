namespace Locate64.Engine.Exceptions
{
	[Serializable]
	public class LocateHeaderFormatException : LocateFormatException
	{
		public LocateHeaderFormatException()
		{
		}

		public LocateHeaderFormatException(string? message) : base(message)
		{
		}

		public LocateHeaderFormatException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
