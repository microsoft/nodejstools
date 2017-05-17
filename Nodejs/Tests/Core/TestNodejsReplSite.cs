// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.NodejsTools.Repl;
using Microsoft.VisualStudioTools.Project;

namespace NodejsTests
{
    internal class TestNodejsReplSite : INodejsReplSite
    {
        private readonly string _filename, _projectDir;
        public static TestNodejsReplSite Instance = new TestNodejsReplSite(null, null);

        public TestNodejsReplSite(string filename, string projectDir)
        {
            _filename = filename;
            _projectDir = projectDir;
        }

        #region INodejsReplSite Members

        public CommonProjectNode GetStartupProject()
        {
            return null;
        }

        public bool TryGetStartupFileAndDirectory(out string fileName, out string directory)
        {
            if (_projectDir != null)
            {
                fileName = _filename;
                directory = _projectDir;
                return true;
            }
            fileName = null;
            directory = null;
            return false;
        }

        #endregion
    }
}

