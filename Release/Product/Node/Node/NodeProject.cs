/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;
using System.Xml;
using System.IO;
using Microsoft.PythonTools;

namespace Microsoft.NodeTools {
    [Guid("78D985FC-2CA0-4D08-9B6B-35ACD5E5294A")]
    class NodeProject : FlavoredProjectBase, IOleCommandTarget, IVsProjectFlavorCfgProvider {
        internal IVsProject _innerProject;
        internal NodePackage _package;
        private OleMenuCommandService _menuService;

        protected override void Close() {
            if (_menuService != null) {
                /*foreach (var command in _commands) {
                    _menuService.RemoveCommand(command);
                }*/
            }

        }

        protected override void InitializeForOuter(string fileName, string location, string name, uint flags, ref Guid guidProject, out bool cancel) {
            base.InitializeForOuter(fileName, location, name, flags, ref guidProject, out cancel);

            object extObject;
            ErrorHandler.ThrowOnFailure(
                _innerVsHierarchy.GetProperty(
                    VSConstants.VSITEMID_ROOT,
                    (int)__VSHPROPID.VSHPROPID_ExtObject,
                    out extObject
                )
            );

            var proj = extObject as EnvDTE.Project;
            if (proj != null) {
                try {
                    dynamic webAppExtender = proj.get_Extender("WebApplication");
                    if (webAppExtender != null) {
                        webAppExtender.StartWebServerOnDebug = false;
                    }
                } catch (COMException) {
                    // extender doesn't exist...
                }
            }
        }

        protected override void SetInnerProject(IntPtr innerIUnknown) {
            var inner = Marshal.GetObjectForIUnknown(innerIUnknown);

            // The reason why we keep a reference to those is that doing a QI after being
            // aggregated would do the AddRef on the outer object.
            _innerProject = inner as IVsProject;
            _innerVsHierarchy = inner as IVsHierarchy;

            // Ensure we have a service provider as this is required for menu items to work
            if (this.serviceProvider == null)
                this.serviceProvider = (System.IServiceProvider)this._package;

            // Now let the base implementation set the inner object
            base.SetInnerProject(innerIUnknown);

            // Add our commands (this must run after we called base.SetInnerProject)            
            _menuService = ((System.IServiceProvider)this).GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

        }

        int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            if (pguidCmdGroup == GuidList.guidWebPackgeCmdId) {
                if (nCmdID == 0x101 /*  EnablePublishToWindowsAzureMenuItem*/) {

                    // We need to forward the command to the web publish package and let it handle it, while
                    // we listen for the project which is going to get added.  After the command succeds
                    // we can then go and update the newly added project so that it is setup appropriately for
                    // Python...
                    using (var listener = new AzureSolutionListener(this)) {
                        var shell = (IVsShell)((System.IServiceProvider)this).GetService(typeof(SVsShell));
                        Guid webPublishPackageGuid = GuidList.guidWebPackageGuid;
                        IVsPackage package;
                        
                        if (ErrorHandler.Succeeded(shell.LoadPackage(ref webPublishPackageGuid, out package))) {
                            var managedPack = package as IOleCommandTarget;
                            if (managedPack != null) {
                                int res = managedPack.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                                if (ErrorHandler.Succeeded(res)) {
                                    // update the users service definition file to include import...
                                    foreach (var project in listener.OpenedHierarchies) {
                                        UpdateAzureDeploymentProject(project);
                                    }
                                }


                                return res;
                            }
                        }
                    }
                }
            }

