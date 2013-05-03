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
using System.Globalization;
using System.IO;
using Microsoft.PythonTools.Project;

namespace Microsoft.NodeTools.Project {
    class NodeProjectNode : CommonProjectNode {
        public NodeProjectNode(NodeProjectPackage package)
            : base(package, Utilities.GetImageList(typeof(NodeProjectNode).Assembly.GetManifestResourceStream("Microsoft.Node.NodeImageList.bmp"))) {
            
            Type projectNodePropsType = typeof(NodeProjectNodeProperties);
            AddCATIDMapping(projectNodePropsType, projectNodePropsType.GUID);
        }

        public override string[] CodeFileExtensions {
            get {
                return new[] { NodeConstants.FileExtension };
            }
        }


        protected internal override FolderNode CreateFolderNode(string path, ProjectElement element) {
            return new CommonFolderNode(this, path, element);
        }

        public override string GetProjectName() {
            return "NodeProject";
        }

        public override Type GetProjectFactoryType() {
            return typeof(NodeProjectFactory);
        }

        public override Type GetEditorFactoryType() {
            // Not presently used
            throw new NotImplementedException();
        }

        public override string GetFormatList() {
            return String.Format(CultureInfo.CurrentCulture, NodeConstants.FileExtension, "\0", "\0");
        }

        public override Type GetGeneralPropertyPageType() {
            return typeof(NodeGeneralPropertyPage);
        }

        public override Type GetLibraryManagerType() {
            return typeof(NodeLibraryManager);
        }

        public override IProjectLauncher GetLauncher() {
            return new NodeProjectLauncher(this);
        }

        protected override NodeProperties CreatePropertiesObject() {
            return new NodeProjectNodeProperties(this);
        }

        protected override Stream ProjectIconsImageStripStream {
            get {
                return typeof(ProjectNode).Assembly.GetManifestResourceStream("imagelis.bmp");
            }
        }
    }
}
