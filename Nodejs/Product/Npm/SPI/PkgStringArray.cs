//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal abstract class PkgStringArray : IPkgStringArray
    {
        private IList<string> elements;

        protected PkgStringArray(JObject package, params string[] arrayPropertyNames)
        {
            var token = GetArrayProperty(package, arrayPropertyNames);
            if (token == null)
            {
                elements = new List<string>();
            }
            else
            {
                switch (token.Type)
                {
                    case JTokenType.String:
                        elements = new[] { token.Value<string>() };
                        break;

                    case JTokenType.Array:
                        elements = (token as JArray).Select(value => value.Value<string>()).ToList();
                        break;

                    default:
                        elements = new List<string>();
                        break;
                }
            }
        }

        private static JToken GetArrayProperty(JObject package, string[] arrayPropertyNames)
        {
            foreach (var name in arrayPropertyNames)
            {
                var array = package[name] as JToken;
                if (null != array)
                {
                    return array;
                }
            }
            return null;
        }

        public int Count
        {
            get
            {
                return elements.Count;
            }
        }

        public string this[int index]
        {
            get
            {
                if (Count <= 0)
                {
                    throw new IndexOutOfRangeException(
                        "Cannot retrieve item from empty package.json string array.");
                }

                if (index > Count)
                {
                    throw new IndexOutOfRangeException(
                        string.Format(CultureInfo.CurrentCulture,
                            "Cannot retrieve value from index '{0}' in a package.json string array containing only 1 item.",
                            index));
                }
                return elements[index];
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}