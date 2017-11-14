// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Repl
{
    internal class VsNodejsReplSite
    {
        public static readonly VsNodejsReplSite Site = new VsNodejsReplSite();

        public CommonProjectNode GetStartupProject()
        {
            var nodeJsInstance = NodejsPackage.Instance;
            if (nodeJsInstance == null)
            {
                // Node.js Tools package has not loaded yet. Expected if no NTVS project is open.
                return null;
            }
            return NodejsPackage.GetStartupProject(nodeJsInstance);
        }

        public bool TryGetStartupFileAndDirectory(out string fileName, out string directory)
        {
            var nodeJsInstance = NodejsPackage.Instance;
            if (nodeJsInstance == null)
            {
                // Node.js Tools package has not loaded yet. Expected if no NTVS project is open.
                fileName = null;
                directory = null;
                return false;
            }
            return NodejsPackage.TryGetStartupFileAndDirectory(nodeJsInstance, out fileName, out directory);
        }
    }
}
