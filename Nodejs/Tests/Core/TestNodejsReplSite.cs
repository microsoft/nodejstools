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

using Microsoft.NodejsTools.Repl;
using Microsoft.VisualStudioTools.Project;

namespace NodejsTests {
    class TestNodejsReplSite : INodejsReplSite {
        private readonly string _filename, _projectDir;
        public static TestNodejsReplSite Instance = new TestNodejsReplSite(null, null);

        public TestNodejsReplSite(string filename, string projectDir) {
            _filename = filename;
            _projectDir = projectDir;
        }

        #region INodejsReplSite Members

        public CommonProjectNode GetStartupProject() {
            return null;
        }

        public bool TryGetStartupFileAndDirectory(out string fileName, out string directory) {
            if (_projectDir != null) {
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
