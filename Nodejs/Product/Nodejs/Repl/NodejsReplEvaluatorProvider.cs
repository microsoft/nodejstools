// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;

namespace Microsoft.NodejsTools.Repl
{
    [Export(typeof(IReplEvaluatorProvider))]
    internal class NodejsReplEvaluatorProvider : IReplEvaluatorProvider
    {
        internal const string NodeReplId = "{E4AC36B7-EDC5-4AD2-B758-B5416D520705}";

        #region IAltReplEvaluatorProvider Members

        public IReplEvaluator GetEvaluator(string replId)
        {
            if (replId == NodeReplId)
            {
                return new NodejsReplEvaluator();
            }
            return null;
        }

        #endregion
    }
}

