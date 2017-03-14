// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    [Export(typeof(IBufferGraphFactoryService))]
    internal class MockBufferGraphFactoryService : IBufferGraphFactoryService
    {
        public IBufferGraph CreateBufferGraph(VisualStudio.Text.ITextBuffer textBuffer)
        {
            throw new NotImplementedException();
        }
    }
}

