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

using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class Person : IPerson {
        private static readonly Regex RegexPerson = new Regex(
            "^"
            + "(?<name>[^<]+)"
            + "(<(?<email>[^>]+)>)?"
            + "(\\s*\\((?<url>[^\\)])\\))?"
            + "$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public Person(string source) {
            InitFromString(source);
        }

        private void InitFromString(string source) {
            var matches = RegexPerson.Matches(source);
            if (matches.Count != 1) {
                Name = source;
            } else {
                var match = matches[0];
                var group = match.Groups["name"];
                if (group.Success) {
                    Name = group.Value;
                }

                group = match.Groups["email"];
                if (group.Success) {
                    Email = group.Value;
                }

                group = match.Groups["url"];
                if (group.Success) {
                    Url = group.Value;
                }
            }
        }

        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Url { get; private set; }

        public override string ToString() {
            var buff = new StringBuilder();
            if (!string.IsNullOrEmpty(Name)) {
                buff.Append(Name);
            }

            if (!string.IsNullOrEmpty(Email)) {
                if (buff.Length > 0) {
                    buff.Append(' ');
                }
                buff.Append('<');
                buff.Append(Email);
                buff.Append('>');
            }

            if (!string.IsNullOrEmpty(Url)) {
                if (buff.Length > 0) {
                    buff.Append(' ');
                }
                buff.Append('(');
                buff.Append(Url);
                buff.Append(')');
            }
            return buff.ToString();
        }
    }
}