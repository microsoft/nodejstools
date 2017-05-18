// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Debugger
{
    internal sealed class NodeException
    {
        public readonly string TypeName, Description;

        public NodeException(string typeName, string description)
        {
            this.TypeName = typeName;
            this.Description = description;
        }
    }
}
