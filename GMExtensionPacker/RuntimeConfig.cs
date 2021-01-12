using System;
using System.IO;

namespace GMExtensionPacker
{
    internal sealed class RuntimeConfig
    {
        public string InputPath { get; private set; }

        public string OutputPath { get; private set; }

        public GmVersion Version { get; private set; }

        public bool ModeConvertToPackage => InputPath.EndsWith(".yy");

        private RuntimeConfig()
        {
            // NOTE Private ctor to enforce factory pattern
        }

        public static RuntimeConfig FromCommandLine(string[] args)
        {
            var config = new RuntimeConfig();
            foreach (var arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    if (arg == "-v22")
                    {
                        config.Version = GmVersion.Gm22;
                        continue;
                    }
                    else if (arg == "-v23")
                    {
                        config.Version = GmVersion.Gm23;
                        continue;
                    }
                }
                
                if (config.InputPath == null)
                {
                    config.InputPath = arg;
                    continue;
                }
                else if (config.OutputPath == null)
                {
                    config.OutputPath = arg;
                    continue;
                }

                Console.WriteLine($"Unknown option: {arg}");
            }

            if (config.Version == GmVersion.None && config.InputPath != null)
            {
                if (config.InputPath.EndsWith(".yymps"))
                    config.Version = GmVersion.Gm23;
                else if (config.InputPath.EndsWith(".yymp"))
                    config.Version = GmVersion.Gm22;
            }

            if (config.OutputPath == null && config.InputPath != null)
            {
                if (config.InputPath.EndsWith(".yymps") || config.InputPath.EndsWith(".yymp"))
                    config.OutputPath = Path.ChangeExtension(config.InputPath, "yy");
                else if (config.InputPath.EndsWith(".yy"))
                {
                    if (config.Version == GmVersion.Gm22)
                        config.OutputPath = Path.ChangeExtension(config.InputPath, "yymp");
                    else if (config.Version == GmVersion.Gm23)
                        config.OutputPath = Path.ChangeExtension(config.InputPath, "yymps");
                }
            }

            if (config.Version == GmVersion.None || config.InputPath == null || config.OutputPath == null)
                return null;

            return config;
        }

        public static void ShowHelp()
        {
            Console.WriteLine("gmextpack <options> input [output]");
            Console.WriteLine("Options");
            Console.WriteLine("    -v22      Generate 2.2.5 compatible files");
            Console.WriteLine("    -v23      Generate 2.3.1 compatible files");
            Console.WriteLine();
            Console.WriteLine("input         Path to a *.yymp, *.yymps, or extension *.yy file");
            Console.WriteLine("[output]      Path to output file. OPTIONAL");
            Console.WriteLine("    If not provided then input filename is used to generate the output name");
            Console.WriteLine("    Output for yymp/s will be to a directory, not file");
        }
    }

    internal enum GmVersion
    {
        None,
        Gm22,
        Gm23
    }
}
