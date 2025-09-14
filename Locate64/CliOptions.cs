namespace Locate64
{
    internal class CliOptions
    {
        public List<string> DatabaseFiles { get; set; } = new();
        public List<string> Patterns { get; set; } = new();
        public bool CaseInsensitive { get; set; }
        public bool UseRegex { get; set; }
        public bool RetainConfiguredDb { get; set; }

        public static CliOptions Parse(string[] args)
        {
            var opts = new CliOptions();
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
                        opts.CaseInsensitive = true;
                        break;
                    case "-r":
                        opts.UseRegex = true;
                        break;
                    case "--retain-configured-db":
                        opts.RetainConfiguredDb = true;
                        break;
                    default:
                        opts.Patterns.Add(args[i]);
                        break;
                }
            }
            return opts;
        }
    }
}
