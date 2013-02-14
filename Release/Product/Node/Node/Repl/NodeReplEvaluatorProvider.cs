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

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Repl;

namespace Microsoft.NodeTools.Repl {
    [ReplRole("Reset"), ReplRole("Execution")]
    [Export(typeof(IReplEvaluatorProvider))]
    class NodeReplEvaluatorProvider : IReplEvaluatorProvider {
        internal const string NodeReplId = "{E4AC36B7-EDC5-4AD2-B758-B5416D520705}";
        
        #region IAltReplEvaluatorProvider Members

        public IReplEvaluator GetEvaluator(string replId) {
            if (replId == NodeReplId) {
                return new NodeReplEvaluator();
            }
            return null;
        }

        #endregion
    }
}
