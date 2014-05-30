// Extensions.cs
//
// Copyright 2012 Microsoft Corporation
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

#if NET_20

namespace System.Runtime.CompilerServices
{
    // Summary:
    //     Indicates that a method is an extension method, or that a class or assembly
    //     contains extension methods.
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    internal sealed class ExtensionAttribute : Attribute
    {
        // Summary:
        //     Initializes a new instance of the System.Runtime.CompilerServices.ExtensionAttribute
        //     class.
        public ExtensionAttribute() { }
    }
}

#endif

namespace Microsoft.NodejsTools.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public static class AjaxMinExtensions
    {
        public static string FormatInvariant(this string format, params object[] args)
        {
            try
            {
                return format == null
                    ? string.Empty
                    : string.Format(CultureInfo.InvariantCulture, format, args);
            }
            catch (FormatException)
            {
                return format;
            }
        }

        public static string ToStringInvariant(this double number, string format)
        {
            return format == null
                ? number.ToString(CultureInfo.InvariantCulture)
                : number.ToString(format, CultureInfo.InvariantCulture);
        }

        public static TResult IfNotNull<TObject, TResult>(this TObject obj, Func<TObject, TResult> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            return obj == null ? default(TResult) : action(obj);
        }

        public static void CopyItemsTo<TSource>(this ICollection<TSource> fromSet, ICollection<TSource> toSet)
        {
            if (toSet == null)
            {
                throw new ArgumentNullException("toSet");
            }

            if (fromSet != null)
            {
                foreach (var item in fromSet)
                {
                    toSet.Add(item);
                }
            }
        }
    }
}
