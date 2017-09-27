// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class RootPackage : IRootPackage
    {
        public RootPackage(
            string fullPathToRootDirectory,
            bool showMissingDevOptionalSubPackages,
            Dictionary<string, ModuleInfo> allModules = null,
            int depth = 0,
            int maxDepth = 1)
        {
            this.Path = fullPathToRootDirectory;
            var packageJsonFile = System.IO.Path.Combine(fullPathToRootDirectory, "package.json");
            try
            {
                if (packageJsonFile.Length < 260)
                {
                    this.PackageJson = PackageJsonFactory.Create(new DirectoryPackageJsonSource(fullPathToRootDirectory));
                }
            }
            catch (RuntimeBinderException rbe)
            {
                throw new PackageJsonException(
                    string.Format(CultureInfo.CurrentCulture, @"Error processing package.json at '{0}'. The file was successfully read, and may be valid JSON, but the objects may not match the expected form for a package.json file.

The following error was reported:

{1}",
                    packageJsonFile,
                    rbe.Message),
                    rbe);
            }

            try
            {
                this.Modules = new NodeModules(this, showMissingDevOptionalSubPackages, allModules, depth, maxDepth);
            }
            catch (PathTooLongException)
            {
                // otherwise we fail to create it completely...
            }

            if (this.PackageJson != null)
            {
                this.Name = this.PackageJson.Name;
            }
            else
            {
                // this is the root package so the full folder name after node_nodules is the name
                // of the package
                var index = this.Path.IndexOf(NodejsConstants.NodeModulesFolderWithSeparators, StringComparison.OrdinalIgnoreCase);

                Debug.Assert(index > -1, "Failed to find the node_modules folder.");

                var name = this.Path.Substring(index + NodejsConstants.NodeModulesFolderWithSeparators.Length);

                this.Name = name;
            }
        }

        public IPackageJson PackageJson { get; }

        public INodeModules Modules { get; }

        public string Path { get; }

        public string Name { get; }

        public IEnumerable<string> Homepages => this.PackageJson?.Homepages ?? Enumerable.Empty<string>();

        public bool HasPackageJson => this.PackageJson != null;

        public SemverVersion Version => this.PackageJson?.Version ?? SemverVersion.UnknownVersion;

        public IPerson Author => this.PackageJson?.Author;

        public string Description => this.PackageJson?.Description;
    }
}
