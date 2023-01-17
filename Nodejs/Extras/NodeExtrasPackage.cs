using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Jade;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using Microsoft.NodejsTools;
using Microsoft.VisualStudioTools;
using System.Runtime.InteropServices;
using Microsoft.NodejsTools.Repl;
using Microsoft.VisualStudio.ComponentModelHost;
using System.IO;
using System.Diagnostics;
using Microsoft.NodejsTools.Commands;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using System.Windows;
using System.Threading;

namespace Extras
{
    [PackageRegistration()]
    [Guid(Microsoft.NodejsTools.Guids.NodeExtrasPackageString)]
    [InstalledProductRegistration("Node.js Extras", "", "Node.Js Extras")]
    [ProvideMenuResource("Menus.ctmenu", 1)]                              // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideLanguageService(typeof(JadeLanguageInfo), JadeContentTypeDefinition.JadeLanguageName, 3041, RequestStockColors = true, ShowSmartIndent = false, ShowCompletion = false, DefaultToInsertSpaces = true, HideAdvancedMembersByDefault = false, EnableAdvancedMembersOption = false, ShowDropDownOptions = false)]
    [ProvideEditorExtension(typeof(JadeEditorFactory), JadeContentTypeDefinition.JadeFileExtension, 50)]
    [ProvideEditorExtension(typeof(JadeEditorFactory), JadeContentTypeDefinition.PugFileExtension, 50)]
    [ProvideEditorLogicalView(typeof(JadeEditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    [ProvideLanguageExtension(typeof(JadeEditorFactory), JadeContentTypeDefinition.JadeFileExtension)]
    [ProvideLanguageExtension(typeof(JadeEditorFactory), JadeContentTypeDefinition.PugFileExtension)]
    [ProvideTextEditorAutomation(JadeContentTypeDefinition.JadeLanguageName, 3041, 3045, ProfileMigrationType.PassThrough)]
    [ProvideInteractiveWindow(Microsoft.NodejsTools.Guids.NodejsInteractiveWindowString, Style = VsDockStyle.Linked, Orientation = ToolWindowOrientation.none, Window = ToolWindowGuids80.Outputwindow)]
    internal class NodeExtrasPackage : Package
    {
        internal static readonly string ProductName = "NodeJsExtras";
        internal static NodeExtrasPackage Instance;

        public NodeExtrasPackage()
        {
            Instance = this;
        }

        protected override void Initialize()
        {
            base.Initialize();

            RegisterEditorFactory(new JadeEditorFactory(this));

            var commands = new List<Command> {
                new OpenReplWindowCommand(),
            };

            RegisterCommands(commands, Microsoft.NodejsTools.Guids.NodejsCmdSet);
        }

        internal void RegisterCommands(IEnumerable<Command> commands, Guid cmdSet)
        {
            if (GetService(typeof(IMenuCommandService)) is IMenuCommandService mcs)
            {
                foreach (var command in commands)
                {
                    var beforeQueryStatus = command.BeforeQueryStatus;
                    var toolwndCommandID = new CommandID(cmdSet, command.CommandId);
                    var menuToolWin = new OleMenuCommand(command.DoCommand, toolwndCommandID);
                    if (beforeQueryStatus != null)
                    {
                        menuToolWin.BeforeQueryStatus += beforeQueryStatus;
                    }
                    mcs.AddCommand(menuToolWin);
                }
            }
        }

        public virtual int FDoIdle(uint grfidlef)
        {
            var onIdle = OnIdle;
            if (onIdle != null)
            {
                onIdle(this, EventArgs.Empty);
            }

            return 0;
        }

        internal event EventHandler<EventArgs> OnIdle;

        protected override int CreateToolWindow(ref Guid toolWindowType, int id)
        {
            if (toolWindowType == Microsoft.NodejsTools.Guids.NodejsInteractiveWindow)
            {
                var replProvider = this.GetInteractiveWindowProvider();

                replProvider.OpenOrCreateWindow(id);
                return VSConstants.S_OK;
            }

            return base.CreateToolWindow(ref toolWindowType, id);
        }

        internal void OpenReplWindow(bool focus = true)
        {
            var replProvider = this.GetInteractiveWindowProvider();

            replProvider.OpenOrCreateWindow(-1).Show(focus);
        }

        private InteractiveWindowProvider GetInteractiveWindowProvider()
        {
            var model = (IComponentModel)GetService(typeof(SComponentModel));
            return model.GetService<InteractiveWindowProvider>();
        }
    }
}
