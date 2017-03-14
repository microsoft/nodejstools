// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    /// <summary>
    /// Stores information about registered language services.
    /// </summary>
    internal class LanguageServiceInfo
    {
        public readonly ProvideLanguageServiceAttribute Attribute;

        public LanguageServiceInfo(ProvideLanguageServiceAttribute attr)
        {
            Attribute = attr;
        }
    }
}

