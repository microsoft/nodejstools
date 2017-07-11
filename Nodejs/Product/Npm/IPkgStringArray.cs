// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm
{
    public interface IPkgStringArray : IEnumerable<string>
    {
        int Count { get; }
        string this[int index] { get; }
    }
}
