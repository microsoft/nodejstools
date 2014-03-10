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

namespace Microsoft.NodejsTools.Debugger {
    /// <summary>
    /// Stores line and column position in the file.
    /// </summary>
    sealed class FilePosition {
        private readonly int _column;
        private readonly string _fileName;
        private readonly int _line;

        public FilePosition(string fileName, int line, int column) {
            _fileName = fileName;
            _line = line;
            _column = column;
        }

        /// <summary>
        /// Gets a file name.
        /// </summary>
        public string FileName {
            get { return _fileName; }
        }

        /// <summary>
        /// Gets a line number.
        /// </summary>
        public int Line {
            get { return _line; }
        }

        /// <summary>
        /// Gets a column number.
        /// </summary>
        public int Column {
            get { return _column; }
        }
    }
}