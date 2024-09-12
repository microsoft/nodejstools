using System;
using System.IO;
using System.Security;

namespace MigrateToJsps
{
    public class MigrationLibrary
    {
        public static string Migrate(string njsprojFile, string projectDir) 
        {
            if (string.IsNullOrEmpty(njsprojFile))
            {
                // LOG ERROR!!

                return null;
                //throw new ArgumentNullException("Please input a non-empty path to your .njsproj file.");
            }

            if (!string.IsNullOrEmpty(projectDir) && VerifyNewProjectDir(projectDir))
            {
                if (VerifyNjsprojPath(njsprojFile))
                {
                    return MigrateProject(njsprojFile, projectDir);
                }
                return null;
                //throw new ArgumentException(".njsproj path not valid");
            }
            else
            {
                return null;
                //throw new ArgumentException("Destination path not valid");
            }
        }

        private static string MigrateProject(string njsprojFilepath, string projectDir)
        {
            Logger logger = new Logger(projectDir);

            // TODO: move to JspsProjectCreator constructor
            var njsprojFile = NjsprojFileReader.ProcessNjsproj(njsprojFilepath);

            var jspsProjCreator = new JspsProjectCreator(projectDir, njsprojFile, logger);

            var esprojFilepath = jspsProjCreator.CreateJspsProject();

            logger.WriteLogFile();

            return esprojFilepath;
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
