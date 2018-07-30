// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Resources;
using System.Threading;
using CommonSR = Microsoft.VisualStudioTools.Project.SR;

namespace Microsoft.NodejsTools.Project
{
    internal class SR : CommonSR
    {
        internal const string NodejsToolsForVisualStudio = "NodejsToolsForVisualStudio";

        internal const string NodeExeDoesntExist = "NodeExeDoesntExist";
        internal const string NodejsNotInstalled = "NodejsNotInstalled";
        internal const string NpmIsInstalling = "NpmIsInstalling";

        internal const string TestFramework = "TestFramework";

        private static readonly Lazy<ResourceManager> _manager = new Lazy<ResourceManager>(
            () => new System.Resources.ResourceManager("Microsoft.NodejsTools.Resources", typeof(SR).Assembly),
            LazyThreadSafetyMode.ExecutionAndPublication
        );

        private static ResourceManager Manager => _manager.Value;

        internal static new string GetString(string value, params object[] args)
        {
            return GetStringInternal(Manager, value, args) ?? CommonSR.GetString(value, args);
        }

        internal static string ProductName => GetString(NodejsToolsForVisualStudio);
    }
}
