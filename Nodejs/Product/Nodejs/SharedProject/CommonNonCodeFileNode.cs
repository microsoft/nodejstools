// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.VisualStudioTools.Project
{
    internal class CommonNonCodeFileNode : CommonFileNode
    {
        public CommonNonCodeFileNode(CommonProjectNode root, ProjectElement e)
            : base(root, e)
        {
        }

        /// <summary>
        /// Open a file depending on the SubType property associated with the file item in the project file
        /// </summary>
        protected override void DoDefaultAction()
        {
            if ("WebBrowser".Equals(this.SubType, StringComparison.OrdinalIgnoreCase))
            {
                CommonPackage.OpenVsWebBrowser(this.ProjectMgr.Site, this.Url);
                return;
            }

            var manager = this.GetDocumentManager() as FileDocumentManager;
            Utilities.CheckNotNull(manager, "Could not get the FileDocumentManager");

            var viewGuid = Guid.Empty;
            manager.Open(false, false, viewGuid, out var frame, WindowFrameShowAction.Show);
        }
    }
}
