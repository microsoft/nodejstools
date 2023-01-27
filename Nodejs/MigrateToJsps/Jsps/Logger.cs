using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using System.Collections;

namespace MigrateToJsps
{
    public class Logger
    {
        private string projectDir;
        private List<string> listOfFiles;

        public string LogfilePath { get => Path.Combine(projectDir, "PROJECT_MIGRATION_LOG.txt"); }

        private string template =
@"Hello! Thank you for trying out our new JavaScript and TypeScript project experience. We've added the below list of files to your project directory in order to enable the new experience:

*LIST_OF_NEW_FILES*

We'd love to get your feedback! Please submit any bugs or improvements that need to be made by going to Help -> Send Feedback.

If you'd like to revert to your original project, you can right-click on the project and click on 'Revert Project To Old Experience'.
";

        public Logger(string projectDir)
        {
            this.projectDir = projectDir;
            this.listOfFiles = new List<String>();
        }

        public void AddFile(string filepath)
        {
            listOfFiles.Add(filepath);
        }

        public void WriteLogFile()
        {
            listOfFiles.Add(LogfilePath);
            string formattedFiles = string.Join(Environment.NewLine, listOfFiles);
            string strToWrite = template.Replace("*LIST_OF_NEW_FILES*", formattedFiles);

            File.WriteAllText(LogfilePath, strToWrite);
        }
    }
}
