namespace Locate64
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var options = CliOptions.Parse(args);

            // Default DB path (ProgramData)
            var defaultDb = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Locate64", "locate64.dbs"
            );

            var dbs = new List<string>();

            if (options.DatabaseFiles.Count > 0)
            {
                if (options.RetainConfiguredDb && File.Exists(defaultDb))
                    dbs.Add(defaultDb);

                dbs.AddRange(options.DatabaseFiles);
            }
            else
            {
                if (File.Exists(defaultDb))
                    dbs.Add(defaultDb);
            }

            if (dbs.Count == 0)
            {
                Console.Error.WriteLine("No database file found. Default search location: " + defaultDb);
                return;
            }

            if (options.Patterns.Count == 0)
            {
                Console.Error.WriteLine("No search pattern provided.");
                return;
            }

            var searcher = new LocateSearcher(dbs);
            foreach (var pattern in options.Patterns)
            {
                var results = searcher.Search(pattern, options.CaseInsensitive, options.UseRegex);
                foreach (var result in results)
                {
                    Console.WriteLine(result);
				}
            }
        }
    }
}
