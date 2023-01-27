using System;
using Microsoft.NodejsTools.Project;
using Microsoft.NodejsTools.Telemetry;
using System.IO;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudio.Shell;
using System.Linq;

namespace Microsoft.NodejsTools.Commands
{
    internal class RevertMigrationCommand : Command
    {
        public override int CommandId => (int)PkgCmdId.cmdidJspsProjectRevert;

        public override void DoCommand(object sender, EventArgs args)
        {
            EnvDTE.Project project = MigrateToJspsUtils.GetActiveProject();

            string projectFilepath = project.FullName;
            string projectFolder = Path.GetDirectoryName(projectFilepath); 

            string njsprojFile = Directory.GetFiles(projectFolder).Where(file => file.EndsWith(".njsproj")).FirstOrDefault();

            if (njsprojFile == null)
            {
                // log error, show user message
            }
            else
            {
                project.Save();
                NodejsPackage.Instance.DTE.Solution.Remove(project);
                NodejsPackage.Instance.DTE.Solution.AddFromFile(njsprojFile, false);

                if (!NodejsPackage.Instance.DTE.Solution.Saved)
                {
                    var solutionFile = NodejsPackage.Instance.DTE.Solution.FullName;
                    NodejsPackage.Instance.DTE.Solution.SaveAs(solutionFile);
                }

                EnvDTE.Project oldNtvsProject = MigrateToJspsUtils.GetActiveProject();
                var projectGuid = oldNtvsProject.GetNodejsProject().ProjectGuid;

                TelemetryHelper.LogUserRevertedBackToNtvs();
            }
        }

        public override EventHandler BeforeQueryStatus
        {
            get
            {
                return new EventHandler((sender, args) => {

                    var cmd = sender as OleMenuCommand;
                    if (cmd != null)
                    {
                        cmd.Visible = cmd.Enabled = false;
                    }

                    try
                    {
                        EnvDTE.Project activeProject = MigrateToJspsUtils.GetActiveProject();

                        string projectDir = Path.GetDirectoryName(activeProject.FullName);

                        if (MigrateToJspsUtils.MigrationIsEnabled() && MigrateToJspsUtils.ProjectFileIsJsps(activeProject.FullName) && MigrateToJspsUtils.DirectoryContainsNjsproj(projectDir))
                        {
                            cmd.Visible = cmd.Enabled = true;
                        }
                    }
                    catch (Exception e)
                    {

                    }
                });
            }
        }

    }
}
