// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools.Profiling
{
    [ComVisible(true)]
    public sealed class ReportWrapper : INodePerformanceReport
    {
        private readonly Report _report;

        internal ReportWrapper(Report report)
        {
            _report = report;
        }

        #region INodePerformanceReport Members

        public string Filename
        {
            get { return _report.Filename; }
        }

        #endregion
    }
}

