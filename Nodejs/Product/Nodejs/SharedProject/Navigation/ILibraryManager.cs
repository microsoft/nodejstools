// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudioTools.Navigation
{
    internal delegate void TextLineChangeEvent(object sender, TextLineChange[] changes, int last);
}
