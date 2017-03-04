// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools.Profiling
{
    [Guid("7C711031-50B4-4263-901E-9EF86DD6DC57")]
    public interface INodeProfileSession
    {
        string Name
        {
            get;
        }

        string Filename
        {
            get;
        }

        INodePerformanceReport GetReport(object item);

        void Save(string filename = null);

        void Launch(bool openReport = false);

        bool IsSaved
        {
            get;
        }
    }
}

