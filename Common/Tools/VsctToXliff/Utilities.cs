// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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

