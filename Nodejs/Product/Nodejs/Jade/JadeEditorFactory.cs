/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Jade {
    /// <summary>
    /// Factory for creating code editor.
    /// </summary>
    /// <remarks>
    /// While currently empty, editor factory has to be unique per language.
    /// </remarks>
    [Guid(Guids.JadeEditorFactoryString)]
    public class JadeEditorFactory : CommonEditorFactory {
        public JadeEditorFactory(Package package) : base(package) { }

        public JadeEditorFactory(Package package, bool promptForEncoding) : base(package, promptForEncoding) { }

        protected override void InitializeLanguageService(IVsTextLines textLines) {
            InitializeLanguageService(textLines, typeof(JadeLanguageInfo).GUID);
        }
    }
}
