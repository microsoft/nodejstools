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
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI{
    internal class Dependencies : IDependencies{
        private JObject _package;
        private string[] _dependencyPropertyNames;

        public Dependencies(JObject package, params string[] dependencyPropertyNames){
            _package = package;
            _dependencyPropertyNames = dependencyPropertyNames;
        }

        private IEnumerable<JObject> GetDependenciesProperties(){
            foreach (var propertyName in _dependencyPropertyNames){
                var property = _package[propertyName] as JObject;
                if (null != property){
                    yield return property;
                }
            }
        }

        public IEnumerator<IDependency> GetEnumerator(){
            var dependencyProps = GetDependenciesProperties();
            foreach (var dependencies in dependencyProps){
                var properties = null == dependencies ? new List<JProperty>() : dependencies.Properties();
                foreach (var property in properties){
                    yield return new Dependency(property.Name, property.Value.Value<string>());
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }

        public int Count{
            get { return this.Count(); }
        }

        public IDependency this[string name]{
            get{
                foreach (var dependencies in GetDependenciesProperties()){
                    var property = dependencies[name];
                    if (null != property){
                        return new Dependency(name, property.Value<string>());
                    }
                }
                return null;
            }
        }

        public bool Contains(string name){
            return this[name] != null;
        }
    }
}