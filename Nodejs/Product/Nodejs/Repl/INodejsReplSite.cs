// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Repl
{
    /// <summary>
    /// Internal interface to mock out project system inputs for test cases.
    /// </summary>
    internal interface INodejsReplSite
    {
        CommonProjectNode GetStartupProject();
        bool TryGetStartupFileAndDirectory(out string fileName, out string directory);
    }
}

