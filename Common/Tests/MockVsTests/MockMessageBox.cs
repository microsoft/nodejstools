// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.VisualStudioTools.MockVsTests
{
    internal class MockMessageBox : MockDialog
    {
        public readonly string Text;

        public MockMessageBox(MockVs vs, string title, string text) : base(vs, title)
        {
            Text = text;
        }
    }
}

