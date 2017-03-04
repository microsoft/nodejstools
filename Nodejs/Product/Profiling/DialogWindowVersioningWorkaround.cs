// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.PlatformUI;

namespace Microsoft.NodejsTools.Profiling
{
    /// <summary>
    /// Works around an issue w/ DialogWindow and targetting multiple versions of VS.
    /// 
    /// Because the Microsoft.VisualStudio.Shell.version.0 assembly changes names
    /// we cannot refer to both v10 and v11 versions from within the same XAML file.
    /// Instead we use this subclass defined in our assembly.
    /// </summary>
    public class DialogWindowVersioningWorkaround : DialogWindow
    {
    }
}

