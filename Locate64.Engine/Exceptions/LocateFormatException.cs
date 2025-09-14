namespace Locate64.Engine.Exceptions
{
	[Serializable]
	public class LocateFormatException : LocateException
	{
		public LocateFormatException()
		{
		}

		public LocateFormatException(string? message) : base(message)
		{
		}

		public LocateFormatException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
