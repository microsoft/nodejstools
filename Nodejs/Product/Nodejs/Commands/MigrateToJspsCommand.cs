using EnvDTE;
using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudioTools;
using Command = Microsoft.VisualStudioTools.Command;
using Microsoft.NodejsTools.Project;
using MigrateToJsps;
using System.IO;
using System.Linq;
using System.Collections;

namespace Microsoft.NodejsTools.Commands
{
    internal class MigrateToJspsCommand : Command
    {
        public override int CommandId => (int)PkgCmdId.cmdidJspsProjectMigrate;

        public override void DoCommand(object sender, EventArgs args)
        {
            Array activeProjects = (Array)NodejsPackage.Instance.DTE.ActiveSolutionProjects;
            EnvDTE.Project project = (EnvDTE.Project)activeProjects.GetValue(0);

            string projectFilepath = project.FullName;

            var nodeProject = (NodejsProjectNode) project.Object;
            string parentProjectDir = Path.GetDirectoryName(nodeProject.ProjectFolder);

            ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                MigrationLibrary.Migrate(projectFilepath, parentProjectDir);
            });
        }
    }
}
