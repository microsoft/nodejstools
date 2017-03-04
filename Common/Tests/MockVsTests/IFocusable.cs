// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.


namespace Microsoft.VisualStudioTools.MockVsTests
{
    /// <summary>
    /// Implemented by mock VS objects which can gain and lose focus.
    /// 
    /// Only one item in mock VS will have focus at a time, and the
    /// current item is tracked by MockVs.
    /// </summary>
    internal interface IFocusable
    {
        void GetFocus();
        void LostFocus();
    }
}

