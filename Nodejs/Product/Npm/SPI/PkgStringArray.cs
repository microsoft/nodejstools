// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

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
        private readonly IList<string> elements;

        protected PkgStringArray(JObject package, params string[] arrayPropertyNames)
        {
            var token = GetArrayProperty(package, arrayPropertyNames);
            if (token == null)
            {
                this.elements = new List<string>();
            }
            else
            {
                switch (token.Type)
                {
                    case JTokenType.String:
                        this.elements = new[] { token.Value<string>() };
                        break;

                    case JTokenType.Array:
                        this.elements = (token as JArray).Select(value => value.Value<string>()).ToList();
                        break;

                    default:
                        this.elements = new List<string>();
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

        public int Count => this.elements.Count;

        public string this[int index]
        {
            get
            {
                if (this.Count <= 0)
                {
                    throw new IndexOutOfRangeException(
                        "Cannot retrieve item from empty package.json string array.");
                }

                if (index > this.Count)
                {
                    throw new IndexOutOfRangeException(
                        string.Format(CultureInfo.CurrentCulture,
                            "Cannot retrieve value from index '{0}' in a package.json string array containing only 1 item.",
                            index));
                }
                return this.elements[index];
            }
        }

        public IEnumerator<string> GetEnumerator() => this.elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
