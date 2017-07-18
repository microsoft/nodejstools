// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal abstract class AbstractNodeModules : INodeModules
    {
        protected readonly List<IPackage> _packagesSorted = new List<IPackage>();
        private readonly IDictionary<string, IPackage> _packagesByName = new Dictionary<string, IPackage>();

        protected virtual void AddModule(IPackage package)
        {
            if (package.Name != null && !this._packagesByName.ContainsKey(package.Name))
            {
                this._packagesSorted.Add(package);
                this._packagesByName[package.Name] = package;
            }
        }

        public int Count
        {
            get { return this._packagesSorted.Count; }
        }

        public IPackage this[int index]
        {
            get { return this._packagesSorted[index]; }
        }

        public IPackage this[string name]
        {
            get
            {
                IPackage pkg;
                this._packagesByName.TryGetValue(name, out pkg);
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
                foreach (IPackage pkg in this)
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
            return this._packagesSorted.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract int GetDepth(string filepath);
    }
}
