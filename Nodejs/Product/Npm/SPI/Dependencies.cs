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
        private string m_DependencyPropertyName;

        public Dependencies(JObject package, string dependencyPropertyName)
        {
            m_Package = package;
            m_DependencyPropertyName = dependencyPropertyName;
        }

        private JObject GetDependenciesProperty()
        {
            return m_Package[m_DependencyPropertyName] as JObject;
        }

        public IEnumerator<IDependency> GetEnumerator()
        {
            var dependencies = GetDependenciesProperty();
            var properties = null == dependencies ? new List<JProperty>() : dependencies.Properties();
            foreach (var property in properties)
            {
                yield return new Dependency(property.Name, property.Value.Value<string>());
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
                var dependencies = GetDependenciesProperty();
                return null == dependencies ? 0 : dependencies.Count;
            }
        }

        public IDependency this[string name]
        {
            get
            {
                var dependencies = GetDependenciesProperty();
                if (null == dependencies)
                {
                    return null;
                }

                var property = dependencies[name];
                return null == property ? null : new Dependency(name, property.Value<string>());
            }
        }
    }
}
