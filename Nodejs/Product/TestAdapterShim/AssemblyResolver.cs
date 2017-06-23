// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;

namespace Microsoft.NodejsTools.TestAdapter
{
    internal sealed class AssemblyResolver : IDisposable
    {
        public AssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Use the setup API to find the VS install Dir, then build paths to the Private and Public Assemblies folders
            string installPath = @"C:\vs2017";
            string ideFolder = Path.Combine(installPath, "Common7\\IDE");
            var paths = new[] {
                Path.Combine(ideFolder, "PrivateAssemblies"),
                Path.Combine(ideFolder, "PublicAssemblies"),
                Path.Combine(installPath, "MSBuild\\15.0\\Bin"),
                Path.Combine(ideFolder, "CommonExtensions\\Microsoft\\WebClient\\Project System") };

            // This is what comes in for args.Name, but we really just want the dll file name:
            // "Microsoft.Build, Version=15.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
            string resolveTargetAssemblyName = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";

            foreach (var path in paths)
            {
                // Check under privateAssemblies
                if (AssemblyResolver.ResolveAssemblyPath(path, resolveTargetAssemblyName, out var resolvedAssembly))
                {
                    return resolvedAssembly;
                }
            }
            return null;
        }

        private static bool ResolveAssemblyPath(string possibleDirectory, string assemblyName, out Assembly resolvedAssembly)
        {
            resolvedAssembly = null;
            string candidateAssemblyPath = Path.Combine(possibleDirectory, assemblyName);
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
