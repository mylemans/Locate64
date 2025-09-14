using System.Text.Json;

namespace Locate64.Engine.Search
{
	public class SearchConfig
	{
		public List<string> Databases { get; set; } = new();
		public bool CaseInsensitive { get; set; } = true; // default insensitive
		public bool UseRegex { get; set; }
		public MatchMode MatchMode { get; set; } = MatchMode.Fullpath;
		public HashSet<string> Extensions { get; set; } = new();
		public bool InvertMatch { get; set; }
		public bool MatchExact { get; set; }
		public SearchTarget Target { get; set; } = SearchTarget.FilesAndFolders;

		public static SearchConfig Load(string path)
		{
			var json = File.ReadAllText(path);
			return JsonSerializer.Deserialize<SearchConfig>(json)
				?? new SearchConfig();
		}
	}
}
