//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.NodejsTools.Commands;
using Microsoft.NodejsTools.Debugger.DataTips;
using Microsoft.NodejsTools.Debugger.DebugEngine;
using Microsoft.NodejsTools.Debugger.Remote;
using Microsoft.NodejsTools.Intellisense;
using Microsoft.NodejsTools.Jade;
using Microsoft.NodejsTools.Logging;
using Microsoft.NodejsTools.Options;
using Microsoft.NodejsTools.Project;
using Microsoft.NodejsTools.ProjectWizard;
using Microsoft.NodejsTools.Repl;
using Microsoft.NodejsTools.Telemetry;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools {
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", AssemblyVersionInfo.Version, IconResourceID = 400)]
    [Guid(Guids.NodejsPackageString)]
    [ProvideOptionPage(typeof(NodejsGeneralOptionsPage), "Node.js Tools", "General", 114, 115, true)]
    [ProvideOptionPage(typeof(NodejsNpmOptionsPage), "Node.js Tools", "Npm", 114, 116, true)]
    [ProvideDebugEngine("Node.js Debugging", typeof(AD7ProgramProvider), typeof(AD7Engine), AD7Engine.DebugEngineId, setNextStatement: false, hitCountBp: true, justMyCodeStepping: false)]
    [ProvideLanguageService(typeof(NodejsLanguageInfo), NodejsConstants.Nodejs, 106, RequestStockColors = true, ShowSmartIndent = true, ShowCompletion = true, DefaultToInsertSpaces = true, HideAdvancedMembersByDefault = true, EnableAdvancedMembersOption = true, ShowDropDownOptions = true)]
    [ProvideDebugLanguage(NodejsConstants.Nodejs, Guids.NodejsDebugLanguageString, NodeExpressionEvaluatorGuid, AD7Engine.DebugEngineId)]
    [WebSiteProject("JavaScript", "JavaScript")]
    [ProvideProjectFactory(typeof(NodejsProjectFactory), null, null, null, null, ".\\NullPath", LanguageVsTemplate = NodejsConstants.JavaScript, SortPriority=0x17)]   // outer flavor, no file extension
    [ProvideDebugPortSupplier("Node remote debugging", typeof(NodeRemoteDebugPortSupplier), NodeRemoteDebugPortSupplier.PortSupplierId)]
    [ProvideMenuResource(1000, 1)]                              // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideBraceCompletion(NodejsConstants.Nodejs)]
    [ProvideProjectItem(typeof(BaseNodeProjectFactory), NodejsConstants.Nodejs, "FileTemplates\\NewItem", 0)]
    [ProvideLanguageTemplates("{349C5851-65DF-11DA-9384-00065B846F21}", NodejsConstants.JavaScript, Guids.NodejsPackageString, "Web", "Node.js Project Templates", "{" + Guids.NodejsBaseProjectFactoryString + "}", ".js", NodejsConstants.Nodejs, "{" + Guids.NodejsBaseProjectFactoryString + "}")]
    [ProvideTextEditorAutomation(NodejsConstants.Nodejs, 106, 102, ProfileMigrationType.PassThrough)]
    [ProvideLanguageService(typeof(JadeLanguageInfo), JadeContentTypeDefinition.JadeLanguageName, 3041, RequestStockColors = true, ShowSmartIndent = false, ShowCompletion = false, DefaultToInsertSpaces = true, HideAdvancedMembersByDefault = false, EnableAdvancedMembersOption = false, ShowDropDownOptions = false)]
    [ProvideEditorExtension2(typeof(JadeEditorFactory), JadeContentTypeDefinition.JadeFileExtension, 50, __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, "*:1", ProjectGuid = VSConstants.CLSID.MiscellaneousFilesProject_string, NameResourceID = 3041, EditorNameResourceId = 3045)]
    [ProvideEditorExtension2(typeof(JadeEditorFactory), JadeContentTypeDefinition.PugFileExtension, 50, __VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, "*:1", ProjectGuid = VSConstants.CLSID.MiscellaneousFilesProject_string, NameResourceID = 3041, EditorNameResourceId = 3045)]
    [ProvideEditorLogicalView(typeof(JadeEditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    [ProvideLanguageExtension(typeof(JadeEditorFactory), JadeContentTypeDefinition.JadeFileExtension)]
    [ProvideLanguageExtension(typeof(JadeEditorFactory), JadeContentTypeDefinition.PugFileExtension)]
    [ProvideTextEditorAutomation(JadeContentTypeDefinition.JadeLanguageName, 3041, 3045, ProfileMigrationType.PassThrough)]
    [ProvideLanguageEditorOptionPage(typeof(NodejsFormattingSpacingOptionsPage), NodejsConstants.Nodejs, "Formatting", "Spacing", "3042")]
    [ProvideLanguageEditorOptionPage(typeof(NodejsFormattingBracesOptionsPage), NodejsConstants.Nodejs, "Formatting", "Braces", "3043")]
    [ProvideLanguageEditorOptionPage(typeof(NodejsFormattingGeneralOptionsPage), NodejsConstants.Nodejs, "Formatting", "General", "3044")]
#if DEV14
    [ProvideLanguageEditorOptionPage(typeof(NodejsIntellisenseOptionsPage), NodejsConstants.Nodejs, "IntelliSense", "", "3048")]
#endif
    [ProvideLanguageEditorOptionPage(typeof(NodejsAdvancedEditorOptionsPage), NodejsConstants.Nodejs, "Advanced", "", "3050")]
    internal sealed partial class NodejsPackage : CommonPackage {
        internal const string NodeExpressionEvaluatorGuid = "{F16F2A71-1C45-4BAB-BECE-09D28CFDE3E6}";
        private IContentType _contentType;
        internal const string NodeJsFileType = ".njs";
        internal static NodejsPackage Instance;
        private string _surveyNewsUrl;
        private object _surveyNewsUrlLock = new object();
        internal HashSet<ITextBuffer> ChangedBuffers = new HashSet<ITextBuffer>();
        private LanguagePreferences _langPrefs;
        internal VsProjectAnalyzer _analyzer;
        private NodejsToolsLogger _logger;
        private ITelemetryLogger _telemetryLogger;
        // Hold references for the subscribed events. Otherwise the callbacks will be garbage collected
        // after the initialization
        private List<EnvDTE.CommandEvents> _subscribedCommandEvents = new List<EnvDTE.CommandEvents>();

        private static readonly Version _minRequiredTypescriptVersion = new Version("1.8");

        private readonly Lazy<bool> _hasRequiredTypescriptVersion = new Lazy<bool>(() => {
            Version version;
            var versionString = GetTypeScriptToolsVersion();
            return !string.IsNullOrEmpty(versionString)
                && Version.TryParse(versionString, out version)
                && version.CompareTo(_minRequiredTypescriptVersion) > -1;
        });

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public NodejsPackage() {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
            Debug.Assert(Instance == null, "NodejsPackage created multiple times");
            Instance = this;
        }

        public NodejsGeneralOptionsPage GeneralOptionsPage {
            get {
                return (NodejsGeneralOptionsPage)GetDialogPage(typeof(NodejsGeneralOptionsPage));
            }
        }

        public NodejsNpmOptionsPage NpmOptionsPage {
            get {
                return (NodejsNpmOptionsPage)GetDialogPage(typeof(NodejsNpmOptionsPage));
            }
        }

        public NodejsFormattingSpacingOptionsPage FormattingSpacingOptionsPage {
            get {
                return (NodejsFormattingSpacingOptionsPage)GetDialogPage(typeof(NodejsFormattingSpacingOptionsPage));
            }
        }

        public NodejsFormattingBracesOptionsPage FormattingBracesOptionsPage {
            get {
                return (NodejsFormattingBracesOptionsPage)GetDialogPage(typeof(NodejsFormattingBracesOptionsPage));
            }
        }

        public NodejsFormattingGeneralOptionsPage FormattingGeneralOptionsPage {
            get {
                return (NodejsFormattingGeneralOptionsPage)GetDialogPage(typeof(NodejsFormattingGeneralOptionsPage));
            }
        }

        public NodejsIntellisenseOptionsPage IntellisenseOptionsPage {
            get {
                return (NodejsIntellisenseOptionsPage)GetDialogPage(typeof(NodejsIntellisenseOptionsPage));
            }
        }

        public NodejsAdvancedEditorOptionsPage AdvancedEditorOptionsPage {
            get {
                return (NodejsAdvancedEditorOptionsPage)GetDialogPage(typeof(NodejsAdvancedEditorOptionsPage));
            }
        }

        public NodejsDiagnosticsOptionsPage DiagnosticsOptionsPage {
            get {
                return (NodejsDiagnosticsOptionsPage)GetDialogPage(typeof(NodejsDiagnosticsOptionsPage));
            }
        }

        public EnvDTE.DTE DTE {
            get {
                return (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize() {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            if (!_hasRequiredTypescriptVersion.Value) {
                MessageBox.Show(
                   Project.SR.GetString(Project.SR.TypeScriptMinVersionNotInstalled, _minRequiredTypescriptVersion.ToString()),
                   Project.SR.ProductName,
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Error);
            }

            SubscribeToVsCommandEvents(
                (int)VSConstants.VSStd97CmdID.AddNewProject,
                delegate { NewProjectFromExistingWizard.IsAddNewProjectCmd = true; },
                delegate { NewProjectFromExistingWizard.IsAddNewProjectCmd = false; }
            );

            var langService = new NodejsLanguageInfo(this);
            ((IServiceContainer)this).AddService(langService.GetType(), langService, true);

            ((IServiceContainer)this).AddService(typeof(ClipboardServiceBase), new ClipboardService(), true);

            RegisterProjectFactory(new NodejsProjectFactory(this));
            RegisterEditorFactory(new JadeEditorFactory(this));

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var commands = new List<Command> {
                new OpenReplWindowCommand(),
                new OpenRemoteDebugProxyFolderCommand(),
                new OpenRemoteDebugDocumentationCommand(),
                new SurveyNewsCommand(),
                new ImportWizardCommand(),
                new DiagnosticsCommand(this),
                new SendFeedbackCommand(),
                new ShowDocumentationCommand()
            };
            try {
                commands.Add(new AzureExplorerAttachDebuggerCommand());
            } catch (NotSupportedException) {
            }
            RegisterCommands(commands, Guids.NodejsCmdSet);

            IVsTextManager4 textMgr = (IVsTextManager4)Instance.GetService(typeof(SVsTextManager));

            LANGPREFERENCES3[] langPrefs = GetNodejsLanguagePreferencesFromTypeScript(textMgr);
            _langPrefs = new LanguagePreferences(langPrefs[0]);

            var textManagerEvents2Guid = typeof(IVsTextManagerEvents4).GUID;
            IConnectionPoint textManagerEvents2ConnectionPoint;
            ((IConnectionPointContainer)textMgr).FindConnectionPoint(ref textManagerEvents2Guid, out textManagerEvents2ConnectionPoint);
            uint cookie;
            textManagerEvents2ConnectionPoint.Advise(_langPrefs, out cookie);

            var textManagerEventsGuid = typeof(IVsTextManagerEvents).GUID;
            IConnectionPoint textManagerEventsConnectionPoint;
            ((IConnectionPointContainer)textMgr).FindConnectionPoint(ref textManagerEventsGuid, out textManagerEventsConnectionPoint);
            textManagerEventsConnectionPoint.Advise(new DataTipTextManagerEvents(this), out cookie);

            MakeDebuggerContextAvailable();

            InitializeLogging();

            InitializeTelemetry();

            // The variable is inherited by child processes backing Test Explorer, and is used in
            // the NTVS test discoverer and test executor to connect back to VS.
            Environment.SetEnvironmentVariable(NodejsConstants.NodeToolsProcessIdEnvironmentVariable, Process.GetCurrentProcess().Id.ToString());
        }

        public static LANGPREFERENCES3[] GetNodejsLanguagePreferencesFromTypeScript(IVsTextManager4 textMgr) {
            var langPrefs = new LANGPREFERENCES3[1];
            langPrefs[0].guidLang = Guids.TypeScriptLanguageInfo;
            int hr = textMgr.GetUserPreferences4(null, langPrefs, null);
            if (ErrorHandler.Failed(hr)) {
                MessageBox.Show(Project.SR.GetString(Project.SR.CouldNotGetTypeScriptLanguagePreferences), Project.SR.ProductName);
                ErrorHandler.ThrowOnFailure(hr);
            }
            langPrefs[0].guidLang = typeof(NodejsLanguageInfo).GUID;
            textMgr.SetUserPreferences4(null, langPrefs, null);
            return langPrefs;
        }

        private void SubscribeToVsCommandEvents(
            int eventId, 
            EnvDTE._dispCommandEvents_BeforeExecuteEventHandler beforeExecute = null,
            EnvDTE._dispCommandEvents_AfterExecuteEventHandler afterExecute = null) {
            var commandEventGuid = typeof(VSConstants.VSStd97CmdID).GUID.ToString("B");
            var targetEvent = DTE.Events.CommandEvents[commandEventGuid, eventId];
            if (beforeExecute != null) {
                targetEvent.BeforeExecute += beforeExecute;
            }
            if (afterExecute != null) {
                targetEvent.AfterExecute += afterExecute;
            }
            _subscribedCommandEvents.Add(targetEvent);
        }

        private void InitializeLogging() {
            _logger = new NodejsToolsLogger(ComponentModel.GetExtensions<INodejsToolsLogger>().ToArray());

            // log interesting stats on startup
            _logger.LogEvent(NodejsToolsLogEvent.SurveyNewsFrequency, GeneralOptionsPage.SurveyNewsCheck);
            _logger.LogEvent(NodejsToolsLogEvent.AnalysisLevel, AnalysisLevel.Preview);
        }

        private void InitializeTelemetry() {
            var thisAssembly = typeof(NodejsPackage).Assembly;

            // Get telemetry logger
            _telemetryLogger = TelemetrySetup.Instance.GetLogger(thisAssembly);

            TelemetrySetup.Instance.LogPackageLoad(_telemetryLogger, Guid.Parse(Guids.NodejsPackageString), thisAssembly, Application.ProductVersion);
        }

        public new IComponentModel ComponentModel {
            get {
                return this.GetComponentModel();
            }
        }

        internal NodejsToolsLogger Logger {
            get {
                return _logger;
            }
        }

        internal ITelemetryLogger TelemetryLogger {
            get {
                return _telemetryLogger;
            }
        }

        /// <summary>
        /// Makes the debugger context available - this enables our debugger when we're installed into
        /// a SKU which doesn't support every installed debugger.
        /// </summary>
        private void MakeDebuggerContextAvailable() {
            var monitorSelection = (IVsMonitorSelection)GetService(typeof(SVsShellMonitorSelection));
            Guid debugEngineGuid = AD7Engine.DebugEngineGuid;
            uint contextCookie;
            if (ErrorHandler.Succeeded(monitorSelection.GetCmdUIContextCookie(ref debugEngineGuid, out contextCookie))) {
                ErrorHandler.ThrowOnFailure(monitorSelection.SetCmdUIContext(contextCookie, 1));
            }
        }

        internal IReplWindow2 OpenReplWindow(bool focus = true) {
            var compModel = ComponentModel;
            var provider = compModel.GetService<IReplWindowProvider>();

            var window = (IReplWindow2)provider.FindReplWindow(NodejsReplEvaluatorProvider.NodeReplId);
            if (window == null) {
                window = (IReplWindow2)provider.CreateReplWindow(
                    ReplContentType,
                    "Node.js Interactive Window",
                    typeof(NodejsLanguageInfo).GUID,
                    NodejsReplEvaluatorProvider.NodeReplId
                );
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)((ToolWindowPane)window).Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());

            if (focus) {
                window.Focus();
            }

            return window;
        }

        internal static bool TryGetStartupFileAndDirectory(System.IServiceProvider serviceProvider, out string fileName, out string directory) {
            var startupProject = GetStartupProject(serviceProvider);
            if (startupProject != null) {
                fileName = startupProject.GetStartupFile();
                directory = startupProject.GetWorkingDirectory();                
            } else {
                var textView = CommonPackage.GetActiveTextView(serviceProvider);
                if (textView == null) {
                    fileName = null;
                    directory = null;
                    return false;
                }
                fileName = textView.GetFilePath();
                directory = Path.GetDirectoryName(fileName);
            }
            return true;
        }


        internal LanguagePreferences LangPrefs {
            get {
                return _langPrefs;
            }
        }

        private IContentType ReplContentType {
            get {
                if (_contentType == null) {
                    _contentType = ComponentModel.GetService<IContentTypeRegistryService>().GetContentType(NodejsConstants.Nodejs);
                }
                return _contentType;
            }
        }

        #endregion

        internal override VisualStudioTools.Navigation.LibraryManager CreateLibraryManager(CommonPackage package) {
            return new NodejsLibraryManager(this);
        }

        public override Type GetLibraryManagerType() {
            return typeof(NodejsLibraryManager);
        }

        public override bool IsRecognizedFile(string filename) {
            var ext = Path.GetExtension(filename);

            return String.Equals(ext, NodejsConstants.JavaScriptExtension, StringComparison.OrdinalIgnoreCase);
        }

        internal new object GetService(Type serviceType) {
            return base.GetService(serviceType);
        }

        public string BrowseForDirectory(IntPtr owner, string initialDirectory = null) {
            IVsUIShell uiShell = GetService(typeof(SVsUIShell)) as IVsUIShell;
            if (null == uiShell) {
                using (var ofd = new FolderBrowserDialog()) {
                    ofd.RootFolder = Environment.SpecialFolder.Desktop;
                    ofd.ShowNewFolderButton = false;
                    DialogResult result;
                    if (owner == IntPtr.Zero) {
                        result = ofd.ShowDialog();
                    } else {
                        result = ofd.ShowDialog(NativeWindow.FromHandle(owner));
                    }
                    if (result == DialogResult.OK) {
                        return ofd.SelectedPath;
                    } else {
                        return null;
                    }
                }
            }

            if (owner == IntPtr.Zero) {
                ErrorHandler.ThrowOnFailure(uiShell.GetDialogOwnerHwnd(out owner));
            }

            VSBROWSEINFOW[] browseInfo = new VSBROWSEINFOW[1];
            browseInfo[0].lStructSize = (uint)Marshal.SizeOf(typeof(VSBROWSEINFOW));
            browseInfo[0].pwzInitialDir = initialDirectory;
            browseInfo[0].hwndOwner = owner;
            browseInfo[0].nMaxDirName = 260;
            IntPtr pDirName = IntPtr.Zero;
            try {
                browseInfo[0].pwzDirName = pDirName = Marshal.AllocCoTaskMem(520);
                int hr = uiShell.GetDirectoryViaBrowseDlg(browseInfo);
                if (hr == VSConstants.OLE_E_PROMPTSAVECANCELLED) {
                    return null;
                }
                ErrorHandler.ThrowOnFailure(hr);
                return Marshal.PtrToStringAuto(browseInfo[0].pwzDirName);
            } finally {
                if (pDirName != IntPtr.Zero) {
                    Marshal.FreeCoTaskMem(pDirName);
                }
            }
        }

        private void BrowseSurveyNewsOnIdle(object sender, ComponentManagerEventArgs e) {
            this.OnIdle -= BrowseSurveyNewsOnIdle;

            lock (_surveyNewsUrlLock) {
                if (!string.IsNullOrEmpty(_surveyNewsUrl)) {
                    OpenVsWebBrowser(this, _surveyNewsUrl);
                    _surveyNewsUrl = null;
                }
            }
        }

        internal void BrowseSurveyNews(string url) {
            lock (_surveyNewsUrlLock) {
                _surveyNewsUrl = url;
            }

            this.OnIdle += BrowseSurveyNewsOnIdle;
        }

        private void CheckSurveyNewsThread(Uri url, bool warnIfNoneAvailable) {
            // We can't use a simple WebRequest, because that doesn't have access
            // to the browser's session cookies.  Cookies are used to remember
            // which survey/news item the user has submitted/accepted.  The server 
            // checks the cookies and returns the survey/news urls that are 
            // currently available (availability is determined via the survey/news 
            // item start and end date).
            var th = new Thread(() => {
                var br = new WebBrowser();
                br.Tag = warnIfNoneAvailable;
                br.DocumentCompleted += OnSurveyNewsDocumentCompleted;
                br.Navigate(url);
                Application.Run();
            });
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }

        private void OnSurveyNewsDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
            var br = (WebBrowser)sender;
            var warnIfNoneAvailable = (bool)br.Tag;
            if (br.Url == e.Url) {
                List<string> available = null;

                string json = br.DocumentText;
                if (!string.IsNullOrEmpty(json)) {
                    int startIndex = json.IndexOf("<PRE>");
                    if (startIndex > 0) {
                        int endIndex = json.IndexOf("</PRE>", startIndex);
                        if (endIndex > 0) {
                            json = json.Substring(startIndex + 5, endIndex - startIndex - 5);

                            try {
                                // Example JSON data returned by the server:
                                //{
                                // "cannotvoteagain": [], 
                                // "notvoted": [
                                //  "http://ptvs.azurewebsites.net/news/141", 
                                //  "http://ptvs.azurewebsites.net/news/41", 
                                // ], 
                                // "canvoteagain": [
                                //  "http://ptvs.azurewebsites.net/news/51"
                                // ]
                                //}

                                // Description of each list:
                                // voted: cookie found
                                // notvoted: cookie not found
                                // canvoteagain: cookie found, but multiple votes are allowed
                                JavaScriptSerializer serializer = new JavaScriptSerializer();
                                var results = serializer.Deserialize<Dictionary<string, List<string>>>(json);
                                available = results["notvoted"];
                            } catch (ArgumentException) {
                            } catch (InvalidOperationException) {
                            }
                        }
                    }
                }

                if (available != null && available.Count > 0) {
                    BrowseSurveyNews(available[0]);
                } else if (warnIfNoneAvailable) {
                    if (available != null) {
                        BrowseSurveyNews(GeneralOptionsPage.SurveyNewsIndexUrl);
                    } else {
                        BrowseSurveyNews(NodejsToolsInstallPath.GetFile("NoSurveyNewsFeed.html"));
                    }
                }

                Application.ExitThread();
            }
        }

        internal void CheckSurveyNews(bool forceCheckAndWarnIfNoneAvailable) {
            if (forceCheckAndWarnIfNoneAvailable || ShouldQuerySurveryNewsServer()) {
                var options = GeneralOptionsPage;
                options.SurveyNewsLastCheck = DateTime.Now;
                options.SaveSettingsToStorage();
                CheckSurveyNewsThread(new Uri(options.SurveyNewsFeedUrl), forceCheckAndWarnIfNoneAvailable);
            }
        }

        private bool ShouldQuerySurveryNewsServer() {
            var options = GeneralOptionsPage;
            // Ensure that we don't prompt the user on their very first project creation.
            // Delay by 3 days by pretending we checked 4 days ago (the default of check
            // once a week ensures we'll check again in 3 days).
            if (options.SurveyNewsLastCheck == DateTime.MinValue) {
                options.SurveyNewsLastCheck = DateTime.Now - TimeSpan.FromDays(4);
                options.SaveSettingsToStorage();
            }

            var elapsedTime = DateTime.Now - options.SurveyNewsLastCheck;
            switch (options.SurveyNewsCheck) {
                case SurveyNewsPolicy.Disabled:
                    return false;
                case SurveyNewsPolicy.CheckOnceDay:
                    return elapsedTime.TotalDays >= 1;
                case SurveyNewsPolicy.CheckOnceWeek:
                    return elapsedTime.TotalDays >= 7;
                case SurveyNewsPolicy.CheckOnceMonth:
                    return elapsedTime.TotalDays >= 30;
                default:
                    Debug.Assert(false, String.Format("Unexpected SurveyNewsPolicy: {0}.", options.SurveyNewsCheck));
                    return false;
            }
        }

        internal static void NavigateTo(string filename, int line, int col) {
            VsUtilities.NavigateTo(Instance, filename, Guid.Empty, line, col);
        }

        internal static void NavigateTo(string filename, int pos) {
            VsUtilities.NavigateTo(Instance, filename, Guid.Empty, pos);
        }

        /// <summary>
        /// The analyzer which is used for loose files.
        /// </summary>
        internal VsProjectAnalyzer DefaultAnalyzer {
            get {
                if (_analyzer == null) {
                    _analyzer = CreateLooseVsProjectAnalyzer();
                    LogLooseFileAnalysisLevel();
                    _analyzer.MaxLogLength = 100;
                }
                return _analyzer;
            }
        }

        private VsProjectAnalyzer CreateLooseVsProjectAnalyzer() {
            // In ES6 IntelliSense mode, rather than overriding the default JS editor,
            // we throw to Salsa. If Salsa detects that it's not operating within the Node.js project context,
            // it will throw to JSLS. This means that currently, loose files will be handled by the JSLS editor,
            // rather than the NodeLS editor (which means that the loose project analyzer will not be invoked
            // in these scenarios.)
            //
            // Because Salsa doesn't support the stitching together inputs from the surrounding text view,
            // we do not throw to Salsa from the REPL window. However, in order to provide a workable editing
            // experience within the REPL context, we initialize the loose analyzer with Quick IntelliSense
            // during ES6 mode.
            var analysisLevel = AnalysisLevel.NodeLsMedium;
            return new VsProjectAnalyzer(analysisLevel, false);
        }

        private void LogLooseFileAnalysisLevel() {
            var analyzer = _analyzer;
            if (analyzer != null) {
                var val = analyzer.AnalysisLevel;
                _logger.LogEvent(NodejsToolsLogEvent.AnalysisLevel, (int)val);
            }
        }

        private static string GetTypeScriptToolsVersion() {
            var toolsVersion = string.Empty;
            try {
                object installDirAsObject = null;
                var shell = NodejsPackage.Instance.GetService(typeof(SVsShell)) as IVsShell;
                if (shell != null) {
                    shell.GetProperty((int)__VSSPROPID.VSSPROPID_InstallDirectory, out installDirAsObject);
                }

                var idePath = CommonUtils.NormalizeDirectoryPath((string)installDirAsObject) ?? string.Empty;
                if (string.IsNullOrEmpty(idePath)) {
                    return toolsVersion;
                }

                var typeScriptServicesPath = Path.Combine(idePath, @"CommonExtensions\Microsoft\TypeScript\typescriptServices.js");
                if (!File.Exists(typeScriptServicesPath)) {
                    return toolsVersion;
                }

                var regex = new Regex(@"toolsVersion = ""(?<version>\d.\d?)"";");
                var fileText = File.ReadAllText(typeScriptServicesPath);
                var match = regex.Match(fileText);

                var version = match.Groups["version"].Value;
                if (!string.IsNullOrWhiteSpace(version)) {
                    toolsVersion = version;
                }
            } catch (Exception ex) {
                if (ex.IsCriticalException()) {
                    throw;
                }

                Debug.WriteLine(string.Format("Failed to obtain TypeScript tools version: {0}", ex.ToString()));
            }

            return toolsVersion;
        }
    }
}
