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

using System.IO;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NodeModules : AbstractNodeModules {
        public NodeModules(IRootPackage parent, bool showMissingDevOptionalSubPackages) {
            var modulesBase = Path.Combine(parent.Path, "node_modules");
            if (Directory.Exists(modulesBase)) {
                var bin = string.Format("{0}.bin", Path.DirectorySeparatorChar);
                foreach (var moduleDir in Directory.EnumerateDirectories(modulesBase)) {
                    if (!moduleDir.EndsWith(bin)) {
                        AddModule(new Package(parent, moduleDir, showMissingDevOptionalSubPackages));
                    }
                }
            }

            var parentPackageJson = parent.PackageJson;
            if (null != parentPackageJson) {
                foreach (var dependency in parentPackageJson.AllDependencies) {
                    Package module = null;
                    if (!Contains(dependency.Name)) {
                        module = new Package(
                            parent,
                            Path.Combine(modulesBase, dependency.Name),
                            showMissingDevOptionalSubPackages);
                        if (parent as IPackage == null || !module.IsMissing || showMissingDevOptionalSubPackages) {
                            AddModule(module);
                        }
                    } else {
                        module = this[dependency.Name] as Package;
                    }

                    if (null != module) {
                        module.RequestedVersionRange = dependency.VersionRangeText;
                    }
                }
            }

            _packagesSorted.Sort(new PackageComparer());
        }
    }
}