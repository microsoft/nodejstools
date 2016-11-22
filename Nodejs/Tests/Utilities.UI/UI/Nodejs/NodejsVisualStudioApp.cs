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
using System.Windows.Automation;
using EnvDTE;

namespace TestUtilities.UI.Nodejs {
    public class NodejsVisualStudioApp : VisualStudioApp {
        private NodejsPerfExplorer _perfTreeView;
        private NodejsPerfToolBar _perfToolBar;

        public const string JavaScriptTemplateLanguageName = "JavaScript";
        public const string TypeScriptTemplateLanguageName = "TypeScript";

        public const string JavaScriptAzureWebAppTemplate = "Blank Azure Node.js Web Application";
        public const string TypeScriptAzureWebAppTemplate = "Blank Azure Node.js Web Application";
        public const string JavascriptWebAppTemplate = "Blank Node.js Web Application";

        public NodejsVisualStudioApp(DTE dte = null)
            : base(dte) {
        }

        protected override void Dispose(bool disposing) {
            if (!IsDisposed) {
                try {
                    InteractiveWindow.CloseAll(this);
                } catch (Exception ex) {
                    Console.WriteLine("Error while closing all interactive windows");
                    Console.WriteLine(ex);
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Opens and activates the solution explorer window.
        /// </summary>
        public void OpenNodejsPerformance() {
            Dte.ExecuteCommand("View.Node.jsPerformanceExplorer");
        }

        /// <summary>
        /// Provides access to the Python profiling tree view.
        /// </summary>
        public NodejsPerfExplorer NodejsPerformanceExplorerTreeView {
            get {
                if (_perfTreeView == null) {
                    var element = Element.FindFirst(TreeScope.Descendants,
                        new AndCondition(
                            new PropertyCondition(
                                AutomationElement.ClassNameProperty,
                                "SysTreeView32"
                            ),
                            new PropertyCondition(
                                AutomationElement.NameProperty,
                                "Node.js Performance"
                            )
                        )
                    );
                    _perfTreeView = new NodejsPerfExplorer(element);
                }
                return _perfTreeView;
            }
        }

        /// <summary>
        /// Provides access to the Python profiling tool bar
        /// </summary>
        public NodejsPerfToolBar NodejsPerformanceExplorerToolBar {
            get {
                if (_perfToolBar == null) {
                    var element = Element.FindFirst(TreeScope.Descendants,
                        new AndCondition(
                            new PropertyCondition(
                                AutomationElement.ClassNameProperty,
                                "ToolBar"
                            ),
                            new PropertyCondition(
                                AutomationElement.NameProperty,
                                "Node.js Performance"
                            )
                        )
                    );
                    _perfToolBar = new NodejsPerfToolBar(element);
                }
                return _perfToolBar;
            }
        }

        public Document WaitForDocument(string docName) {
            for (int i = 0; i < 20; i++) {
                try {
                    return Dte.Documents.Item(docName);
                } catch {
                    System.Threading.Thread.Sleep(500);
                }
            }
            throw new InvalidOperationException("Document not opened: " + docName);
        }

        public InteractiveWindow GetInteractiveWindow(string title) {
            string autoId = GetName(title);
            AutomationElement element = null;
            for (int i = 0; i < 5 && element == null; i++) {
                element = Element.FindFirst(TreeScope.Descendants,
                        new AndCondition(
                            new PropertyCondition(
                                AutomationElement.AutomationIdProperty,
                                autoId
                            ),
                            new PropertyCondition(
                                AutomationElement.ClassNameProperty,
                                ""
                            )
                        )
                    );
                if (element == null) {
                    System.Threading.Thread.Sleep(500);
                }
            }

            return new InteractiveWindow(
                title,
                element.FindFirst(
                    TreeScope.Descendants,
                    new PropertyCondition(
                        AutomationElement.AutomationIdProperty,
                        "WpfTextView"
                    )
                ),
                this
            );

        }
    }
}
