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

namespace Microsoft.NodejsTools.TestFrameworks {
    class TestFrameworkDirectories {
        private readonly Dictionary<string, string> _frameworkDirectories;
        public const string DefaultFramework = "Default"; 

        public TestFrameworkDirectories() {
            string installFolder = GetExecutingAssemblyPath();
            _frameworkDirectories = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            string baseTestframeworkFolder = installFolder + @"\TestFrameworks";
            foreach (string directory in Directory.GetDirectories(baseTestframeworkFolder)) {
                string name = Path.GetFileName(directory);
                _frameworkDirectories.Add(name, directory);
            }
            string defaultFx;
            _frameworkDirectories.TryGetValue(DefaultFramework, out defaultFx);
            if (defaultFx == null) {
                throw new InvalidOperationException("Missing generic test framework");
            }
        }

        public List<string> GetFrameworkNames() {
            return new List<string>(_frameworkDirectories.Keys);
        }

        public List<string> GetFrameworkDirectories() {
            return new List<string>(_frameworkDirectories.Values);
        }

        private string GetExecutingAssemblyPath() {
            string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
