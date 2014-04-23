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
using System.IO;
using System.Linq;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks {
    //TODO, this should be an application level global object, where to put it?
    class FrameworkDiscover {
        private List<TestFramework> _framworks;
        private TestFramework Default = null;
        public FrameworkDiscover() {
            string installFolder = GetExecutingAssemblyPath();
            _framworks = new List<TestFramework>();
            foreach (string directory in Directory.GetDirectories(installFolder + @"\TestFrameworks")) {
                TestFramework fx = new TestFramework(directory);
                _framworks.Add(fx);
            }
            Default = Search("generic");
        }

        public TestFramework Get(string frameworkName) {
            TestFramework matched = Search(frameworkName);
            if (matched == null) {
                matched = Default;
            }
            return matched;
        }

        private TestFramework Search(string frameworkName) {
            TestFramework matched = _framworks.FirstOrDefault((p) => { 
                return p.Name.Equals(frameworkName, StringComparison.OrdinalIgnoreCase); 
            });
            return matched;
        }
        private string GetExecutingAssemblyPath() {
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
