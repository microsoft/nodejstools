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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Formatting {
    /// <summary>
    /// Provides the host which gives information to the TypeScript language
    /// service about files in our project.
    /// </summary>
    [ComVisible(true)]
    public sealed class TypeScriptServiceHost {
        internal dynamic ArrayMaker;
        private readonly Dictionary<string, BufferInfo> _openBuffers = new Dictionary<string, BufferInfo>(StringComparer.OrdinalIgnoreCase);

        class BufferInfo {
            public readonly ITextBuffer Buffer;
            public int OpenCount;

            public BufferInfo(ITextBuffer buffer) {
                Buffer = buffer;
                OpenCount = 1;
            }
        }


        public object/*CompilationSettings*/ getCompilationSettings() {
            return null;
        }

        public object getScriptFileNames() {
            return ArrayMaker(new ArrayHelper(_openBuffers.Keys.ToArray()));
        }

        public int getScriptVersion(string fileName) {
            BufferInfo bufferInfo;
            if (_openBuffers.TryGetValue(fileName, out bufferInfo)) {
                return bufferInfo.Buffer.CurrentSnapshot.Version.VersionNumber;
            }
            return -1;
        }

        public bool getScriptIsOpen(string fileName) {
            if (_openBuffers.ContainsKey(fileName)) {
                return true;
            }
            return false;
        }

        public int getScriptByteOrderMark(string fileName) {
            return (int)ByteOrderMark.None;
        }

        public TypeScriptSnapshot getScriptSnapshot(string fileName) {
            BufferInfo bufferInfo;
            if (_openBuffers.TryGetValue(fileName, out bufferInfo)) {
                return new TypeScriptSnapshot(this, bufferInfo.Buffer.CurrentSnapshot);
            }
            return null;
        }

        public object getDiagnosticsObject() {
            return null;
        }

        public object getLocalizedDiagnosticMessages() {
            return null;
        }

        public bool information() {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
        
        public bool debug() {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
        
        public bool warning() {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
        
        public bool error() {
            return true;
        }

        public bool fatal() {
            return true;
        }
        
        public void log(string s) {
            Debug.WriteLine(String.Format("LOG: {0}", s));
        }

        internal void AddDocument(string path, ITextBuffer buffer) {
           BufferInfo bufferInfo;
           if (!_openBuffers.TryGetValue(path, out bufferInfo)) {
               _openBuffers[path] = new BufferInfo(buffer);
           } else {
               bufferInfo.OpenCount += 1;
           }
        }

        internal void RemoveDocument(string path) {
            BufferInfo bufferInfo;
            if (_openBuffers.TryGetValue(path, out bufferInfo) && --bufferInfo.OpenCount == 0) {
                _openBuffers.Remove(path);
            }
        }
    }
}
