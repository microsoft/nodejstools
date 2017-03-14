// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    [Export(typeof(IGlyphService))]
    internal class MockGlyphService : IGlyphService
    {
        public ImageSource GetGlyph(StandardGlyphGroup group, StandardGlyphItem item)
        {
            return null;
        }
    }
}

