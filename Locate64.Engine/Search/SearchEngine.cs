using Locate64.Engine.Data.Archive32File;
using System.Text.RegularExpressions;

namespace Locate64.Engine.Search
{
	public class SearchEngine
	{
		private readonly SearchConfig _config;

		public SearchEngine(SearchConfig config)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
		}

		/// <summary>
		/// Callback to decide how to handle exceptions.
		/// Args: database file path, exception.
		/// Return null → suppress and continue,
		/// same exception → rethrow,
		/// different exception → throw that instead.
		/// </summary>
		public Func<string, Exception, Exception?>? OnError { get; set; }

		public IEnumerable<SearchResult> Search(string? pattern)
		{
			Regex? regex = null;

			if (!string.IsNullOrEmpty(pattern))
			{
				if (_config.UseRegex)
				{
					regex = new Regex(
						pattern,
						_config.CaseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None
					);
				}
				else if (pattern.Contains('*') || pattern.Contains('?'))
				{
					// Convert wildcard into regex
					regex = WildcardToRegex(pattern, _config.CaseInsensitive, _config.MatchExact);
				}
			}

			foreach (var db in _config.Databases)
			{
				IEnumerable<SearchResult> results;

				try
				{
					results = SearchDb(db, pattern, regex);
				}
				catch (Exception ex)
				{
					if (OnError == null)
					{
						throw;
					}

					var decision = OnError(db, ex);
					if (decision == null)
					{
						continue;
					}

					if (ReferenceEquals(decision, ex))
					{
						throw;
					}

					throw decision;
				}

				foreach (var r in results)
				{
					yield return r;
				}
			}
		}

		private IEnumerable<SearchResult> SearchDb(string db, string? pattern, Regex? regex)
		{
			if (!File.Exists(db))
			{
				throw new FileNotFoundException("Database file not found", db);
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
				bool isFile = entry is Archive32FileEntry;
				bool isDir = entry is Archive32DirectoryEntry;

				// Target filter
				if (_config.Target == SearchTarget.FilesOnly && !isFile)
				{
					continue;
				}
				if (_config.Target == SearchTarget.FoldersOnly && !isDir)
				{
					continue;
				}

				string name = entry.FullName;
				if (_config.MatchMode == MatchMode.Basename)
				{
					name = entry.Name;
				}

				// Extension filter
				if (_config.Extensions.Count > 0)
				{
					if (isFile)
					{
						var ext = Path.GetExtension(entry.Name).TrimStart('.').ToLowerInvariant();

						if (!_config.Extensions.Contains(ext))
						{
							continue; // skip non-matching file extensions
						}
					}
					else
					{
						// If it's not a file, and extensions are required → skip
						continue;
					}
				}

				bool matched;

				// Pattern matching
				if (string.IsNullOrEmpty(pattern))
				{
					matched = true;
				}
				else if (regex != null)
				{
					matched = regex.IsMatch(name);
				}
				else if (_config.MatchExact)
				{
					var comparison = _config.CaseInsensitive
						? StringComparison.OrdinalIgnoreCase
						: StringComparison.Ordinal;
					matched = string.Equals(name, pattern, comparison);
				}
				else
				{
					var comparison = _config.CaseInsensitive
						? StringComparison.OrdinalIgnoreCase
						: StringComparison.Ordinal;
					matched = name.Contains(pattern, comparison);
				}

				if (_config.InvertMatch)
				{
					matched = !matched;
				}

				if (matched)
				{
					yield return new SearchResult(db, entry.FullName);
				}
			}
		}

		private static Regex WildcardToRegex(string pattern, bool caseInsensitive, bool exact)
		{
			var regexPattern = Regex.Escape(pattern)
				.Replace(@"\*", ".*")
				.Replace(@"\?", ".");

			if (exact)
			{
				regexPattern = "^" + regexPattern + "$";
			}

			var options = caseInsensitive ? RegexOptions.IgnoreCase : RegexOptions.None;
			return new Regex(regexPattern, options);
		}
	}
}
