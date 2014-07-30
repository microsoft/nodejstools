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
using System.IO;

namespace Microsoft.NodejsTools.TestAdapter.TestFrameworks {
    class NodejsTestInfo {
        public NodejsTestInfo(string fullyQualifiedName) {
            string[] parts = fullyQualifiedName.Split(new string[] { "::" }, StringSplitOptions.None);
            if (parts.Length != 3) {
                throw new ArgumentException("Invalid fully qualified test name");
            }
            ModulePath = parts[0];
            TestName = parts[1];
            TestFramework = parts[2];
        }

        public NodejsTestInfo(string modulePath, string testName, string modulaName, string testFramework)
        {
            ModulePath = modulePath;
            TestName = testName;
            TestFramework = testFramework;
        }

        public string FullyQualifiedName {
            get {
                return ModulePath + "::" + TestName + "::" + TestFramework;
            }
        }
        public string ModulePath { get; private set; }

        public string TestName { get; private set; }

        public string TestFramework { get; private set; }
    }
}
