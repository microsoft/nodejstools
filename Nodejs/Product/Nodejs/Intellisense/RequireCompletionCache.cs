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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Intellisense {
    /// <summary>
    /// Stores cached completion results for require calls which can be expensive
    /// to calculate in large projects.  The cache is cleared when anything which
    /// would alter require semantics changes and each file will need to be updated.
    /// </summary>
    class RequireCompletionCache {
        private Dictionary<FileNode, CompletionInfo[]> _cachedEntries = new Dictionary<FileNode, CompletionInfo[]>();

        public void Clear() {
            _cachedEntries.Clear();
        }

        public bool TryGetCompletions(FileNode node, out CompletionInfo[] result) {
            return _cachedEntries.TryGetValue(node, out result);
        }

        public void CacheCompletions(FileNode node, CompletionInfo[] completions) {
            _cachedEntries[node] = completions;
        }
    }
}
