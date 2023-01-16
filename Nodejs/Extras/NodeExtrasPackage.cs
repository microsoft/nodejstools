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

namespace Extras
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(Guids.NodeExtrasPackageString)]
    [ProvideLanguageService(typeof(JadeLanguageInfo), JadeContentTypeDefinition.JadeLanguageName, 3041, RequestStockColors = true, ShowSmartIndent = false, ShowCompletion = false, DefaultToInsertSpaces = true, HideAdvancedMembersByDefault = false, EnableAdvancedMembersOption = false, ShowDropDownOptions = false)]
    [ProvideEditorExtension2(typeof(JadeEditorFactory), JadeContentTypeDefinition.JadeFileExtension, 50, __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, "*:1", ProjectGuid = VSConstants.CLSID.MiscellaneousFilesProject_string, NameResourceID = 3041, EditorNameResourceId = 3045)]
    [ProvideEditorExtension2(typeof(JadeEditorFactory), JadeContentTypeDefinition.PugFileExtension, 50, __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, "*:1", ProjectGuid = VSConstants.CLSID.MiscellaneousFilesProject_string, NameResourceID = 3041, EditorNameResourceId = 3045)]
    [ProvideEditorLogicalView(typeof(JadeEditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    [ProvideLanguageExtension(typeof(JadeEditorFactory), JadeContentTypeDefinition.JadeFileExtension)]
    [ProvideLanguageExtension(typeof(JadeEditorFactory), JadeContentTypeDefinition.PugFileExtension)]
    [ProvideTextEditorAutomation(JadeContentTypeDefinition.JadeLanguageName, 3041, 3045, ProfileMigrationType.PassThrough)]

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

    }
}
