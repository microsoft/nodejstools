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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Formatting {
    /// <summary>
    /// Encapsulates a VS ITextSnapshot interface and marshals it into the
    /// JavaScript engine using the API that TypeScript expects.
    /// </summary>
    [ComVisible(true)]
    public sealed class TypeScriptSnapshot {
        private readonly ITextSnapshot _snapshot;
        private readonly TypeScriptServiceHost _host;

        public TypeScriptSnapshot(TypeScriptServiceHost host, ITextSnapshot snapshot) {
            _snapshot = snapshot;
            _host = host;
        }

        public string getText(int start, int end) {
            return _snapshot.GetText(start, end - start);            
        }

        public int getLength() {
            return _snapshot.Length;
        }

        public object getLineStartPositions() {
            object[] res = new object[_snapshot.LineCount];
            int i = 0;
            foreach(var line in _snapshot.Lines) {
                res[i++] = line.Start.Position;
            }
            return _host.ArrayMaker(new ArrayHelper(res));
        }

        public object /*: TypeScript.TextChangeRange*/ getTextChangeRangeSinceVersion(int scriptVersion) {
            return DBNull.Value;
        }
    }
}
