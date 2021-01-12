using System;
using System.Text;
using System.Threading.Tasks;

namespace GMExtensionPacker
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            // gmextpack -v23 extension.yy
            // gmextpack -v22 package.yymps

            var config = RuntimeConfig.FromCommandLine(args);
            if (config == null)
            {
                RuntimeConfig.ShowHelp();
                return;
            }

            if (config.ModeConvertToPackage)
            {
                AssetPackageBuilder.CreateFromExtension(config.InputPath, config.OutputPath);
                Console.WriteLine($"Saved to '{config.OutputPath}'...");
            }
            else
            {
                ExtensionBuilder.Do(config.InputPath, config.OutputPath);
                Console.WriteLine($"Saved to '{config.OutputPath}'...");
            }
        }
    }
}
