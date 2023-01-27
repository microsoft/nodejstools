using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NtvsMigration
{
    internal static class EsprojFileWriter
    {

        private static string template =
@"<Project Sdk=""Microsoft.VisualStudio.JavaScript.Sdk/0.5.64-alpha"">
  <PropertyGroup>
    <!-- Command to run on project build -->
    <BuildCommand></BuildCommand>
    <!-- Command to run on project clean -->
    <CleanCommand></CleanCommand>
  </PropertyGroup>
</Project>
";
        public static void WriteEsproj(string projectDir, string projectName)
        {
            var fileName = projectName + ".esproj";
            var filePath = Path.Combine(projectDir, fileName);

            using (FileStream fs = File.Create(filePath))
            {
                byte[] buffer = new UTF8Encoding(true).GetBytes(template);
                fs.Write(buffer, 0, buffer.Length);
            }
        }

        private static string GetSdkVersion()
        {
            // TODO: figure out if sdk is installed on machine?
            // do i need to do that or will VS automatically install given version
            throw new NotImplementedException();
        }
    }
}
