using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;

namespace MigrateToJsps
{
    internal static class LaunchJsonWriter
    {
        private static Configuration NodeLaunchTemplate
        {
            get 
            {
                return new Configuration()
                {
                    Name = "Debug node process",
                    Type = "node",
                    Request = "launch",
                    Program = @"${workspaceFolder}/*STARTUP_FILE*",
                    Skipfiles = new string[] { @"<node_internals/**" },
                    StopOnEntry = true,
                    Cwd = @"${workspaceFolder}",
                    Console = "externalTerminal"
                }; 
            }
        }

        private static Configuration ChromeLaunchTemplate
        {
            get
            {
                return new Configuration()
                {
                    Name = "localhost (Chrome)",
                    Type = "chrome",
                    Request = "launch",
                    Url = @"http://localhost:*PORT*",
                    WebRoot = @"${workspaceFolder}/public"
                };
            }
        }

        private static Configuration EdgeLaunchTemplate
        {
            get
            {
                return new Configuration()
                {
                    Name = "localhost (Edge)",
                    Type = "edge",
                    Request = "launch",
                    Url = @"http://localhost:*PORT*",
                    WebRoot = @"${workspaceFolder}/public"
                };
            }
        }

        private static Compound CompoundLaunchTemplate 
        {
            get
            {
                return new Compound()
                {
                    Name = "Launch Node and Browser",
                    Configurations = new string[] { "Debug node process", "localhost (Chrome)" }
                };
            }
        }

        public static void CreateLaunchJson(string projectDir, NjsprojFileModel njsprojFileModel, Logger logger)
        {
            var vscodeDir = Path.Combine(projectDir, ".vscode");
            Directory.CreateDirectory(vscodeDir);
            var filePath = Path.Combine(vscodeDir, "launch.json");

            LaunchJson launchJson = null;

            if (njsprojFileModel.ProjectTypeGuids.Contains(ProjectGuids.DotnetMVC5WebApp)) // this means the NTVS project is one of the "web app" templates
            {
                var port = string.IsNullOrEmpty(njsprojFileModel.NodejsPort) ? "3000" : njsprojFileModel.NodejsPort;

                var nodeLaunch = NodeLaunchTemplate;
                nodeLaunch.Program = nodeLaunch.Program.Replace("*STARTUP_FILE*", njsprojFileModel.StartupFile);
                nodeLaunch.Args = njsprojFileModel.ScriptArguments.Split(';');
                nodeLaunch.Env = new JObject(new JProperty("port", port));

                var edgeLaunch = EdgeLaunchTemplate;
                edgeLaunch.Url = edgeLaunch.Url.Replace("*PORT*", port);

                var chromeLaunch = ChromeLaunchTemplate;
                chromeLaunch.Url = chromeLaunch.Url.Replace("*PORT*", port);

                Configuration[] launchConfigs;

                if (bool.Parse(njsprojFileModel.StartWebBrowser))
                {
                    launchConfigs = new Configuration[]
                    {
                        edgeLaunch, chromeLaunch, nodeLaunch
                    };
                }
                else
                {
                    launchConfigs = new Configuration[]
                    {
                        nodeLaunch, edgeLaunch, chromeLaunch
                    };
                }

                var compoundLaunch = new Compound[] { CompoundLaunchTemplate };
                launchJson = new LaunchJson() { Configurations = launchConfigs, Compounds = compoundLaunch };
            }
            else // this means the NTVS project is a console app
            {
                var nodeLaunch = NodeLaunchTemplate;
                nodeLaunch.Program = nodeLaunch.Program.Replace("*STARTUP_FILE*", njsprojFileModel.StartupFile);

                Configuration[] launchConfigs = new Configuration[] { nodeLaunch };
                launchJson = new LaunchJson() { Configurations = launchConfigs };
            }

            using (FileStream fs = File.Create(filePath))
            {
                byte[] buffer = new UTF8Encoding(true).GetBytes(launchJson.ToJsonString());
                fs.Write(buffer, 0, buffer.Length);

                logger.AddFile(filePath);
            }
        }
    }
}
