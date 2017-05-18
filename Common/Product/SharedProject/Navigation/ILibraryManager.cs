// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudioTools.Navigation
{
    /// <summary>
    /// This interface defines the service that finds current language files inside a hierarchy
    /// and builds information to expose to the class view or object browser.
    /// </summary>    
    internal interface ILibraryManager
    {
        void RegisterHierarchy(IVsHierarchy hierarchy);
        void UnregisterHierarchy(IVsHierarchy hierarchy);
        void RegisterLineChangeHandler(uint document, TextLineChangeEvent lineChanged, Action<IVsTextLines> onIdle);
    }
    internal delegate void TextLineChangeEvent(object sender, TextLineChange[] changes, int last);
}

