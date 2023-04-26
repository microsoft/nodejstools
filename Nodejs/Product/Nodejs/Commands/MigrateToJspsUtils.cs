using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Internal.VisualStudio.Shell.Interop;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.NodejsTools.Commands
{
    internal static class MigrateToJspsUtils
    {
        internal static bool IsTypeScriptProject(EnvDTE.Project project)
        {
            var nodeProject = (NodejsProjectNode)project.Object;
            return nodeProject.IsTypeScriptProject;
        }

        internal static bool ProjectFileIsNtvs(string filepath)
        {
            string fileExtension = Path.GetExtension(filepath);
            return (!string.IsNullOrEmpty(fileExtension)) && (fileExtension == NodejsConstants.NodejsProjectExtension);
        }

        internal static bool ProjectFileIsJsps(string filepath)
        {
            string fileExtension = Path.GetExtension(filepath);
            return (!string.IsNullOrEmpty(fileExtension)) && (fileExtension == ".esproj");
        }

        internal static bool DirectoryContainsNjsproj(string directory)
        {
            return Directory.GetFiles(directory).Where(file => file.EndsWith(".njsproj")).Any();
        }

        internal static EnvDTE.Project GetActiveProject()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Array activeProjects = (Array)NodejsPackage.Instance.DTE.ActiveSolutionProjects;
            EnvDTE.Project project = (EnvDTE.Project)activeProjects.GetValue(0);

            return project;
        }

        internal static bool MigrationIsEnabled()
        {
            IVsFeatureFlags featureFlags = ServiceProvider.GlobalProvider.GetService(typeof(SVsFeatureFlags)) as IVsFeatureFlags;

            return featureFlags.IsFeatureEnabled("JavaScript.NodejsTools.EnableNtvsJspsMigration", defaultValue: false);
        }
    }
}
