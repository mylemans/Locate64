using Locate64.Engine.Search;
using System.Text.Json;

namespace Locate64
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var options = CliOptions.Parse(args);

			if (options.ShowHelp)
			{
				PrintHelp();
				return;
			}

			// Default DB paths
			var userDb = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Locate64", "locate64.dbs"
			);

			var systemDb = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
				"Locate64", "locate64.dbs"
			);

			// Try config file first (unless suppressed)
			SearchConfig? configFromFile = null;
			if (!options.NoConfig)
			{
				string? configPath = options.ConfigFile;

				if (string.IsNullOrEmpty(configPath))
				{
					var userConfig = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
						"Locate64", "locate64.config"
					);

					var systemConfig = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
						"Locate64", "locate64.config"
					);

					if (File.Exists(userConfig))
					{
						configPath = userConfig;
					}
					else if (File.Exists(systemConfig))
					{
						configPath = systemConfig;
					}
				}

				if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
				{
					try
					{
						var json = File.ReadAllText(configPath);
						configFromFile = JsonSerializer.Deserialize<SearchConfig>(json);
					}
					catch
					{
						// swallow silently — no debug output
					}
				}
			}

			// Build final database list
			var dbs = new List<string>();

			if (options.DatabaseFiles.Count > 0)
			{
				if (options.RetainConfiguredDb)
				{
					if (File.Exists(userDb))
					{
						dbs.Add(userDb);
					}
					else if (File.Exists(systemDb))
					{
						dbs.Add(systemDb);
					}
				}

				dbs.AddRange(options.DatabaseFiles);
			}
			else if (configFromFile?.Databases?.Count > 0)
			{
				dbs.AddRange(configFromFile.Databases);
			}
			else
			{
				if (File.Exists(userDb))
				{
					dbs.Add(userDb);
				}
				else if (File.Exists(systemDb))
				{
					dbs.Add(systemDb);
				}
				else
				{
					// Fallback: Locate32 default
					var locate32Db = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
						"Locate32", "files.dbs"
					);

					if (File.Exists(locate32Db))
					{
						dbs.Add(locate32Db);
					}
				}
			}

			if (dbs.Count == 0)
			{
				Console.Error.WriteLine("No database file found.");
				return;
			}

			// Merge config + CLI
			var config = configFromFile ?? new SearchConfig();
			config.Databases = dbs;
			config.CaseInsensitive = !options.CaseSensitive;
			config.UseRegex = options.UseRegex;
			config.MatchMode = options.MatchMode;
			config.Extensions = options.Extensions;
			config.InvertMatch = options.InvertMatch;
			config.MatchExact = options.MatchExact;
			config.Target = options.Target;

			var searcher = new SearchEngine(config)
			{
				OnError = (db, ex) =>
				{
					Console.Error.WriteLine($"[WARN] Failed to process database {db}: {ex.Message}");
					return null; // swallow and continue
				}
			};

			// If no patterns provided → list everything
			if (options.Patterns.Count == 0)
			{
				foreach (var result in searcher.Search(string.Empty))
				{
					Console.WriteLine(result);
				}
				return;
			}

			foreach (var pattern in options.Patterns)
			{
				foreach (var result in searcher.Search(pattern))
				{
					Console.WriteLine(result);
				}
			}
		}

		private static void PrintHelp()
		{
			var exeName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);

			string[] lines =
			{
				$"{exeName} - fast file search for Windows",
				"",
				"Usage:",
				$"  {exeName} [options] [pattern]",
				"",
				"Options:",
				"  -d <files>                 Use specific database(s), separated by ';'",
				"  --retain-configured-db      Include default database along with -d files",
				"  -r                          Treat pattern as a regular expression",
				"  -i                          Case-insensitive search (default)",
				"  --case-sensitive            Force case-sensitive search",
				"  --match=basename|fullpath   Match only filename or full path (default: fullpath)",
				"  --match-exact               Require exact match instead of substring",
				"  --extension=ext1,ext2,...   Comma-separated list of extensions to filter (files only)",
				"  --invert-match              Invert the match result",
				"  --only=files|folders        Restrict results to files or folders (default: both)",
				"  --config=<file>             Load settings from a specific config file",
				"  --no-config                 Do not load any config file",
				"  -h, --help                  Show this help message",
				"",
				"Examples:",
				$"  {exeName} myfile.txt",
				$"  {exeName} --only=files --extension=mp4,avi",
				$"  {exeName} --match=basename --match-exact readme.md",
				$"  {exeName} -r \".*\\.sln$\""
			};

			foreach (var line in lines)
			{
				Console.WriteLine(line);
			}
		}
	}
}
