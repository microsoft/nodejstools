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

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm.SPI{
    internal abstract class AbstractNodeModules : INodeModules{
        protected readonly List<IPackage> _packagesSorted = new List<IPackage>();
        private readonly IDictionary<string, IPackage> _packagesByName = new Dictionary<string, IPackage>();

        protected virtual void AddModule(IPackage package){
            _packagesSorted.Add(package);
            _packagesByName[package.Name] = package;
        }

        public int Count{
            get { return _packagesSorted.Count; }
        }

        public IPackage this[int index]{
            get { return _packagesSorted[index]; }
        }

        public IPackage this[string name]{
            get{
                IPackage pkg;
                _packagesByName.TryGetValue(name, out pkg);
                return pkg;
            }
        }

        public bool Contains(string name){
            return this[name] != null;
        }

        public IEnumerator<IPackage> GetEnumerator(){
            return _packagesSorted.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }
    }
}