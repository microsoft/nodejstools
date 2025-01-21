using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MigrateToJsps
{
    internal static class EsprojFileWriter
    {
        public static string WriteEsproj(string projectDir, string projectName, string startupFile, string port, Logger logger)
        {
            var fileName = projectName + ".esproj";
            var filePath = Path.Combine(projectDir, fileName);

            EsprojPropertyGroup propGroup = new EsprojPropertyGroup();
            
            var pkgJson = ReadPackageJson(projectDir);
            if (pkgJson != null)
            {
                if (StartCmdExistsInPkgJson(pkgJson)) {
                    var startupCmd = "npm run start";

                    if (!string.IsNullOrEmpty(port))
                    {
                        startupCmd = $"set PORT={port} & " + startupCmd; 
                    }
                    propGroup.StartupCommand = startupCmd;
                }

                propGroup.BuildCommand = BuildCmdExistsInPkgJson(pkgJson) ? "npm run build" : " ";

                propGroup.CleanCommand = CleanCmdExistsInPkgJson(pkgJson) ? "npm run clean" : " ";
            }
            else
            {
                propGroup.StartupCommand = "node " + startupFile;
            }

            EsprojFile esprojFile = new EsprojFile() { PropertyGroup = propGroup };
            var sdkVersion = GetSdkVersion();

            // If the SDK version is not found on NuGet fallback folder, keep the one hardcoded on EsProjFileModel
            if (sdkVersion != null)
            {
                esprojFile.Sdk = $"Microsoft.VisualStudio.JavaScript.Sdk/{sdkVersion}";
            }

            XmlSerializer serializer = new XmlSerializer(typeof(EsprojFile));

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            XmlSerializerNamespaces emptyNamespaces = new XmlSerializerNamespaces();
            emptyNamespaces.Add("", "");

            using (XmlWriter writer = XmlWriter.Create(filePath, settings))
            {
                serializer.Serialize(writer, esprojFile, emptyNamespaces);
            }

            logger.AddFile(filePath);

            return filePath;
        }

        private static bool StartCmdExistsInPkgJson(JObject packageJson)
        {
            if (packageJson.ContainsKey("scripts"))
            {
                return packageJson["scripts"]["start"] != null;
            }

            return false;
        }

        private static bool BuildCmdExistsInPkgJson(JObject packageJson)
        {
            if (packageJson.ContainsKey("scripts"))
            {
                return packageJson["scripts"]["build"] != null;
            }

            return false;
        }

        private static bool CleanCmdExistsInPkgJson(JObject packageJson)
        {
            if (packageJson.ContainsKey("scripts"))
            {
                return packageJson["scripts"]["clean"] != null;
            }

            return false;
        }

        private static JObject ReadPackageJson(string projectDir)
        {
            string filePath = Path.Combine(projectDir, "package.json");
            if (File.Exists(filePath))
            {
                string packageJsonContent = File.ReadAllText(filePath);

                JObject packageJson = JsonConvert.DeserializeObject<JObject>(packageJsonContent);

                return packageJson;
            }

            return null;
        }

        private static string GetSdkVersion()
        {
            Version newestVersion = null;

            try
            {
                // Use the installed version of JSPS on the NuGet fallback folder.
                var versions = Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft Visual Studio", "Shared", "NuGetPackages", "microsoft.visualstudio.javascript.sdk"));
                newestVersion = versions
                    .Select(v =>
                    {
                        Version.TryParse(Path.GetFileName(v), out var version);
                        return version;
                    })
                    .OrderByDescending(v => v)
                    .FirstOrDefault();
            }
            catch (Exception)
            {
                // If the SDK version is not found on NuGet fallback folder, use the one hardcoded on EsProjFileModel
                return null;
            }

            return newestVersion?.ToString();
        }
    }
}
