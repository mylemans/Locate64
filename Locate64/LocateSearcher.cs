using Locate64.Engine.Data.Archive32File;
using System.Text.RegularExpressions;

namespace Locate64
{
	internal class LocateSearcher
	{
		private readonly List<string> _databases;

		public LocateSearcher(List<string> databases)
		{
			_databases = databases;
		}

		public IEnumerable<string> Search(string pattern, bool caseInsensitive, bool useRegex)
		{
			Regex? regex = null;
			if (useRegex)
			{
				regex = new Regex(pattern, caseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None);
			}

			foreach (var db in _databases)
			{
				foreach (var result in SearchDb(db, pattern, regex, caseInsensitive, useRegex))
				{
					yield return result;
				}
			}
		}

		private IEnumerable<string> SearchDb(string db, string pattern, Regex? regex, bool caseInsensitive, bool useRegex)
		{
			if (!File.Exists(db))
			{
				Console.Error.WriteLine($"Warning: Database file not found: {db}");
				yield break;
			}

			using var stream = new FileStream(
				db,
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				1024 * 1024,
				FileOptions.SequentialScan
			);

			var reader = new Archive32Reader(stream);

			foreach (var entry in reader.Traverse())
			{
				if (entry is Archive32FileEntry fileEntry)
				{
					var name = fileEntry.FullName;

					if (useRegex)
					{
						if (regex!.IsMatch(name))
							yield return name;
					}
					else
					{
						var comparison = caseInsensitive
							? StringComparison.OrdinalIgnoreCase
							: StringComparison.Ordinal;

						if (name.Contains(pattern, comparison))
							yield return name;
					}
				}
			}
		}
	}
}
