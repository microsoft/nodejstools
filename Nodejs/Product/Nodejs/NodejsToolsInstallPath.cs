// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;

namespace Microsoft.NodejsTools
{
    public static class NodejsToolsInstallPath
    {
        private static string GetFromAssembly(Assembly assembly, string filename)
        {
            var path = Path.Combine(
                Path.GetDirectoryName(assembly.Location),
                filename);
            if (File.Exists(path))
            {
                return path;
            }
            return string.Empty;
        }

        public static string GetFile(string filename)
        {
            var path = GetFromAssembly(typeof(NodejsToolsInstallPath).Assembly, filename);
            if (!string.IsNullOrEmpty(path))
            {
                return path;
            }

            throw new InvalidOperationException("Unable to determine Node.js Tools installation path");
        }
    }
}

