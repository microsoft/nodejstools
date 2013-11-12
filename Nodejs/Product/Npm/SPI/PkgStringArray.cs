using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI{
    internal abstract class PkgStringArray : IPkgStringArray{
        private readonly JObject _package;
        private readonly string[] arrayPropertyNames;

        protected PkgStringArray(JObject package, params string[] arrayPropertyNames){
            _package = package;
            this.arrayPropertyNames = arrayPropertyNames;
        }

        private JToken GetArrayProperty(){
            foreach (var name in arrayPropertyNames){
                var array = _package[name] as JToken;
                if (null != array){
                    return array;
                }
            }
            return null;
        }

        public int Count{
            get{
                var token = GetArrayProperty();
                if (null == token){
                    return 0;
                }

                switch (token.Type){
                    case JTokenType.String:
                        return 1;

                    case JTokenType.Array:
                        return (token as JArray).Count;

                    default:
                        return 0;
                }
            }
        }

        public string this[int index]{
            get{
                if (Count <= 0){
                    throw new IndexOutOfRangeException(
                        "Cannot retrieve item from empty package.json string array.");
                }

                var token = GetArrayProperty();
                switch (token.Type){
                    case JTokenType.String:
                        if (index != 0){
                            throw new IndexOutOfRangeException(
                                string.Format(
                                    "Cannot retrieve value from index '{0}' in a package.json string array containing only 1 item.",
                                    index));
                        }
                        return token.Value<string>();

                    default: //  Can only be an array at this point, since Count has been called.
                        return (token as JArray)[index].Value<string>();
                }
            }
        }

        public IEnumerator<string> GetEnumerator(){
            switch (Count){
                case 0:
                    return new List<string>().GetEnumerator();

                case 1:
                    return new List<string>{this[0]}.GetEnumerator();

                default:
                    return
                        (GetArrayProperty() as JArray).Select(value => value.Value<string>())
                                                      .ToList()
                                                      .GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }
    }
}