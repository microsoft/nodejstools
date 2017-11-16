// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.InteractiveWindow;

namespace Microsoft.NodejsTools.Repl
{
    [Export(typeof(NodejsReplEvaluatorProvider))]
    internal class NodejsReplEvaluatorProvider
    {
        public const string NodeReplId = "{E4AC36B7-EDC5-4AD2-B758-B5416D520705}";

        public IInteractiveEvaluator GetEvaluator(string replId)
        {
            if (replId == NodeReplId)
            {
                return new NodejsReplEvaluator();
            }
            return null;
        }
    }
}
