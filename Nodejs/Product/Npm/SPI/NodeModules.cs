// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class NodeModules : AbstractNodeModules
    {
        private readonly Dictionary<string, ModuleInfo> _allModules;
        private static readonly string[] _ignoredDirectories = { @"\.bin", @"\.staging" };

        public NodeModules(IRootPackage parent, bool showMissingDevOptionalSubPackages, Dictionary<string, ModuleInfo> allModulesToDepth = null, int depth = 0, int maxDepth = 1)
        {
            if (depth >= maxDepth)
            {
                return;
            }

            var modulesBase = Path.Combine(parent.Path, NodejsConstants.NodeModulesFolder);

            this._allModules = allModulesToDepth ?? new Dictionary<string, ModuleInfo>();

            // This is the first time NodeModules is being created.
            // Iterate through directories to add everything that's known to be top-level.
            if (depth == 0)
            {
                Debug.Assert(this._allModules.Count == 0, "Depth is 0, but top-level modules have already been added.");

                // Go through every directory in node_modules, and see if it's required as a top-level dependency
                foreach (var topLevelDependency in GetTopLevelPackageDirectories(modulesBase))
                {
                    var moduleDir = topLevelDependency.Key;
                    var packageJson = topLevelDependency.Value;
                    if (packageJson.RequiredBy.Any())
                    {
                        // All dependencies in npm v3 will have at least one element present in _requiredBy.
                        // _requiredBy dependencies that begin with hash characters represent top-level dependencies
                        foreach (var requiredBy in packageJson.RequiredBy)
                        {
                            if (requiredBy.StartsWith("#", StringComparison.Ordinal) || requiredBy == "/")
                            {
                                AddTopLevelModule(parent, showMissingDevOptionalSubPackages, moduleDir, depth, maxDepth);
                                break;
                            }
                        }
                    }
                    else
                    {
                        // This dependency is a top-level dependency not added by npm v3
                        AddTopLevelModule(parent, showMissingDevOptionalSubPackages, moduleDir, depth, maxDepth);
                    }
                }
            }

            if (modulesBase.Length < NativeMethods.MAX_FOLDER_PATH && parent.HasPackageJson)
            {
                // Iterate through all dependencies in the root package.json
                // Otherwise, only iterate through "dependencies" because iterating through optional, bundle, etc. dependencies
                // becomes unmanageable when they are already installed at the root of the project, and the performance impact
                // typically isn't worth the value add. 
                var dependencies = depth == 0 ? parent.PackageJson.AllDependencies : parent.PackageJson.Dependencies;
                foreach (var dependency in dependencies)
                {
                    var moduleDir = modulesBase;

                    // try to find folder by recursing up tree
                    do
                    {
                        moduleDir = Path.Combine(moduleDir, dependency.Name);
                        if (AddModuleIfNotExists(parent, moduleDir, showMissingDevOptionalSubPackages, depth, maxDepth, dependency))
                        {
                            break;
                        }

                        var parentNodeModulesIndex = moduleDir.LastIndexOf(NodejsConstants.NodeModulesFolder, Math.Max(0, moduleDir.Length - NodejsConstants.NodeModulesFolder.Length - dependency.Name.Length - 1), StringComparison.Ordinal);
                        moduleDir = moduleDir.Substring(0, parentNodeModulesIndex + NodejsConstants.NodeModulesFolder.Length);
                    } while (moduleDir.Contains(NodejsConstants.NodeModulesFolder));
                }
            }

            this._packagesSorted.Sort(new PackageComparer());
        }

        private void AddTopLevelModule(IRootPackage parent, bool showMissingDevOptionalSubPackages, string moduleDir, int depth, int maxDepth)
        {
            Debug.Assert(depth == 0, "Depth should be 0 when adding a top level dependency");
            AddModuleIfNotExists(parent, moduleDir, showMissingDevOptionalSubPackages, depth, maxDepth);
        }

        private bool AddModuleIfNotExists(IRootPackage parent, string moduleDir, bool showMissingDevOptionalSubPackages, int depth, int maxDepth, IDependency dependency = null)
        {
            depth++;

            this._allModules.TryGetValue(moduleDir, out var moduleInfo);

            if (moduleInfo != null)
            {
                // Update module information if the module already exists.
                if (moduleInfo.Depth > depth)
                {
                    moduleInfo.Depth = depth;
                }

                if (dependency != null)
                {
                    var existingPackage = this[dependency.Name] as Package;
                    if (existingPackage != null)
                    {
                        existingPackage.RequestedVersionRange = dependency.VersionRangeText;
                    }
                }
            }
            else if (Directory.Exists(moduleDir) || depth == 1)
            {
                // Top-level modules are always added so we can include missing modules.
                moduleInfo = new ModuleInfo(depth);
                this._allModules.Add(moduleDir, moduleInfo);
            }
            else
            {
                // The module directory wasn't found.
                return false;
            }

            IPackage package = moduleInfo.Package;

            if (package == null || depth == 1 || !moduleInfo.RequiredBy.Contains(parent.Path))
            {
                // Create a dummy value for the current package to prevent infinite loops
                moduleInfo.Package = new PackageProxy();

                moduleInfo.RequiredBy.Add(parent.Path);

                var pkg = new Package(parent, moduleDir, showMissingDevOptionalSubPackages, this._allModules, depth, maxDepth);
                if (dependency != null)
                {
                    pkg.RequestedVersionRange = dependency.VersionRangeText;
                }

                package = moduleInfo.Package = pkg;
            }

            if (parent as IPackage == null || !package.IsMissing || showMissingDevOptionalSubPackages)
            {
                AddModule(package);
            }

            return true;
        }

        public override int GetDepth(string filepath)
        {
            var lastNodeModules = filepath.LastIndexOf(NodejsConstants.NodeModulesFolder + "\\", StringComparison.Ordinal);
            var directoryToSearch = filepath.IndexOf("\\", lastNodeModules + NodejsConstants.NodeModulesFolder.Length + 1, StringComparison.Ordinal);
            var directorySubString = directoryToSearch == -1 ? filepath : filepath.Substring(0, directoryToSearch);

            this._allModules.TryGetValue(directorySubString, out var value);

            var depth = value != null ? value.Depth : 0;
            Debug.WriteLine("Module Depth: {0} [{1}]", filepath, depth);

            return depth;
        }

        private static IEnumerable<KeyValuePair<string, IPackageJson>> GetTopLevelPackageDirectories(string modulesBase)
        {
            var topLevelDirectories = Enumerable.Empty<string>();
            if (Directory.Exists(modulesBase))
            {
                try
                {
                    topLevelDirectories = Directory.EnumerateDirectories(modulesBase);
                }
                catch (IOException)
                {
                    // We want to handle DirectoryNotFound, DriveNotFound, PathTooLong
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            // Go through every directory in node_modules, and see if it's required as a top-level dependency
            foreach (var moduleDir in topLevelDirectories)
            {
                if (moduleDir.Length < NativeMethods.MAX_FOLDER_PATH && !_ignoredDirectories.Any(toIgnore => moduleDir.EndsWith(toIgnore, StringComparison.Ordinal)))
                {
                    IPackageJson json = null;
                    try
                    {
                        json = PackageJsonFactory.Create(new DirectoryPackageJsonSource(moduleDir));
                    }
                    catch (PackageJsonException)
                    {
                        // Fail gracefully if there was an error parsing the package.json
                        Debug.Fail("Failed to parse package.json in {0}", moduleDir);
                    }
                    if (json != null)
                    {
                        yield return new KeyValuePair<string, IPackageJson>(moduleDir, json);
                    }
                }
            }
        }
    }

    internal class ModuleInfo
    {
        public int Depth { get; set; }

        public IPackage Package { get; set; }

        public IList<string> RequiredBy { get; set; }

        internal ModuleInfo(int depth)
        {
            this.Depth = depth;
            this.RequiredBy = new List<string>();
        }
    }
}
