using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtvsMigration
{
    internal static class LaunchJsonWriter
    {
        private static string template =
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
        public static void CreateLaunchJson(string projectRootDir, string startupFile)
        {
            var vscodeDir = Path.Combine(projectRootDir, ".vscode");
            Directory.CreateDirectory(vscodeDir);
            var filePath = Path.Combine(vscodeDir, "launch.json");
            var launchJson = template.Replace("*startupFile*", startupFile);

            using (FileStream fs = File.Create(filePath))
            {
                byte[] buffer = new UTF8Encoding(true).GetBytes(launchJson);
                fs.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
