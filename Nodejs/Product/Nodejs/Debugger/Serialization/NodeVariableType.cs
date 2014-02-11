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

namespace Microsoft.NodejsTools.Debugger.Serialization {
    /// <summary>
    /// Contains VS type aliases for v8 types.
    /// </summary>
    sealed class NodeVariableType {
        public const string Unknown = "Unknown";
        public const string Undefined = "Undefined";
        public const string Null = "Null";
        public const string Number = "Number";
        public const string Boolean = "Boolean";
        public const string Regexp = "Regular Expression";
        public const string Function = "Function";
        public const string String = "String";
        public const string Object = "Object";
        public const string Error = "Error";
        public const string AnonymousFunction = "Anonymous function";
        public const string AnonymousVariable = "(anonymous variable)";
    }
}