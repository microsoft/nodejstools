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
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using Microsoft.NodejsTools;
#if DEV11
using Microsoft.VisualStudio.Shell.Interop;
#endif

namespace Microsoft.NodejsTools.Project {
    //Set the projectsTemplatesDirectory to a non-existant path to prevent VS from including the working directory as a valid template path
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Description("Node.js Project Package")]
    [Guid("D861969D-CEC8-4411-AB85-C08EFE4100A2")]
    [DeveloperActivity(NodeConstants.JavaScript, typeof(NodeProjectPackage))]
    [ProvideProjectFactory(typeof(BaseNodeProjectFactory), NodeConstants.Nodejs, NodeFileFilter, "njsproj", "njsproj", ".\\NullPath", LanguageVsTemplate = NodeConstants.Nodejs)]
    public class NodeProjectPackage : CommonProjectPackage {
        internal const string NodeFileFilter = "Node.js Project Files (*.njsproj);*.njsproj";

        public override ProjectFactory CreateProjectFactory() {
            return new BaseNodeProjectFactory(this);
        }

        public override CommonEditorFactory CreateEditorFactory() {
            return null;
        }

        public override CommonEditorFactory CreateEditorFactoryPromptForEncoding() {
            return null;
        }

        protected override void Initialize() {
            RegisterProjectFactory(new BaseNodeProjectFactory(this));

            base.Initialize();
        }

        /// <summary>
        /// This method is called to get the icon that will be displayed in the
        /// Help About dialog when this package is selected.
        /// </summary>
        /// <returns>The resource id corresponding to the icon to display on the Help About dialog</returns>
        public override uint GetIconIdForAboutBox() {
            return 0;
        }
        /// <summary>
        /// This method is called during Devenv /Setup to get the bitmap to
        /// display on the splash screen for this package.
        /// </summary>
        /// <returns>The resource id corresponding to the bitmap to display on the splash screen</returns>
        public override uint GetIconIdForSplashScreen() {
            return 0;
        }
        /// <summary>
        /// This methods provides the product official name, it will be
        /// displayed in the help about dialog.
        /// </summary>
        public override string GetProductName() {
            return "Node.js Project";
        }

        /// <summary>
        /// This methods provides the product description, it will be
        /// displayed in the help about dialog.
        /// </summary>
        public override string GetProductDescription() {
            return NodeConstants.Nodejs;
            //return Resources.ProductDescription;
        }
        /// <summary>
        /// This methods provides the product version, it will be
        /// displayed in the help about dialog.
        /// </summary>
        public override string GetProductVersion() {
            return this.GetType().Assembly.GetName().Version.ToString();
        }
    }
}
