using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace MigrateToJsps
{
    internal class NjsprojFileReader
    {
        //TODO: change return type to void
        public static NjsprojFileModel ProcessNjsproj(string njsprojPath)
        {
            var name = "";
            var startupFile = "";
            var projectTypeGuidsString = "";
            var nodejsPort = "";

            using (var fileStream = File.Open(njsprojPath, FileMode.Open, FileAccess.Read))
            {
                var xmlSerializer = new XmlSerializer(typeof(Project));

                var ntvsProj = (Project)xmlSerializer.Deserialize(fileStream);

                if (ntvsProj == null)
                {
                    throw new Exception("Deserialized project is null");
                }

                // get startup file and name of project
                foreach (var propertyGroup in ntvsProj.PropertyGroup)
                {
                    if (!string.IsNullOrEmpty(propertyGroup.Name))
                    {
                        name = propertyGroup.Name;
                    }
                    if (!string.IsNullOrEmpty(propertyGroup.StartupFile))
                    {
                        startupFile = propertyGroup.StartupFile;
                    }
                    if (!string.IsNullOrEmpty(propertyGroup.ProjectTypeGuids))
                    {
                        projectTypeGuidsString = propertyGroup.ProjectTypeGuids;
                    }
                    if (!string.IsNullOrEmpty(propertyGroup.NodejsPort))
                    {
                        nodejsPort = propertyGroup.NodejsPort;
                    }
                }

                List<string> files = new List<string>();
                List<string> folders = new List<string>();

                foreach (var itemGroup in ntvsProj.ItemGroup)
                {
                    foreach (var content in itemGroup.Content)
                    {
                        if (content.Include != null)
                        {
                            files.Add(content.Include);
                        }
                    }

                    foreach (var compile in itemGroup.Compile)
                    {
                        if (compile.Include != null)
                        {
                            files.Add(compile.Include);
                        }
                    }

                    foreach (var none in itemGroup.None)
                    {
                        if (none.Include != null)
                        {
                            files.Add(none.Include);
                        }
                    }

                    foreach (var folder in itemGroup.Folder)
                    {
                        if (folder.Include != null)
                        {
                            folders.Add(folder.Include);
                        }
                    }
                }

                List<Guid> projectTypeGuids = 
                    string.IsNullOrEmpty(projectTypeGuidsString) ? 
                    new List<Guid>() : 
                    projectTypeGuidsString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(guidString => Guid.Parse(guidString)).ToList();

                return new NjsprojFileModel() { ProjectName = name, StartupFile = startupFile, ProjectFiles = files, ProjectTypeGuids = projectTypeGuids, ProjectIncludeFolders = folders, NodejsPort = nodejsPort };
            }
        }
    }
}
