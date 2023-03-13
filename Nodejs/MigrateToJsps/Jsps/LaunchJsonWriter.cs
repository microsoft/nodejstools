using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateToJsps
{
    internal static class LaunchJsonWriter
    {
        private static string consoleAppLaunchTemplate =
@"{
  ""version"": ""0.2.0"",
  ""configurations"": [
    {
      ""type"": ""node"",
      ""request"": ""launch"",
      ""name"": ""Launch Program"",
      ""skipFiles"": [ ""<node_internals>/**"" ],
      ""program"": ""${workspaceFolder}/*startupFile*"",
      ""cwd"": ""${workspaceFolder}"",
      ""console"": ""externalTerminal""
    }
  ]
}
";

        private static string expressAppLaunchTemplate =
@"{
  ""version"": ""0.2.0"",
  ""configurations"": [
    {
      ""name"": ""localhost (Edge)"",
      ""type"": ""edge"",
      ""request"": ""launch"",
      ""url"": ""http://localhost:3000"",
      ""webRoot"": ""${workspaceFolder}\\public""
    },
    {
      ""name"": ""localhost (Chrome)"",
      ""type"": ""chrome"",
      ""request"": ""launch"",
      ""url"": ""http://localhost:3000"",
      ""webRoot"": ""${workspaceFolder}\\public""
    },
    {
      ""name"": ""Debug Dev Env"",
      ""type"": ""node"",
      ""request"": ""launch"",
      ""program"": ""${workspaceFolder}/*startupFile*"",
      ""cwd"": ""${workspaceFolder}"",
      ""stopOnEntry"": true
    }
  ],
  ""compounds"": [
    {
      ""name"": ""Launch Node and Browser"",
      ""configurations"": [
        ""Debug Dev Env"",
        ""localhost (Edge)""
      ]
    }
  ]
}
";

        public static void CreateLaunchJson(string projectTypeGuids, string projectRootDir, string startupFile)
        {
            var vscodeDir = Path.Combine(projectRootDir, ".vscode");
            Directory.CreateDirectory(vscodeDir);
            var filePath = Path.Combine(vscodeDir, "launch.json");

            var launchJson = "";
            if (projectTypeGuids.Contains(ProjectGuids.Express))
            {
                launchJson = expressAppLaunchTemplate.Replace("*startupFile*", startupFile);
            }
            else
            {
                launchJson = consoleAppLaunchTemplate.Replace("*startupFile*", startupFile);
            }

            using (FileStream fs = File.Create(filePath))
            {
                byte[] buffer = new UTF8Encoding(true).GetBytes(launchJson);
                fs.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
