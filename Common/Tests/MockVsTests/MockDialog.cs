// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using TestUtilities;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    internal class MockDialog
    {
        public readonly string Title;
        public readonly MockVs Vs;
        public int DialogResult = 0;
        private AutoResetEvent _dismiss = new AutoResetEvent(false);

        public MockDialog(MockVs vs, string title)
        {
            Title = title;
            Vs = vs;
        }

        public virtual void Type(string text)
        {
            switch (text)
            {
                case "\r":
                    Close((int)MessageBoxButton.Ok);
                    break;
                default:
                    throw new NotImplementedException("Unhandled dialog text: " + text);
            }
        }

        public virtual void Run()
        {
            Vs.RunMessageLoop(_dismiss);
        }

        public virtual void Close(int result)
        {
            DialogResult = result;
            _dismiss.Set();
        }
    }
}

