// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal abstract class AbstractNodeModules : INodeModules
    {
        protected readonly SortedSet<IPackage> SortedPackages = new SortedSet<IPackage>(PackageComparer.Instance);
        private readonly IDictionary<string, IPackage> packagesByName = new Dictionary<string, IPackage>();

        protected virtual void AddModule(IPackage package)
        {
            if (package.Name != null && !this.packagesByName.ContainsKey(package.Name))
            {
                this.SortedPackages.Add(package);
                this.packagesByName[package.Name] = package;
            }
        }

        public int Count => this.SortedPackages.Count;

        public IPackage this[string name]
        {
            get
            {
                this.packagesByName.TryGetValue(name, out var pkg);
                return pkg;
            }
        }

        public bool Contains(string name)
        {
            return this[name] != null;
        }

        public bool HasMissingModules
        {
            get
            {
                foreach (var pkg in this)
                {
                    if (pkg.IsMissing)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public IEnumerator<IPackage> GetEnumerator()
        {
            return this.SortedPackages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract int GetDepth(string filepath);
    }
}
