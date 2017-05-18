// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Debugger
{
    /// <summary>
    /// Stores line and column position in the file.
    /// </summary>
    internal sealed class FilePosition
    {
        public readonly int Column;
        public readonly string FileName;
        public readonly int Line;

        public FilePosition(string fileName, int line, int column)
        {
            this.FileName = fileName;
            this.Line = line;
            this.Column = column;
        }
    }
}
