// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Debugger.Serialization
{
    /// <summary>
    /// Contains VS type aliases for v8 types.
    /// </summary>
    internal sealed class NodeVariableType
    {
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
        public const string AnonymousFunction = "(anonymous function)";
        public const string AnonymousVariable = "(anonymous variable)";
        public const string UnknownModule = "<unknown>";
        public const string Prototype = "__proto__";
    }
}
