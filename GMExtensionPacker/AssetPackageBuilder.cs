using GMExtensionPacker.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace GMExtensionPacker
{
    internal sealed class AssetPackageBuilder
    {
        private readonly List<Models.Gm22.AssetPackageModel.Resource> resources;
        private readonly string workingDirectory;
        private readonly string scriptsDirectory;

        private readonly string packageId;

        private AssetPackageBuilder(string workingDirectory, string packageId)
        {
            this.workingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
            this.packageId = packageId ?? throw new ArgumentNullException(nameof(packageId));

            scriptsDirectory = Path.Combine(workingDirectory, "scripts");

            resources = new List<Models.Gm22.AssetPackageModel.Resource>();
        }

        public static void CreateFromExtension(string inputPath, string outputPath)
        {
            using (var workingDirectory = WorkingDirectory.Create())
            {
                var packageId = Path.GetFileNameWithoutExtension(outputPath);
                var assetPackage = new AssetPackageBuilder(workingDirectory, packageId);
                assetPackage.CreateFromExtension(inputPath);

                ZipFile.CreateFromDirectory(workingDirectory, outputPath);
            }
        }

        private void CreateFromExtension(string inputPath)
        {
            var extensionDirectory = Path.GetDirectoryName(inputPath);
            var extension = Json.Deserialize<Models.Gm22.GMExtensionModel>(inputPath);

            Directory.CreateDirectory(scriptsDirectory);

            foreach (var file in extension.files)
            {
                if (!file.filename.EndsWith(".gml"))
                    throw new NotSupportedException($"Cannot convert extension file '{file.filename}' to package");

                ConvertGMLFile(file, extensionDirectory);
                ConvertConstants(file);
                ConvertInit(file);
                ConvertFinal(file);
            }

            var assetPackagePath = Path.Combine(workingDirectory, "assetpackage.yy");
            Json.SerializeToFile(assetPackagePath, new Models.Gm22.AssetPackageModel
            {
                name = packageId,
                packageID = packageId,
                publisherName = extension.author,
                resources = resources
            });
        }

        private void ConvertGMLFile(Models.Gm22.GMExtensionFileModel file, string extensionDirectory)
        {
            var inFilePath = Path.Combine(extensionDirectory, file.filename);
            using (var fsIn = File.OpenRead(inFilePath))
            using (var reader = new StreamReader(fsIn))
            {
                FileStream fsOut = null;
                StreamWriter writer = null;

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#define"))
                    {
                        writer?.Dispose();
                        fsOut?.Close();

                        var scriptName = line.Split(' ')[1];
                        fsOut = CreateScriptFile(scriptsDirectory, scriptName);
                        writer = new StreamWriter(fsOut);
                        continue;
                    }

                    if (writer != null)
                        writer.WriteLine(line);
                }

                writer?.Dispose();
                fsOut?.Close();
            }
        }

        private void ConvertConstants(Models.Gm22.GMExtensionFileModel file)
        {
            if (file.constants == null || file.constants.Count <= 0)
                return;

            var scriptName = Path.GetFileNameWithoutExtension(file.filename) + "_ext_macros";
            using (var stream = CreateScriptFile(scriptsDirectory, scriptName))
            using (var writer = new StreamWriter(stream))
            {
                foreach (var constant in file.constants)
                    writer.WriteLine($"#macro {constant.constantName} {constant.value}");
            }
        }

        private void ConvertInit(Models.Gm22.GMExtensionFileModel file)
        {
            if (string.IsNullOrEmpty(file.init))
                return;

            var scriptName = Path.GetFileNameWithoutExtension(file.filename) + "_ext_init";
            using (var stream = CreateScriptFile(scriptsDirectory, scriptName))
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine($"gml_pragma(\"global\", \"{file.init}();\");");
            }
        }

        private void ConvertFinal(Models.Gm22.GMExtensionFileModel file)
        {
            if (string.IsNullOrEmpty(file.final))
                return;

            Console.WriteLine($"Warning: The finalizer '{file.final}' will not run in resulting package.");
        }

        private FileStream CreateScriptFile(string scriptsDirectory, string scriptName)
        {
            // TODO scriptsDirectory as a field

            var directory = Path.Combine(scriptsDirectory, scriptName);
            var gmlPath = Path.Combine(directory, scriptName + ".gml");
            var yyPath = Path.Combine(directory, scriptName + ".yy");

            Directory.CreateDirectory(directory);

            resources.Add(Models.Gm22.AssetPackageModel.Resource.NewScript(scriptName, packageId));
            Json.SerializeToFile(yyPath, new Models.Gm22.GMScriptModel
            {
                name = scriptName,
                IsCompatibility = false,
                IsDnD = false
            });

            return File.OpenWrite(gmlPath);
        }
    }
}
