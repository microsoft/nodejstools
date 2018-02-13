// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudioTools.Navigation
{
    /// <summary>
    /// Class storing the data about a parsing task on a language module.
    /// A module in dynamic languages is a source file, so here we use the file name to
    /// identify it.
    /// </summary>
    public sealed class LibraryTask
    {
        public LibraryTask(string fileName, ITextBuffer textBuffer, ModuleId moduleId)
        {
            this.FileName = fileName;
            this.TextBuffer = textBuffer;
            this.ModuleId = moduleId;
        }

        public string FileName { get; }

        public ModuleId ModuleId { get; set; }

        public ITextBuffer TextBuffer { get; }
    }
}
