// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Microsoft.NodejsTools.TestAdapter
{
    internal sealed class AssemblyResolver : IDisposable
    {
        public AssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        internal static string GetVSInstallDir()
        {

            var vsTestFrameworkAssembly = typeof(ITestExecutor).Assembly;
            var testAdapterPath = vsTestFrameworkAssembly.Location;

            // C:\Program Files (x86)\Microsoft Visual Studio\2017\VSUJSLT\Common7\IDE\CommonExtensions\Microsoft\TestWindow\Microsoft.VisualStudio.TestPlatform.ObjectModel.dll
            var indexOfCommon7Ide = testAdapterPath.IndexOf("common7", StringComparison.OrdinalIgnoreCase);
            string vsInstallDir = testAdapterPath.Substring(0, indexOfCommon7Ide);

            return vsInstallDir;
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Use the setup API to find the VS install Dir, then build paths to the Private and Public Assemblies folders
            var installPath = GetVSInstallDir();
            var ideFolder = Path.Combine(installPath, "Common7", "IDE");
            var paths = new[] {
                Path.Combine(ideFolder, "PrivateAssemblies"),
                Path.Combine(ideFolder, "PublicAssemblies"),
                Path.Combine(installPath, "MSBuild","15.0","Bin"),
                Path.Combine(ideFolder, "CommonExtensions","Microsoft","WebClient","Project System") };

            // This is what comes in for args.Name, but we really just want the dll file name:
            // "Microsoft.Build, Version=15.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
            var resolveTargetAssemblyName = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";

            foreach (var path in paths)
            {
                // Check under privateAssemblies
                if (ResolveAssemblyPath(path, resolveTargetAssemblyName, out var resolvedAssembly))
                {
                    return resolvedAssembly;
                }
            }
            return null;
        }

        private static bool ResolveAssemblyPath(string possibleDirectory, string assemblyName, out Assembly resolvedAssembly)
        {
            resolvedAssembly = null;
            var candidateAssemblyPath = Path.Combine(possibleDirectory, assemblyName);
            if (File.Exists(candidateAssemblyPath))
            {
                resolvedAssembly = Assembly.LoadFrom(candidateAssemblyPath);
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
        }
    }
}
