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

namespace Microsoft.NodejsTools.SourceMapping {

    internal class FunctionInformation {
        internal readonly string Namespace;
        internal readonly string Function;
        internal readonly string Filename;
        internal readonly int? LineNumber;
        internal readonly bool IsRecompilation;

        internal FunctionInformation(string ns, string methodName, int? lineNo, string filename) : this(ns, methodName, lineNo, filename, false) { }

        internal FunctionInformation(string ns, string methodName, int? lineNo, string filename, bool isRecompilation) {
            Namespace = ns;
            Function = methodName;
            LineNumber = lineNo;
            Filename = filename;
            IsRecompilation = isRecompilation;
        }
    }
}
