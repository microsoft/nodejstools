// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class Dependencies : IDependencies
    {
        private IList<Dependency> _dependencyProperties;

        public Dependencies(JObject package, params string[] dependencyPropertyNames)
        {
            this._dependencyProperties = new List<Dependency>();
            foreach (var propertyName in dependencyPropertyNames)
            {
                var dependencies = package[propertyName] as JObject;
                if (dependencies != null)
                {
                    foreach (var property in dependencies.Properties())
                    {
                        if (property.Value.Type == JTokenType.String)
                        {
                            this._dependencyProperties.Add(new Dependency(property.Name, property.Value.Value<string>()));
                        }
                    }
                }
            }
        }

        public IEnumerator<IDependency> GetEnumerator()
        {
            return this._dependencyProperties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return this.Count(); }
        }

        public IDependency this[string name]
        {
            get
            {
                foreach (var dependeny in this._dependencyProperties)
                {
                    if (dependeny.Name == name)
                    {
                        return dependeny;
                    }
                }
                return null;
            }
        }

        public bool Contains(string name)
        {
            return this[name] != null;
        }
    }
}

