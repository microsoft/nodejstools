using EnvDTE;
using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudioTools;
using Command = Microsoft.VisualStudioTools.Command;
using Microsoft.NodejsTools.Project;
using Microsoft.NodejsTools.Telemetry;
using MigrateToJsps;
using System.IO;
using System.Linq;
using System.Collections;
using Microsoft.Build.Utilities;
using System.Runtime.Remoting.Channels;
using Microsoft.Internal.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Commands
{
    internal class MigrateToJspsCommand : Command
    {
        public override int CommandId => (int)PkgCmdId.cmdidJspsProjectMigrate;

        public override async void DoCommand(object sender, EventArgs args)
        {
            EnvDTE.Project project = MigrateToJspsUtils.GetActiveProject();

            string projectFilepath = project.FullName;

            var nodeProject = (NodejsProjectNode)project.Object;
            string projectFolder = nodeProject.ProjectFolder;

            var projectGuid = nodeProject.ProjectGuid;
            TelemetryHelper.LogUserMigratedToJsps();

            project.Save();
            NodejsPackage.Instance.DTE.Solution.Remove(project);

            JoinableTask<string> newProjectMigration = ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                return MigrationLibrary.Migrate(projectFilepath, projectFolder);
            });

            var newProjectFilepath = await newProjectMigration;

            if (newProjectFilepath != null)
            {
                NodejsPackage.Instance.DTE.Solution.AddFromFile(newProjectFilepath, false);

                if (!NodejsPackage.Instance.DTE.Solution.Saved)
                {
                    var solutionFile = NodejsPackage.Instance.DTE.Solution.FullName;
                    NodejsPackage.Instance.DTE.Solution.SaveAs(solutionFile);

                    string logfile = Path.Combine(projectFolder, "PROJECT_MIGRATION_LOG.txt");
                    NodejsPackage.Instance.DTE.ItemOperations.OpenFile(logfile);
                }
            }
            else
            {
                // put old projectfile back?
            }
        }

        public override EventHandler BeforeQueryStatus
        {
            get { 
                return new EventHandler((sender, args) => {

                    var cmd = sender as OleMenuCommand;
                    if (cmd != null)
                    {
                        cmd.Visible = cmd.Enabled = false;
                    }

                    try
                    {
                        EnvDTE.Project activeProject = MigrateToJspsUtils.GetActiveProject();

                        if (MigrateToJspsUtils.MigrationIsEnabled() && MigrateToJspsUtils.ProjectFileIsNtvs(activeProject.FullName))
                        {
                            cmd.Visible = cmd.Enabled = true;

                            if (MigrateToJspsUtils.IsTypeScriptProject(activeProject))
                            {
                                cmd.Text = "Convert to New TypeScript Project Experience";
                            }
                            else
                            {
                                cmd.Text = "Convert to New JavaScript Project Experience";
                            }
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
