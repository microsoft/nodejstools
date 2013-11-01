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
using System.Diagnostics;
using System.IO;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Microsoft.VisualStudio.TemplateWizard;

namespace Microsoft.NodejsTools.ProjectWizard {
    /// <summary>
    /// Provides a project wizard extension which will optionally do an
    /// npm install after the project is created.
    /// </summary>
    public sealed class NpmWizardExtension : IWizard {
        #region IWizard Members

        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem) {
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project) {
            // Prompt the user if we should run npm install
            EnvDTE.ProjectItem packageJson;
            try {
                packageJson = project.ProjectItems.Item("package.json");
            } catch (ArgumentException) {
                return;
            }

            var serializer = new JavaScriptSerializer();
            Dictionary<string, object> jsonDict;
            try {
                jsonDict = (Dictionary<string, object>)serializer.DeserializeObject(File.ReadAllText(packageJson.get_FileNames(1)));
            } catch {
                // can't read, failed to deserialize json
                return;
            }

            if (File.Exists(Nodejs.NodeExePath)) {
                var npmPath = Path.Combine(Path.GetDirectoryName(Nodejs.NodeExePath), "npm.cmd");
                if (File.Exists(npmPath)) {

                    object dependenciesObject;
                    Dictionary<string, object> dependencies;
                    if (jsonDict.TryGetValue("dependencies", out dependenciesObject) &&
                        (dependencies = dependenciesObject as Dictionary<string, object>) != null &&
                        dependencies.Count > 0) {

                        // prompt the user to install dependencies
                        var shouldDoInstall = MessageBox.Show(@"The newly created project has dependencies defined in package.json.

Do you want to run npm install to get the dependencies now?",
                            "Node.js Tools for Visual Studio",
                            MessageBoxButtons.YesNo
                        );

                        if (shouldDoInstall == DialogResult.Yes) {
                            var cmd = Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.System),
                                "cmd.exe"
                            );

                            var psi = new ProcessStartInfo(
                                cmd,
                                String.Format("/C \"{0}\" install && pause", npmPath)
                            ) { WorkingDirectory = (string)project.Properties.Item("ProjectHome").Value };
                            psi.UseShellExecute = false;
                            try {
                                using (var proc = Process.Start(psi)) {
                                    proc.WaitForExit();
                                }
                            } catch {
                            }
                        }
                    }
                }
            }
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem) {
        }

        public void RunFinished() {
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams) {
        }

        public bool ShouldAddProjectItem(string filePath) {
            return true;
        }

        #endregion
    }
}
