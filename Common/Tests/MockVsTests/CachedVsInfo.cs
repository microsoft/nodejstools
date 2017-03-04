// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudioTools.MockVsTests
{
    /// <summary>
    /// Stores cached state for creating a MockVs.  This state is initialized once for the process and then
    /// re-used to create new MockVs instances.  We create fresh MockVs instances to avoid having state
    /// lingering between tests.
    /// </summary>
    internal class CachedVsInfo
    {
        public readonly AggregateCatalog Catalog;
        public readonly List<Type> Packages;
        public Dictionary<string, LanguageServiceInfo> LangServicesByName = new Dictionary<string, LanguageServiceInfo>();
        public Dictionary<Guid, LanguageServiceInfo> LangServicesByGuid = new Dictionary<Guid, LanguageServiceInfo>();
        public Dictionary<string, string> _languageNamesByExtension = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public CachedVsInfo(AggregateCatalog catalog, List<Type> packages)
        {
            Catalog = catalog;
            Packages = packages;

            foreach (var package in Packages)
            {
                var attrs = package.GetCustomAttributes(typeof(ProvideLanguageServiceAttribute), false);
                foreach (ProvideLanguageServiceAttribute attr in attrs)
                {
                    foreach (var type in package.Assembly.GetTypes())
                    {
                        if (type.GUID == attr.LanguageServiceSid)
                        {
                            var info = new LanguageServiceInfo(attr);
                            LangServicesByGuid[attr.LanguageServiceSid] = info;
                            LangServicesByName[attr.LanguageName] = info;

                            break;
                        }
                    }
                }

                var extensions = package.GetCustomAttributes(typeof(ProvideLanguageExtensionAttribute), false);
                foreach (ProvideLanguageExtensionAttribute attr in extensions)
                {
                    LanguageServiceInfo info;
                    if (LangServicesByGuid.TryGetValue(attr.LanguageService, out info))
                    {
                        _languageNamesByExtension[attr.Extension] = info.Attribute.LanguageName;
                    }
                }
            }
        }
    }
}

