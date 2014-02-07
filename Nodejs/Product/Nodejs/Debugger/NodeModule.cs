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

using System.Diagnostics;
using System.IO;

namespace Microsoft.NodejsTools.Debugger {
    class NodeModule {
        private readonly ISourceMapper _mapper;
        private readonly int _moduleId;
        private readonly string _fileName;
        private readonly string _source;
        private object _document;

        public NodeModule(ISourceMapper mapper, int moduleId, string fileName, string source = null) {
            Debug.Assert(fileName != null);

            _mapper = mapper;
            _moduleId = moduleId;
            _fileName = fileName;
            _source = source;
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

        public string JavaScriptFileName {
            get {
                return _fileName;
            }
        }

        public string FileName {
            get {
                if (_mapper != null) {
                    var mapping = _mapper.MapToOriginal(_fileName, 0);
                    if (mapping != null) {
                        return Path.Combine(Path.GetDirectoryName(_fileName), Path.GetFileName(mapping.FileName));
                    }
                }
                return _fileName;
            }
        }

        public string Source {
            get {
                return _source;
            }
        }

        public bool BuiltIn {
            get {
                // No directory separator characters implies builtin
                return (_fileName.IndexOfAny(new [] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) == -1);
            }
        }

        public object Document {
            get {
                return _document;
            }
            set {
                _document = value;
            }
        }
    }
}
