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
using Microsoft.NodejsTools.TestFrameworks;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks {
    class FrameworkDiscover {
        private readonly Dictionary<String, TestFramework> _frameworks;
        public FrameworkDiscover(): this(null) {
        }

        public FrameworkDiscover(IEnumerable<string> testFrameworkDirectories) {
            if (testFrameworkDirectories == null) {
                TestFrameworkDirectories directoryLoader = new TestFrameworkDirectories();
                testFrameworkDirectories = directoryLoader.GetFrameworkDirectories();
            }
            _frameworks = new Dictionary<string, TestFramework>(StringComparer.OrdinalIgnoreCase);
            foreach (string directory in testFrameworkDirectories) {
                TestFramework fx = new TestFramework(directory);
                _frameworks.Add(fx.Name, fx);
            }
        }

        public TestFramework Get(string frameworkName) {
            TestFramework testFX = null;
            _frameworks.TryGetValue(frameworkName, out testFX);
            return testFX;
        }
    }
}
