using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Project
{
    internal static class MigrateToJspsInfoBar
    {
        private static IVsInfoBarUIElement CurrentInfoBarElement;
        private const string UserClosedMigrationPrompt = "UserClosedMigrationPrompt";

        public static void Show(IVsShell vsShell, IVsInfoBarUIFactory infoBarUiFactory, IVsSolutionPersistence solutionPersistence, string projectFileLocation)
        {
            if (ErrorHandler.Failed(vsShell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var tmp)))
            {
                return;
            }
            var infoBarHost = (IVsInfoBarHost)tmp;

            var userPreference = new PersistUserPreferences();
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
            Action OnClose = () =>
            {
                userPreference.dismissedMigration = true;
                solutionPersistence.SavePackageUserOpts(userPreference, UserClosedMigrationPrompt);
            };
            CurrentInfoBarElement = infoBarUiFactory.CreateInfoBar(infoBarModel);
            CurrentInfoBarElement.Advise(new InfoBarUIEvents(OnClose), out eventCookie);

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

        /// <summary>
        /// This class is used to persist the user preferences for the migration prompt using the .suo file.
        /// </summary>
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
                // Register the section for saving user options
                return pPersistence.SavePackageUserOpts(this, UserClosedMigrationPrompt);
            }

            // This method reads the boolean setting from the .suo file
            public int ReadUserOptions(IStream pOptionsStream, string pszKey)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (pszKey == UserClosedMigrationPrompt)
                {
                    // Allocate a buffer to hold the data
                    byte[] buffer = new byte[1]; // Assuming the boolean is stored as 0 or 1
                    uint bytesRead = 0;

                    // Read the data from the IStream 
                    pOptionsStream.Read(buffer, (uint)buffer.Length, out bytesRead);

                    // Interpret the byte array as a boolean value
                    dismissedMigration = BitConverter.ToBoolean(buffer, 0);
                }

                return VSConstants.S_OK;
            }

            // This method writes the boolean value to the .suo file
            public int WriteUserOptions(IStream pOptionsStream, string pszKey)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (pszKey == UserClosedMigrationPrompt)
                {
                    uint outBytesWritten = 0;

                    // Convert the boolean to a byte array
                    byte[] buffer = BitConverter.GetBytes(dismissedMigration);

                    // Write the byte array to the IStream
                    pOptionsStream.Write(buffer, (uint)buffer.Length, out outBytesWritten);
                }

                return VSConstants.S_OK;
            }
        }

        private sealed class InfoBarUIEvents : IVsInfoBarUIEvents
        {
            private readonly Action OnClose;

            public InfoBarUIEvents(Action onClose)
            {
                this.OnClose = onClose;
            }

            public void OnClosed(IVsInfoBarUIElement infoBarUIElement) => this.OnClose();

            public void OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem) => (actionItem.ActionContext as Action)?.Invoke();
        }
    }
}
