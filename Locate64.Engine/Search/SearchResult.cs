namespace Locate64.Engine.Search
{
	public readonly struct SearchResult
	{
		public string Database { get; }
		public string Path { get; }

		public SearchResult(string database, string path)
		{
			Database = database;
			Path = path;
		}

		public override string ToString()
		{
			return Path;
		}
	}
}
