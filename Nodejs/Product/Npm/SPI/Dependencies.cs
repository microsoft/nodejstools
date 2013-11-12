using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class Dependencies : IDependencies
    {
        private JObject _package;
        private string [] _dependencyPropertyNames;

        public Dependencies(JObject package, params string [] dependencyPropertyNames)
        {
            _package = package;
            _dependencyPropertyNames = dependencyPropertyNames;
        }

        private IEnumerable< JObject > GetDependenciesProperties()
        {
            foreach (var propertyName in _dependencyPropertyNames)
            {
                var property = _package[propertyName] as JObject;
                if (null != property)
                {
                    yield return property;
                }
            }
        }

        public IEnumerator<IDependency> GetEnumerator()
        {
            var dependencyProps = GetDependenciesProperties();
            foreach (var dependencies in dependencyProps)
            {
                var properties = null == dependencies ? new List<JProperty>() : dependencies.Properties();
                foreach (var property in properties)
                {
                    yield return new Dependency(property.Name, property.Value.Value<string>());
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.Count();
            }
        }

        public IDependency this[string name]
        {
            get
            {
                foreach (var dependencies in GetDependenciesProperties())
                {
                    var property = dependencies[name];
                    if (null != property)
                    {
                        return new Dependency(name, property.Value<string>());
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