            return ((IOleCommandTarget)_menuService).Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText) {
            if (pguidCmdGroup == GuidList.guidVenusCmdId) {
                for (int i = 0; i < prgCmds.Length; i++) {
                    switch (prgCmds[i].cmdID) {
                        case 0x034: /* add app assembly folder */
                        case 0x035: /* add app code folder */
                        case 0x036: /* add global resources */
                        case 0x037: /* add local resources */
                        case 0x038: /* add web refs folder */
                        case 0x039: /* add data folder */
                        case 0x040: /* add browser folders */
                        case 0x041: /* theme */
                        case 0x054: /* package settings */
                        case 0x055: /* context package settings */

                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE | OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                            return VSConstants.S_OK;
                    }
                }
            } else if (pguidCmdGroup == GuidList.guidWebPackgeCmdId) {
                if (prgCmds[0].cmdID == 0x101 /*  EnablePublishToWindowsAzureMenuItem*/) {
                    Console.WriteLine("Hi!");
                }
            } else if (pguidCmdGroup == GuidList.guidWebAppCmdId) {
                for (int i = 0; i < prgCmds.Length; i++) {
                    switch (prgCmds[i].cmdID) {
                        case 0x06A: /* check accessibility */
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE | OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_DEFHIDEONCTXTMENU | OLECMDF.OLECMDF_ENABLED);
                            return VSConstants.S_OK;
                    }
                }
            } else if (pguidCmdGroup == VSConstants.VSStd2K) {
                for (int i = 0; i < prgCmds.Length; i++) {
                    switch ((VSConstants.VSStd2KCmdID)prgCmds[i].cmdID) {
                        case VSConstants.VSStd2KCmdID.SETASSTARTPAGE:
                        case VSConstants.VSStd2KCmdID.CHECK_ACCESSIBILITY:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE | OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_DEFHIDEONCTXTMENU | OLECMDF.OLECMDF_ENABLED);
                            return VSConstants.S_OK;
                    }
                }
            } else if (pguidCmdGroup == VSConstants.GUID_VSStandardCommandSet97) {
                for (int i = 0; i < prgCmds.Length; i++) {
                    switch ((VSConstants.VSStd97CmdID)prgCmds[i].cmdID) {
                        case VSConstants.VSStd97CmdID.PreviewInBrowser:
                        case VSConstants.VSStd97CmdID.BrowseWith:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE | OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_DEFHIDEONCTXTMENU | OLECMDF.OLECMDF_ENABLED);
                            return VSConstants.S_OK;
                    }
                }
            }

            return ((IOleCommandTarget)_menuService).QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }


        private void UpdateAzureDeploymentProject(IVsHierarchy project) {
            object projKind;
            if (!ErrorHandler.Succeeded(project.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_TypeName, out projKind)) ||
                !(projKind is string) ||
                (string)projKind != "CloudComputingProjectType") {
                return;
            }

            var dteProject = project.GetProject();
            var serviceDef = dteProject.ProjectItems.Item("ServiceDefinition.csdef");
            if (serviceDef != null && serviceDef.FileCount == 1) {
                var filename = serviceDef.FileNames[0];
                UpdateServiceDefinition(filename);
            }
        }

        private static void UpdateServiceDefinition(string filename) {
            List<string> elements = new List<string>();
            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true, IndentChars = " ", NewLineHandling = NewLineHandling.Entitize };
            using (var reader = XmlReader.Create(filename)) {
                using (var writer = XmlWriter.Create(filename + ".tmp", settings)) {
                    while (reader.Read()) {
                        switch (reader.NodeType) {
                            case XmlNodeType.Element:
                                // TODO: Switch to the code below when we can successfully install our module...
                                if (reader.Name == "Imports" &&
                                        elements.Count == 2 &&
                                        elements[0] == "ServiceDefinition" &&
                                        elements[1] == "WebRole") {
                                    // insert our Imports node
                                    writer.WriteStartElement("Startup");
                                    writer.WriteStartElement("Task");
                                    writer.WriteAttributeString("commandLine", "setup_web.cmd > log.txt");
                                    writer.WriteAttributeString("executionContext", "elevated");
                                    writer.WriteAttributeString("taskType", "simple");

                                    writer.WriteStartElement("Environment");
                                    writer.WriteStartElement("Variable");
                                    writer.WriteAttributeString("name", "EMULATED");
                                    writer.WriteStartElement("RoleInstanceValue");
                                    writer.WriteAttributeString("xpath", "/RoleEnvironment/Deployment/@emulated");

                                    writer.WriteEndElement(); // RoleInstanceValue
                                    writer.WriteEndElement(); // Variable

                                    writer.WriteStartElement("Variable");
                                    writer.WriteAttributeString("name", "RUNTIMEID");
                                    writer.WriteAttributeString("value", "NODE;IISNODE");
                                    writer.WriteEndElement(); // Variable

                                    writer.WriteStartElement("Variable");
                                    writer.WriteAttributeString("name", "RUNTIMEURL");
                                    writer.WriteAttributeString("value", "http://nodertncu.blob.core.windows.net/node/0.6.20.exe;http://nodertncu.blob.core.windows.net/iisnode/0.1.21.exe");
                                    writer.WriteEndElement(); // Variable

                                    writer.WriteEndElement(); // Environment
                                    writer.WriteEndElement(); // Task
                                    writer.WriteEndElement(); // Startup
                                }
                                writer.WriteStartElement(reader.Prefix, reader.Name, reader.NamespaceURI);
                                writer.WriteAttributes(reader, true);

                                if (!reader.IsEmptyElement) {
                                    /*
                                    if (reader.Name == "Imports" &&
                                        elements.Count == 2 &&
                                        elements[0] == "ServiceDefinition" &&
                                        elements[1] == "WebRole") {

                                        writer.WriteStartElement("Import");
                                        writer.WriteAttributeString("moduleName", "PythonTools");
                                        writer.WriteEndElement();
                                    }*/

                                    elements.Add(reader.Name);
                                } else {
                                    writer.WriteEndElement();
                                }
                                break;
                            case XmlNodeType.Text:
                                writer.WriteString(reader.Value);
                                break;
                            case XmlNodeType.EndElement:
                                writer.WriteFullEndElement();
                                elements.RemoveAt(elements.Count - 1);
                                break;
                            case XmlNodeType.XmlDeclaration:
                            case XmlNodeType.ProcessingInstruction:
                                writer.WriteProcessingInstruction(reader.Name, reader.Value);
                                break;
                            case XmlNodeType.SignificantWhitespace:
                                writer.WriteWhitespace(reader.Value);
                                break;
                            case XmlNodeType.Attribute:
                                writer.WriteAttributes(reader, true);
                                break;
                            case XmlNodeType.CDATA:
                                writer.WriteCData(reader.Value);
                                break;
                            case XmlNodeType.Comment:
                                writer.WriteComment(reader.Value);
                                break;
                        }
                    }
                }
            }

            File.Delete(filename);
            File.Move(filename + ".tmp", filename);
        }

        #region IVsProjectFlavorCfgProvider Members

        public int CreateProjectFlavorCfg(IVsCfg pBaseProjectCfg, out IVsProjectFlavorCfg ppFlavorCfg) {
            // We're flavored with a Web Application project and our normal project...  But we don't
            // want the web application project to influence our config as that alters our debug
            // launch story.  We control that w/ the Django project which is actually just letting the
            // base Python project handle it.  So we keep the base Python project config here.
            ppFlavorCfg = pBaseProjectCfg as IVsProjectFlavorCfg;
            return VSConstants.S_OK;
        }

        #endregion


        protected override int GetProperty(uint itemId, int propId, out object property) {
            switch ((__VSHPROPID4)propId) {

                case __VSHPROPID4.VSHPROPID_TargetFrameworkMoniker:
                    // really only here for testing so WAP projects load correctly...
                    // But this also impacts the toolbox by filtering what available items there are.
                    property = ".NETFramework,Version=v4.0,Profile=Client";
                    return VSConstants.S_OK;
            }

            return base.GetProperty(itemId, propId, out property);
        }
    }
}
