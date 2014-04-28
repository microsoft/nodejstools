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

using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks {
    class FrameworkDiscover {
        private readonly Dictionary<String, TestFramework> _framworks;
        private readonly TestFramework Default;
        public FrameworkDiscover(): this(null) {
        }

        public FrameworkDiscover(string installFolder) {
            if (string.IsNullOrEmpty(installFolder)) {
                installFolder = GetExecutingAssemblyPath();
            }
            _framworks = new Dictionary<string, TestFramework>(StringComparer.OrdinalIgnoreCase);
            string baseTestframeworkFolder = installFolder + @"\TestFrameworks";
            foreach (TestFrameworkType testFX in (TestFrameworkType[])Enum.GetValues(typeof(TestFrameworkType))) {
                if (testFX != TestFrameworkType.None) {
                    string frameworkFolder = Path.Combine(baseTestframeworkFolder, testFX.ToString());
                    TestFramework fx = new TestFramework(frameworkFolder);
                    _framworks.Add(fx.Name, fx);
                    if (testFX == TestFrameworkType.Default) {
                        Default = fx;
                    }
                }
            }
            if (Default == null) {
                throw new InvalidOperationException("Missing generic test framework");
            }
        }

        public TestFramework Get(string frameworkName) {
            TestFramework testFX = null;
            _framworks.TryGetValue(frameworkName, out testFX);
            return testFX;
        }

        private string GetExecutingAssemblyPath() {
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
