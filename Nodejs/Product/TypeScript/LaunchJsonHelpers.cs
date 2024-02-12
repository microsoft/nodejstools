// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudioTools;
namespace Microsoft.NodejsTools.TypeScript
{
    internal static class LaunchJsonHelpers
    {
        internal static bool IsLaunchJsonFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            return StringComparer.OrdinalIgnoreCase.Equals(fileName, NodejsConstants.LaunchJsonFile);
        }
    }
}
