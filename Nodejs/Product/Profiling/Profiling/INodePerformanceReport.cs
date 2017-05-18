// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools.Profiling
{
    [Guid("5949B936-37D6-47E6-BAE5-A2C49A6EE74B")]
    public interface INodePerformanceReport
    {
        string Filename
        {
            get;
        }
    }
}

