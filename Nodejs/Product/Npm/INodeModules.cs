// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm
{
    public interface INodeModules : IEnumerable<IPackage>
    {
        int Count { get; }
        IPackage this[int index] { get; }
        IPackage this[string name] { get; }
        bool Contains(string name);
        bool HasMissingModules { get; }
        int GetDepth(string filepath);
    }
}
