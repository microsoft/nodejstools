// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace Microsoft.NodejsTools.TestAdapter
{
    [Export(typeof(IRunFromContextFileExtensions))]
    internal class RunFromContextFileExtensions : IRunFromContextFileExtensions
    {
        #region IRunFromContextFileExtensions Members

        private static readonly IEnumerable<string> fileTypes = new[] { NodejsConstants.JavaScriptExtension };

        public IEnumerable<string> FileTypes => fileTypes;

        #endregion
    }
}
