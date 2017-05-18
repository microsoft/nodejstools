// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Jade
{
    /// <summary>
    /// Factory for creating code editor.
    /// </summary>
    /// <remarks>
    /// While currently empty, editor factory has to be unique per language.
    /// </remarks>
    [Guid(Guids.JadeEditorFactoryString)]
    public class JadeEditorFactory : CommonEditorFactory
    {
        public JadeEditorFactory(Package package) : base(package) { }

        public JadeEditorFactory(Package package, bool promptForEncoding) : base(package, promptForEncoding) { }

        protected override void InitializeLanguageService(IVsTextLines textLines)
        {
            InitializeLanguageService(textLines, typeof(JadeLanguageInfo).GUID);
        }
    }
}
