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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.Project;

namespace Microsoft.NodejsTools.NpmUI {
    abstract class PackageCatalogEntryViewModel {
        private readonly string _name;
        private readonly SemverVersion? _version;
        private readonly List<SemverVersion> _availableVersions;
        private readonly string _author;
        private readonly string _description;
        private readonly List<string> _homepages;
        private readonly string _keywords;

        private readonly SemverVersion? _localVersion, _globalVersion;

        protected PackageCatalogEntryViewModel(
            string name,
            SemverVersion? version,
            IEnumerable<SemverVersion> availableVersions, 
            string author,
            string description,
            IEnumerable<string> homepages,
            string keywords,
            SemverVersion? localVersion,
            SemverVersion? globalVersion
        ) {
            _name = name;
            _version = version;
            _availableVersions = availableVersions != null ? availableVersions.ToList() : new List<SemverVersion>();
            _author = author;
            _description = description;
            _homepages = homepages != null ? homepages.ToList() : new List<string>();
            _keywords = keywords;
            _localVersion = localVersion;
            _globalVersion = globalVersion;
        }

        public virtual string Name { 
            get { return _name; } 
        }

        public string Version { 
            get { return ToString(_version); } 
        }

        public IEnumerable<SemverVersion> AvailableVersions {
            get { return _availableVersions; }
        } 

        public string Author { 
            get { return _author; } 
        }

        public Visibility AuthorVisibility { 
            get { return string.IsNullOrEmpty(_author) ? Visibility.Collapsed : Visibility.Visible; } 
        }

        public string Description { get { return _description; } }

        public Visibility DescriptionVisibility { get { return string.IsNullOrEmpty(_description) ? Visibility.Collapsed : Visibility.Visible; } }

        public IEnumerable<string> Homepages { get { return _homepages; } }

        public Visibility HomepagesVisibility { 
            get { return _homepages.Any() ? Visibility.Visible : Visibility.Collapsed; } 
        }
        
        public string Keywords { 
            get { return _keywords; } 
        }
        
        public bool IsInstalledLocally { 
            get { return _localVersion.HasValue; } 
        }
        
        public bool IsInstalledGlobally { 
            get { return _globalVersion.HasValue; } 
        }
        
        public bool IsLocalInstallOutOfDate { 
            get { return _localVersion.HasValue && _localVersion < _version; } 
        }
        
        public bool IsGlobalInstallOutOfDate { 
            get { return _globalVersion.HasValue && _globalVersion < _version; } 
        }
        
        public string LocalVersion { 
            get { return ToString(_localVersion); } 
        }
        
        public string GlobalVersion { 
            get { return ToString(_globalVersion); } 
        }

        private static string ToString(SemverVersion? version) {
            return version.HasValue ? version.ToString() : string.Empty;
        }
    }

    internal class ReadOnlyPackageCatalogEntryViewModel : PackageCatalogEntryViewModel {
        public ReadOnlyPackageCatalogEntryViewModel(IPackage package, IPackage localInstall, IPackage globalInstall)
            : base(
                package.Name ?? string.Empty,
                package.Version,
                package.AvailableVersions,
                package.Author == null ? string.Empty : package.Author.ToString(),
                package.Description ?? string.Empty,
                package.Homepages,
                (package.Keywords != null && package.Keywords.Any())
                    ? string.Join(", ", package.Keywords)
                    : SR.GetString(SR.NoKeywordsInPackage),
                localInstall != null ? (SemverVersion?)localInstall.Version : null,
                globalInstall != null ? (SemverVersion?)globalInstall.Version : null
            ) {
            if (string.IsNullOrEmpty(Name)) {
                throw new ArgumentNullException("package.Name");
            }
        }
    }
}
