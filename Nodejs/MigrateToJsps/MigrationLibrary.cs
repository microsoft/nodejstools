using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MigrateToJsps
{
    public class MigrationLibrary
    {
        public static void Migrate(string njsprojFile, string newProjectDir) 
        {
            if (string.IsNullOrEmpty(njsprojFile)) throw new ArgumentNullException("Please input a non-empty path to your .njsproj file.");

            if (!string.IsNullOrEmpty(newProjectDir) && VerifyNewProjectDir(newProjectDir))
            {
                if (VerifyNjsprojPath(njsprojFile))
                {
                    MigrateProject(njsprojFile, newProjectDir);
                }
            }
            else
            {
                throw new ArgumentException("Destination path not valid");
            }
        }

        private static void MigrateProject(string njsprojFilepath, string destinationDir)
        {
            // TODO: move to JspsProjectCreator constructor
            var njsprojFile = NjsprojFileReader.ProcessNjsproj(njsprojFilepath);

            var njsprojDir = Path.GetDirectoryName(njsprojFilepath);
            var renamedNjsprojDir = njsprojDir + "-old";
            Directory.Move(njsprojDir, renamedNjsprojDir);

            var jspsProjCreator = new JspsProjectCreator(renamedNjsprojDir, njsprojFile, destinationDir);

            jspsProjCreator.CreateJspsProject();

            // write .sln file?
        }

        private static bool VerifyNewProjectDir(string userInputtedPath)
        {
            if (userInputtedPath == null)
            {
                return false;
            }
            var newProjectDir = userInputtedPath.Trim();
            // TODO: clean up for relative paths -- permissions errors?
            if (!Path.IsPathRooted(newProjectDir))
            {
                return false;
            }
            return Directory.Exists(newProjectDir);
        }

        private static bool VerifyNjsprojPath(string pathToNjsproj)
        {
            string fullPath;

            try
            {
                fullPath = Path.GetFullPath(pathToNjsproj);
            }
            catch (Exception ex) when (ex is ArgumentException ||
                                       ex is ArgumentNullException ||
                                       ex is NotSupportedException)
            {
                Console.WriteLine("Please input a valid filepath.");
                throw;
            }
            catch (Exception ex) when (ex is SecurityException)
            {
                Console.WriteLine("Does not have required permissions.");
                throw;
            }
            catch (Exception ex) when (ex is PathTooLongException)
            {
                Console.WriteLine("Path is too long.");
                throw;
            }

            // verify it is an njsproj file
            var fileExtension = Path.GetExtension(fullPath);

            if (String.IsNullOrEmpty(fileExtension) || fileExtension != ".njsproj")
            {
                Console.WriteLine("Path does not point to an .njsproj file.");
                return false;
            }

            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"{fullPath} does not exist.");
                return false;
            }

            return true;
        }
    }
}
