// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Repl
{
    public static class ReplWindowExtensions
    {
        public static IReplEvaluator GetReplEvaluator(this ITextBuffer textBuffer)
        {
            IReplEvaluator res;
            if (textBuffer.Properties.TryGetProperty<IReplEvaluator>(typeof(IReplEvaluator), out res))
            {
                return res;
            }
            return null;
        }
    }
}
