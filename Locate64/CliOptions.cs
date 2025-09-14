using Locate64.Engine.Search;

namespace Locate64
{
	internal class CliOptions
	{
		public List<string> DatabaseFiles { get; set; } = new();
		public List<string> Patterns { get; set; } = new();
		public bool CaseSensitive { get; set; } = false; // default: insensitive
		public bool UseRegex { get; set; }
		public bool RetainConfiguredDb { get; set; }
		public bool ShowHelp { get; set; }
		public MatchMode MatchMode { get; set; } = MatchMode.Fullpath;
		public HashSet<string> Extensions { get; set; } = new();
		public bool InvertMatch { get; set; }
		public bool MatchExact { get; set; }
		public SearchTarget Target { get; set; } = SearchTarget.FilesAndFolders;
		public bool NoConfig { get; set; }
		public string? ConfigFile { get; set; }

		public static CliOptions Parse(string[] args)
		{
			var opts = new CliOptions();

			// First pass for key=value style args
			foreach (var arg in args)
			{
				if (arg.StartsWith("--match=", StringComparison.OrdinalIgnoreCase))
				{
					var mode = arg.Substring("--match=".Length).ToLowerInvariant();
					opts.MatchMode = mode switch
					{
						"basename" => MatchMode.Basename,
						"fullpath" => MatchMode.Fullpath,
						_ => opts.MatchMode
					};
				}
				else if (arg.Equals("--match-exact", StringComparison.OrdinalIgnoreCase))
				{
					opts.MatchExact = true;
				}
				else if (arg.StartsWith("--extension=", StringComparison.OrdinalIgnoreCase))
				{
					var list = arg.Substring("--extension=".Length)
						.Split(',', StringSplitOptions.RemoveEmptyEntries);
					foreach (var ext in list)
					{
						opts.Extensions.Add(ext.Trim().ToLowerInvariant());
					}
				}
				else if (arg.StartsWith("--only=", StringComparison.OrdinalIgnoreCase))
				{
					var val = arg.Substring("--only=".Length).ToLowerInvariant();
					opts.Target = val switch
					{
						"files" => SearchTarget.FilesOnly,
						"folders" => SearchTarget.FoldersOnly,
						_ => SearchTarget.FilesAndFolders
					};
				}
				else if (arg.StartsWith("--config=", StringComparison.OrdinalIgnoreCase))
				{
					opts.ConfigFile = arg.Substring("--config=".Length).Trim();
				}
				else if (arg.Equals("--no-config", StringComparison.OrdinalIgnoreCase))
				{
					opts.NoConfig = true;
				}
			}

			// Second pass for flags and positional args
			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i])
				{
					case "-d":
						if (i + 1 < args.Length)
						{
							opts.DatabaseFiles.AddRange(args[++i].Split(';'));
						}
						break;

					case "-i":
						opts.CaseSensitive = false; // explicit insensitive
						break;

					case "--case-sensitive":
						opts.CaseSensitive = true;
						break;

					case "-r":
						opts.UseRegex = true;
						break;

					case "--retain-configured-db":
					case "-C":
						opts.RetainConfiguredDb = true;
						break;

					case "--invert-match":
						opts.InvertMatch = true;
						break;

					case "-h":
					case "--help":
						opts.ShowHelp = true;
						break;

					default:
						if (!args[i].StartsWith("--"))
						{
							opts.Patterns.Add(args[i]);
						}
						break;
				}
			}

			return opts;
		}
	}
}
