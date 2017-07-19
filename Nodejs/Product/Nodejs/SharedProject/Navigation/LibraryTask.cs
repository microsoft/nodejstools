// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudioTools.Navigation
{
    /// <summary>
    /// Class storing the data about a parsing task on a language module.
    /// A module in dynamic languages is a source file, so here we use the file name to
    /// identify it.
    /// </summary>
    public class LibraryTask
    {
        private string _fileName;
        private ITextBuffer _textBuffer;
        private ModuleId _moduleId;

        public LibraryTask(string fileName, ITextBuffer textBuffer, ModuleId moduleId)
        {
            this._fileName = fileName;
            this._textBuffer = textBuffer;
            this._moduleId = moduleId;
        }

        public string FileName => this._fileName;
        public ModuleId ModuleID
        {
            get { return this._moduleId; }
            set { this._moduleId = value; }
        }

        public ITextBuffer TextBuffer => this._textBuffer;
    }
}
