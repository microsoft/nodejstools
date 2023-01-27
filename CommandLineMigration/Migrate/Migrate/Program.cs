using NtvsMigration;
using System;
using System.Reflection;
using System.Security;

namespace NtvsMigration
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please input a non-empty path to your .njsproj file.");
                return;
            }

            Console.WriteLine("Please enter the full path for an existing directory for the new project to live:");
            var newProjectDir = Console.ReadLine();
            if (!string.IsNullOrEmpty(newProjectDir) && VerifyNewProjectDir(newProjectDir))
            {
                var pathToNjsproj = args[0];

                if (VerifyNjsprojPath(pathToNjsproj))
                {
                    Migrate(pathToNjsproj, newProjectDir);
                }
            }
            else
            {
                Console.WriteLine("Destination path not valid.");
            }
        }

        private static void Migrate(string njsprojFilepath, string destinationDir)
        {
            // TODO: move to JspsProjectCreator constructor
            var njsprojFile = NjsprojFileReader.ProcessNjsproj(njsprojFilepath);

            var njsprojDir = Path.GetDirectoryName(njsprojFilepath);

            var jspsProjCreator = new JspsProjectCreator(njsprojDir, njsprojFile, destinationDir);

            jspsProjCreator.CreateJspsProject();

            // write .sln file?
        }

        private static Stream GetManifestResourceStream(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(resourceName);
        }

        private static bool VerifyNewProjectDir(string? userInputtedPath)
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

            if (!File.Exists(fullPath)) {
                Console.WriteLine($"{fullPath} does not exist.");
                return false;
            }

            return true;
        }
    }
}