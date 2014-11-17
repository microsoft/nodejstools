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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.NodejsTools.Npm;

namespace NpmTests {
    public class MockPackageCatalog : IPackageCatalog {
        private IDictionary<string, IPackage> _byName = new Dictionary<string, IPackage>();
        private IList<IPackage> _results; 
 
        public MockPackageCatalog(IList<IPackage> results) {
            _results = results;
            LastRefreshed = DateTime.Now;

            foreach (var package in results) {
                _byName[package.Name] = package;
            }
        }

        public DateTime LastRefreshed { get; private set; }

        public IEnumerable<IPackage> GetCatalogPackages(string filterText) {
            return _results;
        }

        public IPackage this[string name] {
            get {
                IPackage match;
                _byName.TryGetValue(name, out match);
                return match;
            }
        }

        public long? ResultsCount {
            get { return _results.LongCount(); }
        }
    }
}
