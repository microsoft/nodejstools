/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of 
 * the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.IO;

namespace VsctToXliff
{
    internal static class Utilities
    {
        public static string EnsureRootPath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            return new FileInfo(Path.Combine(Environment.CurrentDirectory, path)).FullName;
        }

        /// <returns>The filename without extension or locale.</returns>
        public static string VsctFileNameWithoutExtension(string fileName)
        {
            // assume filename have the following structure: <filename>.<locale>.vsct
            var file = Path.GetFileName(fileName);

            return file.Substring(0, file.IndexOf('.'));
        }
    }
}
