using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrateToJsps
{
    internal static class NugetConfigWriter
    {
        private static string template =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <clear />
    <add key=""nuget.org"" value=""https://api.nuget.org/v3/index.json"" />
  </packageSources>
  <disabledPackageSources>
    <clear />
  </disabledPackageSources>
</configuration>
";

        public static void GenerateNugetConfig(string projectDir, Logger logger)
        {
            var filePath = Path.Combine(projectDir, "nuget.config");

            using (FileStream fs = File.Create(filePath))
            {
                byte[] buffer = new UTF8Encoding(true).GetBytes(template);
                fs.Write(buffer, 0, buffer.Length);

                logger.AddFile(filePath);
            }
        }
    }
}
