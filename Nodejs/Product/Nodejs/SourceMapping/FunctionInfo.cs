// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.SourceMapping
{
    public sealed class FunctionInformation
    {
        public readonly string Namespace;
        public readonly string Function;
        public readonly string Filename;
        public readonly int? LineNumber;
        public readonly bool IsRecompilation;

        internal FunctionInformation(string ns, string methodName, int? lineNo, string filename, bool isRecompilation = false)
        {
            this.Namespace = ns;
            this.Function = methodName;
            this.LineNumber = lineNo;
            this.Filename = filename;
            this.IsRecompilation = isRecompilation;
        }
    }
}
