namespace Locate64.Engine.Data.Archive32File
{
	public class Archive32Entry
	{
		public string Name { get; internal set; } = "";

		public Archive32Entry? Parent { get; internal set; }

		public virtual string FullName => Name;
	}
}
