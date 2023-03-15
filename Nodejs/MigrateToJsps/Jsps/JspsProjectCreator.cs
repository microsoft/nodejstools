using System.IO;

namespace MigrateToJsps
{
    internal class JspsProjectCreator
    {
        private string njsprojDir;
        private NjsprojFileModel njsprojFileModel;
        private string newProjectParentDir;

        public JspsProjectCreator(string njsprojDir, NjsprojFileModel njsprojFileModel, string newProjectParentDir)
        {
            this.njsprojDir = njsprojDir;
            this.njsprojFileModel = njsprojFileModel;
            this.newProjectParentDir = newProjectParentDir;
        }

        public void CreateJspsProject() // to be called by outsiders
        {
            var parentDir = Path.Combine(newProjectParentDir, njsprojFileModel.ProjectName);
            var newProjDir = Path.Combine(parentDir, njsprojFileModel.ProjectName);

            Directory.CreateDirectory(newProjDir);

            // TODO: account for none includes also!
            CopyProjectFiles(newProjDir);

            LaunchJsonWriter.CreateLaunchJson(njsprojFileModel.ProjectTypeGuids, newProjDir, njsprojFileModel.StartupFile);

            NugetConfigWriter.GenerateNugetConfig(newProjDir);

            EsprojFileWriter.WriteEsproj(newProjDir, njsprojFileModel.ProjectName);
        }

        private void CopyProjectFiles(string destinationDir)
        {
            foreach (var fileItem in njsprojFileModel.ProjectFiles)
            {
                var source = Path.Combine(njsprojDir, fileItem);
                var destination = Path.Combine(destinationDir, fileItem);

                var fileDir = Path.GetDirectoryName(destination);
                if (!string.IsNullOrEmpty(fileDir))
                {
                    Directory.CreateDirectory(fileDir);
                }

                File.Copy(source, destination, true);
            }
        }
    }
}
