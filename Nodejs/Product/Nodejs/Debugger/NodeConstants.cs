// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Debugger
{
    internal sealed class NodeConstants
    {
        public const string ScriptWrapBegin = "(function (exports, require, module, __filename, __dirname) { ";
        public const string ScriptWrapEnd = "\n});";
    }
}
