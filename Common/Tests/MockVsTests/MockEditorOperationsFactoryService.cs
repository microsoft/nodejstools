// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Operations;
using TestUtilities.Mocks;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    [Export(typeof(IEditorOperationsFactoryService))]
    internal class MockEditorOperationsFactoryService : IEditorOperationsFactoryService
    {
        public IEditorOperations GetEditorOperations(VisualStudio.Text.Editor.ITextView textView)
        {
            return new MockEditorOperations((MockTextView)textView);
        }
    }
}

