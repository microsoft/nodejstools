using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            // TODO: figure out if sdk is installed on machine?
            // do i need to do that or will VS automatically install given version
            throw new NotImplementedException();
        }
    }
}
