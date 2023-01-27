using System.IO;

namespace MigrateToJsps
{
    internal class JspsProjectCreator
    {
        private string projectDir;
        private NjsprojFileModel njsprojFileModel;
        private Logger logger;

        public JspsProjectCreator(string projectDir, NjsprojFileModel njsprojFileModel, Logger logger)
        {
            this.projectDir = projectDir;
            this.njsprojFileModel = njsprojFileModel;

            this.logger = logger;
        }

        public string CreateJspsProject() // to be called by outsiders
        {
            LaunchJsonWriter.CreateLaunchJson(projectDir, njsprojFileModel, this.logger);

            NugetConfigWriter.GenerateNugetConfig(projectDir, this.logger);

            var port = njsprojFileModel.NodejsPort;

            return EsprojFileWriter.WriteEsproj(projectDir, njsprojFileModel.ProjectName, njsprojFileModel.StartupFile, port, this.logger);
        }
    }
}
