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
            if (parts.Length != 4) {
                throw new ArgumentException("Invalid fully qualified test name");
            }
            ModulePath = parts[0];
            ModuleName = parts[1];
            TestName = parts[2];
            TestFramework = parts[3];
        }

        public NodejsTestInfo(string modulePath, string testName, string modulaName, string testFramework)
        {
            ModulePath = modulePath;
            ModuleName = modulaName;
            TestName = testName;
            TestFramework = testFramework;
        }

        public string FullyQualifiedName {
            get {
                return ModulePath + "::" + ModuleName + "::" + TestName + "::" + TestFramework;
            }
        }
        public string ModulePath { get; private set; }

        public string ModuleName { get; private set; }

        public string TestName { get; private set; }

        public string TestFramework { get; private set; }

        public string DisplayName {
            get {
                return string.IsNullOrWhiteSpace(ModuleName) ? TestName : string.Format("{0}:{1}", ModuleName, TestName);
            }
        }
    }
}
