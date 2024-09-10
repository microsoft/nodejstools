using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;

namespace Microsoft.NodejsTools.Project
{
    internal abstract class MigrateToJspsInfoBar
    {
        private static IVsInfoBarUIElement CurrentInfoBarElement;
        private const string UserClosedMigrationPrompt = "UserClosedMigrationPrompt";

        public static void Show(IVsShell vsShell, IVsInfoBarUIFactory infoBarUiFactory, IVsSolutionPersistence solutionPersistence, string projectFileLocation)
        {
            var userPreference = new PersistUserPreferences();
            if (ErrorHandler.Failed(vsShell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var tmp)))
            {
                return;
            }
            var infoBarHost = (IVsInfoBarHost)tmp;

            if (solutionPersistence != null)
            {
                // Load the section from the .suo file
                solutionPersistence.LoadPackageUserOpts(userPreference, UserClosedMigrationPrompt);
            }

            if (userPreference.dismissedMigration)
            {
                return;
            }

            Action migrateProjectCmd = MigrateProjectCommand;

            var actionItems = new[]
            {
                new InfoBarHyperlink(MigrateToJspsResources.MigrateToJspsClickHere, migrateProjectCmd)
            };
            var infoBarModel = new InfoBarModel(MigrateToJspsResources.MigrateToJspsPrompt, actionItems);

            uint eventCookie = 2;
            Action<string> OnClose = (userFile) =>
            {
                userPreference.dismissedMigration = true;
                solutionPersistence.SavePackageUserOpts(userPreference, UserClosedMigrationPrompt);
            };
            CurrentInfoBarElement = infoBarUiFactory.CreateInfoBar(infoBarModel);
            CurrentInfoBarElement.Advise(new InfoBarUIEvents(OnClose, ""), out eventCookie);

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

        private sealed class PersistUserPreferences : IVsPersistSolutionOpts
        {
            public bool dismissedMigration = false;

            public int LoadUserOptions(IVsSolutionPersistence pPersistence, uint grfLoadOpts)
            {
                // Register the section to load user options
                return pPersistence.LoadPackageUserOpts(this, UserClosedMigrationPrompt);
            }

            public int SaveUserOptions(IVsSolutionPersistence pPersistence)
            {
                // Register the section for saving
                
                return pPersistence.SavePackageUserOpts(this, UserClosedMigrationPrompt);
            }

            // This method reads the boolean setting from the .suo file
            public int ReadUserOptions(IStream pOptionsStream, string pszKey)
            {
                if (pszKey == UserClosedMigrationPrompt)
                {
                    // Allocate a buffer to hold the data
                    byte[] buffer = new byte[256]; // Assuming the boolean is stored as a small string
                    uint bytesRead = 0;

                    // Read the data from the IStream (use ref for the bytesRead count)
                    pOptionsStream.Read(buffer, (uint)buffer.Length, out bytesRead);

                    // Convert the byte array to a string (only up to the bytes that were read)
                    string booleanValue = System.Text.Encoding.UTF8.GetString(buffer, 0, (int)bytesRead);

                    // Parse the string into a boolean value
                    bool.TryParse(booleanValue, out dismissedMigration);  // Store the boolean value
                }

                return VSConstants.S_OK;
            }

            // This method writes the boolean value to the .suo file
            public int WriteUserOptions(IStream pOptionsStream, string pszKey)
            {
                if (pszKey == UserClosedMigrationPrompt)
                {
                    uint outBytesWritten = 0;

                    // Convert the boolean to string and then to a byte array
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(dismissedMigration.ToString());

                    // Write the byte array to the IStream
                    pOptionsStream.Write(buffer, (uint)buffer.Length, out outBytesWritten);
                }

                return VSConstants.S_OK;
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
