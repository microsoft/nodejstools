// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class Person : IPerson
    {
        // We cannot rely on the ordering of any of these fields,
        // so we should match them separately.
        private static readonly Regex ObjectPersonRegex = new Regex(
            "\"name\":\\s*\"(?<name>[^<]+?)\"" +
            "|" +
            "\"email\":\\s*\"(?<email>[^<]+?)\"" +
            "|" +
            "\"url\":\\s*\"(?<url>[^<]+?)\"",
            RegexOptions.Singleline);

        [JsonConstructor]
        private Person()
        {
            // Enables Json deserialization
        }

        private Person(string name, string email = null, string url = null)
        {
            this.Name = name;
            this.Email = email;
            this.Url = url;
        }

        public static Person CreateFromJsonSource(string source)
        {
            if (source == null)
            {
                return new Person(string.Empty);
            }

            return TryCreatePersonFromObject(source) ?? CreatePersonFromString(source);
        }

        /// <summary>
        /// Try to create a person object from a json object.
        /// 
        /// This can either be a json object or a string: https://docs.npmjs.com/files/package.json#people-fields-author-contributors
        /// </summary>
        /// <param name="source">Json source</param>
        private static Person TryCreatePersonFromObject(string source)
        {
            string name = null;
            string email = null;
            string url = null;

            // We parse using a regex because JObject.Parse throws exceptions for malformatted json,
            // and simply handling them causes performance issues.
            var matches = ObjectPersonRegex.Matches(source);
            if (matches.Count >= 1)
            {
                foreach (Match match in matches)
                {
                    var group = match.Groups["name"];
                    if (group.Success)
                    {
                        name = group.Value;
                        continue;
                    }

                    group = match.Groups["email"];
                    if (group.Success)
                    {
                        email = group.Value;
                        continue;
                    }

                    group = match.Groups["url"];
                    if (group.Success)
                    {
                        url = group.Value;
                        continue;
                    }
                }
            }
            return name == null ? null : new Person(name, email, url);
        }

        /// <summary>
        /// Try to create a person object from a string.
        /// 
        /// TODO: currently does not try to parse the string to extract the email or url.
        /// </summary>
        /// <param name="source">Json source</param>
        private static Person CreatePersonFromString(string source)
        {
            return new Person(source);
        }

        [JsonProperty]
        public string Name { get; private set; }

        [JsonProperty]
        public string Email { get; private set; }

        [JsonProperty]
        public string Url { get; private set; }

        public override string ToString()
        {
            var buff = new StringBuilder();
            if (!string.IsNullOrEmpty(this.Name))
            {
                buff.Append(this.Name);
            }

            if (!string.IsNullOrEmpty(this.Email))
            {
                if (buff.Length > 0)
                {
                    buff.Append(' ');
                }
                buff.Append('<');
                buff.Append(this.Email);
                buff.Append('>');
            }

            if (!string.IsNullOrEmpty(this.Url))
            {
                if (buff.Length > 0)
                {
                    buff.Append(' ');
                }
                buff.Append('(');
                buff.Append(this.Url);
                buff.Append(')');
            }
            return buff.ToString();
        }
    }
}
