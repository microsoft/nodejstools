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
using Microsoft.NodejsTools.Npm;

namespace NpmTests {
    internal class MockPackageCatalog : IPackageCatalog {
        private IDictionary<string, IPackage> _byName = new Dictionary<string, IPackage>();
 
        public MockPackageCatalog(IList<IPackage> results) {
            Results = results;
            LastRefreshed = DateTime.Now;

            foreach (var package in results) {
                _byName[package.Name] = package;
            }
        }
        public IList<IPackage> Results { get; private set; }
        public DateTime LastRefreshed { get; private set; }

        public IPackage this[string name] {
            get {
                IPackage match;
                _byName.TryGetValue(name, out match);
                return match;
            }
        }
    }
}
