// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using System.IO;

namespace Microsoft.NodejsTools.Debugger
{
    internal class NodeModule
    {
        private readonly string _fileName;
        private readonly int _id;
        private readonly string _javaScriptFileName;

        public NodeModule(int id, string fileName) : this(id, fileName, fileName)
        {
        }

        public NodeModule(int id, string fileName, string javaScriptFileName)
        {
            Debug.Assert(fileName != null);

            this._id = id;
            this._fileName = fileName;
            this._javaScriptFileName = javaScriptFileName;
        }

        public int Id => this._id;
        public string Name
        {
            get
            {
                if (this._fileName.IndexOfAny(Path.GetInvalidPathChars()) == -1)
                {
                    return Path.GetFileName(this._fileName);
                }
                return this._fileName;
            }
        }

        public string JavaScriptFileName => this._javaScriptFileName;
        public string FileName => this._fileName;
        public string Source { get; set; }

        public bool BuiltIn =>
                // No directory separator characters implies builtin
                (this._fileName.IndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) == -1);

        public object Document { get; set; }
    }
}
