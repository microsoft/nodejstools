using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            using (var fileStream = File.Open(njsprojPath, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Project));

                Project ntvsProj = (Project)xmlSerializer.Deserialize(fileStream);

                if (ntvsProj == null)
                {
                    Console.Error.WriteLine("Something went wrong");
                    throw new Exception("Deserialized project is null");
                }

                // get startup file and name of project
                foreach (var propertyGroup in ntvsProj.PropertyGroup)
                {
                    if (!string.IsNullOrEmpty(propertyGroup.Name))
                    {
                        //Console.WriteLine("Name prop is: " + propertyGroup.Name);
                        name = propertyGroup.Name;
                    }
                    if (!string.IsNullOrEmpty(propertyGroup.StartupFile))
                    {
                        //Console.WriteLine("StartupFile prop is: " + propertyGroup.StartupFile);
                        startupFile = propertyGroup.StartupFile;
                    }
                    if (!string.IsNullOrEmpty(propertyGroup.ProjectTypeGuids))
                    {
                        //Console.WriteLine("StartupFile prop is: " + propertyGroup.StartupFile);
                        projectTypeGuidsString = propertyGroup.ProjectTypeGuids;
                    }
                }

                List<string> files = new List<string>();
                foreach (var itemGroup in ntvsProj.ItemGroup)
                {
                    foreach (var content in itemGroup.Content)
                    {
                        //TODO: need to look at none includes also
                        if (content.Include != null)
                        {
                            files.Add(content.Include);
                        }
                    }
                }

                List<Guid> projectTypeGuids = 
                    string.IsNullOrEmpty(projectTypeGuidsString) ? 
                    new List<Guid>() : 
                    projectTypeGuidsString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(guidString => Guid.Parse(guidString)).ToList();

                return new NjsprojFileModel() { ProjectName = name, StartupFile = startupFile, ProjectFiles = files, ProjectTypeGuids = projectTypeGuids };
            }
        }
    }
}
