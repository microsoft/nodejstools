// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.SourceMapping
{
    internal class FunctionInformation
    {
        internal readonly string Namespace;
        internal readonly string Function;
        internal readonly string Filename;
        internal readonly int? LineNumber;
        internal readonly bool IsRecompilation;

        internal FunctionInformation(string ns, string methodName, int? lineNo, string filename) : this(ns, methodName, lineNo, filename, false) { }

        internal FunctionInformation(string ns, string methodName, int? lineNo, string filename, bool isRecompilation)
        {
            this.Namespace = ns;
            this.Function = methodName;
            this.LineNumber = lineNo;
            this.Filename = filename;
            this.IsRecompilation = isRecompilation;
        }
    }
}

