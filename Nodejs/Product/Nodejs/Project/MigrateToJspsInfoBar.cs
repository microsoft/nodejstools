using System;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.NodejsTools.Commands;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Project
{
    internal static class MigrateToJspsInfoBar
    {
        private static IVsInfoBarUIElement CurrentInfoBarElement;

        private const string UserClosedMigrationPrompt = "PromptedToMigrateToJsps";

        private static string GetUserFilePath(string projectFilepath)
        {
            return projectFilepath + ".user";
        }

        private static XDocument GetXDocumentForUserFile(string userFile)
        {
            if (File.Exists(userFile))
            {
                try
                {
                    return XDocument.Load(userFile);
                }
                catch
                {
                    // do nothing, just return null
                }
            }

            return null;
        }

        private static void WritePropertyToUserFile(string userFilePath)
        {
            try
            {
                var userProperties = GetXDocumentForUserFile(userFilePath);
                var ns = userProperties.Root.Name.Namespace; 

                var newPropertyGroup = new XElement(ns + "PropertyGroup");
                var newProperty = new XElement(ns + UserClosedMigrationPrompt);

                newPropertyGroup.Add(newProperty);

                userProperties.Root.Add(newPropertyGroup);

                userProperties.Save(userFilePath);
            }
            catch
            {
                // it's fine, if something goes wrong, we just won't write it to the userfile
            }
        }

        public static void Show(IVsShell vsShell, IVsInfoBarUIFactory infoBarUiFactory, string projectFileLocation)
        {
            if (ErrorHandler.Failed(vsShell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var tmp)))
            {
                return;
            }
            var infoBarHost = (IVsInfoBarHost)tmp;

            CurrentInfoBarElement?.Close();

            var userFile = GetUserFilePath(projectFileLocation);
            var userProperties = GetXDocumentForUserFile(userFile);

            if (userProperties != null)
            {
                var userClosedPrompt = userProperties.Descendants().Any(prop => prop.Name.ToString().Contains(UserClosedMigrationPrompt));

                if (userClosedPrompt)
                {
                    return;
                }
            }

            Action migrateProjectCmd = MigrateProjectCommand;

            var actionItems = new[]
            {
                new InfoBarHyperlink(Resources.MigrateToJspsClickHere, migrateProjectCmd)
            };
            var infoBarModel = new InfoBarModel(Resources.MigrateToJspsPrompt, actionItems);

            uint eventCookie = 2;
            Action<string> OnClose = (filePath) =>
            {
                CurrentInfoBarElement.Unadvise(eventCookie);
                WritePropertyToUserFile(filePath);
            };
            CurrentInfoBarElement = infoBarUiFactory.CreateInfoBar(infoBarModel);
            CurrentInfoBarElement.Advise(new InfoBarUIEvents(OnClose, userFile), out eventCookie);

            infoBarHost.AddInfoBar(CurrentInfoBarElement);

            void MigrateProjectCommand()
            {
                try
                {
                    NodejsPackage.Instance.DTE.Commands.Raise(Guids.MigrateToJspsCmdSet.ToString(), PkgCmdId.cmdidJspsProjectMigrate, null, null);
                }
                catch
                {
                    // do nothing if there is an error in raising the command (like if the project is not loaded)
                }
            }
        }

        private sealed class InfoBarUIEvents : IVsInfoBarUIEvents
        {
            private readonly Action<string> OnClose;

            private string UserFile;

            public InfoBarUIEvents(Action<string> onClose, string userFile)
            {
                this.OnClose = onClose;
                this.UserFile = userFile;
            }

            public void OnClosed(IVsInfoBarUIElement infoBarUIElement) => this.OnClose(UserFile);

            public void OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem) => (actionItem.ActionContext as Action)?.Invoke();
        }
    }
}
