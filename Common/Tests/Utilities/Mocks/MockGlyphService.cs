// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TestUtilities.Mocks
{
    public class MockGlyphService : IGlyphService
    {
        #region IGlyphService Members

        public ImageSource GetGlyph(StandardGlyphGroup group, StandardGlyphItem item)
        {
            return new DrawingImage();
        }

        #endregion
    }
}

