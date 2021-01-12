using GMExtensionPacker.Models.Common;
using GMExtensionPacker.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GMExtensionPacker
{
    internal sealed class ExtensionBuilder
    {
        public static void Do(string inputPath, string outputPath)
        {
            var extension = new Models.Gm22.GMExtensionFileModel
            {
                filename = Path.GetFileNameWithoutExtension(outputPath) + ".gml",
                kind = Models.Gm22.ExtensionKind.Gml,
                uncompress = false,
                copyToTargets = TargetPlatforms.AllPlatforms,

                constants = new List<Models.Gm22.GMExtensionConstantModel>(),
                functions = new List<Models.Gm22.GMExtensionFunctionModel>(),
                ProxyFiles = new List<Models.Gm22.GMProxyFileModel>()
            };

            using (var workingDirectory = WorkingDirectory.Create())
            {
                ZipFile.ExtractToDirectory(inputPath, workingDirectory);

                var assetPackagePath = Path.Combine(workingDirectory, "assetpackage.yy");
                var assetPackage = Json.Deserialize<Models.Gm22.AssetPackageModel>(assetPackagePath);

                var initScriptName = assetPackage.packageID + "_ext_init";
                var macrosScriptName = assetPackage.packageID + "_ext_macros";

                var outputDirectory = Path.GetDirectoryName(outputPath);
                var gmlPath = Path.Combine(outputDirectory, assetPackage.packageID + ".gml");
                var fsGml = File.OpenWrite(gmlPath);
                var gmlWriter = new StreamWriter(fsGml);

                foreach (var resource in assetPackage.resources)
                {
                    var scriptPath = Path.Combine(workingDirectory, resource.resourcePath);
                    scriptPath = Path.ChangeExtension(scriptPath, "gml");
                    var scriptName = Path.GetFileNameWithoutExtension(scriptPath);

                    if (!File.Exists(scriptPath))
                        continue;

                    var gmlContent = File.ReadAllText(scriptPath);

                    if (scriptName == initScriptName)
                    {
                        const string Preamble = "gml_pragma(\"global\"";
                        if (!gmlContent.StartsWith(Preamble))
                        {
                            Console.WriteLine("Warning: Extension Init script does not start with gml_pragma global");
                            continue;
                        }

                        var l = gmlContent.IndexOf('\"', Preamble.Length) + 1;
                        var r = gmlContent.IndexOf('(', Preamble.Length);
                        extension.init = gmlContent.Substring(l, r - l);
                        continue;
                    }
                    else if (scriptName == macrosScriptName)
                    {
                        const string Preamble = "#macro ";
                        using (var reader = new StringReader(gmlContent))
                        {
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                if (!line.StartsWith(Preamble))
                                    continue;

                                var i1 = line.IndexOf(' ') + 1;
                                var i2 = line.IndexOf(' ', i1) + 1;
                                if (i1 <= 0 || i2 <= 0)
                                    continue;

                                extension.constants.Add(new Models.Gm22.GMExtensionConstantModel
                                {
                                    constantName = line.Substring(i1, i2 - i1 - 1),
                                    value = line.Substring(i2, line.Length - i2),
                                    hidden = false
                                });
                            }
                        }
                        continue;
                    }
                    else
                    {
                        var jsDoc = JsDocParser.Parse(gmlContent);

                        if (jsDoc.ReturnType == VariableType.None)
                        {
                            Console.WriteLine($"Warning: Script '{scriptName}' did not specify a return type. Defaulting to 'double'");
                            jsDoc.ReturnType = VariableType.Double;
                        }

                        gmlWriter.WriteLine($"#define {scriptName}");
                        gmlWriter.WriteLine(gmlContent);

                        extension.functions.Add(new Models.Gm22.GMExtensionFunctionModel
                        {
                            externalName = scriptName,
                            kind = Models.Gm22.ExtensionKind.Gml,
                            name = scriptName,
                            help = jsDoc.HelpString,
                            hidden = jsDoc.IsHidden,
                            returnType = jsDoc.ReturnType,
                            argCount = jsDoc.ArgumentCount,
                            args = jsDoc.Arguments
                        });
                    }
                }

                gmlWriter.Dispose();
                fsGml.Close();

                extension.order = extension.functions.Select(x => x.id).ToList();
                Json.SerializeToFile(outputPath, new Models.Gm22.GMExtensionModel
                {
                    name = Path.GetFileNameWithoutExtension(outputPath),
                    extensionName = "",
                    version = assetPackage.version,
                    packageID = "",
                    productID = "",
                    author = "",
                    date = DateTime.UtcNow,
                    license = "",
                    description = "",
                    helpfile = "",
                    iosProps = false,
                    androidProps = false,
                    installdir = "",
                    files = new List<Models.Gm22.GMExtensionFileModel> { extension },
                    classname = "",
                    androidclassname = "",
                    sourcedir = "",
                    macsourcedir = "",
                    maccompilerflags = "",
                    maclinkerflags = "",
                    iosplistinject = "",
                    androidinject = "",
                    androidmanifestinject = "",
                    androidactivityinject = "",
                    gradleinject = "",
                    iosSystemFrameworkEntries = new List<Models.Gm22.GMExtensionFrameworkEntryModel>(),
                    iosThirdPartyFrameworkEntries = new List<Models.Gm22.GMExtensionFrameworkEntryModel>(),
                    IncludedResources = new List<string>(), // TODO Include Files
                    copyToTargets = TargetPlatforms.AllPlatforms
                });
            }
        }
    }
}
