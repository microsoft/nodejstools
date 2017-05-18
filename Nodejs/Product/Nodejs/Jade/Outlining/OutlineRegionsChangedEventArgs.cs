// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Jade
{
    internal class OutlineRegionsChangedEventArgs : EventArgs
    {
        public OutlineRegionCollection Regions { get; private set; }
        public ITextRange ChangedRange { get; private set; }

        public OutlineRegionsChangedEventArgs(OutlineRegionCollection regions, ITextRange changedRange)
        {
            this.Regions = regions;
            this.ChangedRange = changedRange;
        }
    }
}
