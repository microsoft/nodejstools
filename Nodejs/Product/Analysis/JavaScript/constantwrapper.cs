// constantwrapper.cs
//
// Copyright 2010 Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.NodejsTools.Parsing
{
    public class ConstantWrapper : Expression
    {
        // this is a regular expression that we'll use to strip a leading "0x" from
        // a string if we are trying to parse it into a number. also removes the leading
        // and trailing spaces, while we're at it.
        // will also capture a sign if it's present. Strictly speaking, that's not allowed
        // here, but some browsers (Firefox, Opera, Chrome) will parse it. IE and Safari
        // will not. So if we match that sign, we are in a cross-browser gray area.
        private static Regex s_hexNumberFormat = new Regex(
          @"^\s*(?<sign>[-+])?0X(?<hex>[0-9a-f]+)\s*$",
          RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
#if !SILVERLIGHT
 | RegexOptions.Compiled
#endif
);

        // used to detect possible ASP.NET substitutions in a string
        private static Regex s_aspNetSubstitution = new Regex(
            @"\<%.*%\>",
            RegexOptions.CultureInvariant
#if !SILVERLIGHT
 | RegexOptions.Compiled
#endif
);

        public Object Value { get; set; }

        public ConstantWrapper(Object value, IndexSpan span)
            : base(span)
        {
            Value = value;
        }

        public override void Walk(AstVisitor visitor) {
            if (visitor.Walk(this)) {
            }
            visitor.PostWalk(this);
        }

        public override string ToString()
        {
            if (Value == null) {
                // null is just "null"
                return "null";
            } else if (Value is bool) {
                // boolean is "true" or "false"
                return (bool)Value ? "true" : "false";
            } else if (Value is double) {
                // handle some special values, otherwise just fall through
                // to the default ToString implementation
                double doubleValue = (double)Value;
                if (doubleValue == 0) {
                    // both -0 and 0 return "0". Go figure.
                    return "0";
                }
                if (double.IsNaN(doubleValue)) {
                    return "NaN";
                }
                if (double.IsPositiveInfinity(doubleValue)) {
                    return "Infinity";
                }
                if (double.IsNegativeInfinity(doubleValue)) {
                    return "-Infinity";
                }

                // use the "R" format, which guarantees that the double value can
                // be round-tripped to the same value
                return doubleValue.ToStringInvariant("R");
            }

            // otherwise this must be a string
            return Value.ToString();
        }
    }

    public class InvalidNumericErrorValue {
        private readonly string _code;

        public InvalidNumericErrorValue(string code) {
            _code = code;
        }

        public override string ToString() {
            return "Invalid numeric literal: " + _code;
        }

        public override bool Equals(object obj) {
            var other = obj as InvalidNumericErrorValue;
            if (other != null) {
                return other._code == _code;
            }
            return false;
        }

        public override int GetHashCode() {
            return _code.GetHashCode() + 1;
        }
    }
}
