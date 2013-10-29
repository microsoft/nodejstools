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
        private JObject m_Package;
        private string [] m_DependencyPropertyNames;

        public Dependencies(JObject package, params string [] dependencyPropertyNames)
        {
            m_Package = package;
            m_DependencyPropertyNames = dependencyPropertyNames;
        }

        private IEnumerable< JObject > GetDependenciesProperties()
        {
            foreach (var propertyName in m_DependencyPropertyNames)
            {
                var property = m_Package[propertyName] as JObject;
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
