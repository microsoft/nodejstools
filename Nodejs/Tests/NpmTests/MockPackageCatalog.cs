// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;

namespace NpmTests
{
    public class MockPackageCatalog : IPackageCatalog
    {
        private IDictionary<string, IPackage> _byName = new Dictionary<string, IPackage>();
        private IList<IPackage> _results;

        public MockPackageCatalog(IList<IPackage> results)
        {
            _results = results;
            LastRefreshed = DateTime.Now;

            foreach (var package in results)
            {
                _byName[package.Name] = package;
            }
        }

        public DateTime LastRefreshed { get; private set; }

        public Task<IEnumerable<IPackage>> GetCatalogPackagesAsync(string filterText, Uri registryUrl = null)
        {
            return Task.FromResult(_results.AsEnumerable());
        }

        public IPackage this[string name]
        {
            get
            {
                IPackage match;
                _byName.TryGetValue(name, out match);
                return match;
            }
        }

        public long? ResultsCount
        {
            get { return _results.LongCount(); }
        }
    }
}

