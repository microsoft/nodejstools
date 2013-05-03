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

using System.IO;

namespace Microsoft.NodejsTools.Debugger {
    class NodeModule {
        private readonly int _moduleId;
        private readonly string _fileName;

        public NodeModule(int moduleId, string fileName) {
            _moduleId = moduleId;
            _fileName = fileName;
        }

        public int ModuleId {
            get {
                return _moduleId;
            }
        }

        public string Name {
            get {
                
                if (_fileName.IndexOfAny(Path.GetInvalidPathChars()) == -1) {
                    return Path.GetFileName(_fileName);
                }
                return _fileName;
            }
        }

        public string FileName {
            get {
                return _fileName;
            }
        }
    }
}
