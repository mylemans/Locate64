namespace Locate64.Engine.Exceptions
{
	[Serializable]
	public class LocateException : Exception
	{
		public LocateException()
		{
		}

		public LocateException(string? message) : base(message)
		{
		}

		public LocateException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
