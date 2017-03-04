// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.Project;

namespace Microsoft.NodejsTools.NpmUI
{
    internal abstract class PackageCatalogEntryViewModel
    {
        private readonly string _name;
        private readonly SemverVersion? _version;
        private readonly List<SemverVersion> _availableVersions;
        private readonly string _author;
        private readonly string _description;
        private readonly List<string> _homepages;
        private readonly string _keywords;

        private readonly SemverVersion? _localVersion;

        protected PackageCatalogEntryViewModel(
            string name,
            SemverVersion? version,
            IEnumerable<SemverVersion> availableVersions,
            string author,
            string description,
            IEnumerable<string> homepages,
            string keywords,
            SemverVersion? localVersion
        )
        {
            this._name = name;
            this._version = version;
            this._availableVersions = availableVersions != null ? availableVersions.ToList() : new List<SemverVersion>();
            this._author = author;
            this._description = description;
            this._homepages = homepages != null ? homepages.ToList() : new List<string>();
            this._keywords = keywords;
            this._localVersion = localVersion;
        }

        public virtual string Name => this._name;
        public string Version => ToString(this._version);
        public IEnumerable<SemverVersion> AvailableVersions => this._availableVersions;
        public string Author => this._author;
        public Visibility AuthorVisibility => string.IsNullOrEmpty(this._author) ? Visibility.Collapsed : Visibility.Visible;
        public string Description => this._description;
        public Visibility DescriptionVisibility => string.IsNullOrEmpty(this._description) ? Visibility.Collapsed : Visibility.Visible;
        public IEnumerable<string> Homepages => this._homepages;
        public Visibility HomepagesVisibility => this._homepages.Any() ? Visibility.Visible : Visibility.Collapsed;
        public string Keywords => this._keywords;
        public bool IsInstalledLocally => this._localVersion.HasValue;
        public bool IsLocalInstallOutOfDate => this._localVersion.HasValue && this._localVersion < this._version;
        public string LocalVersion => ToString(this._localVersion);
        private static string ToString(SemverVersion? version)
        {
            return version.HasValue ? version.ToString() : string.Empty;
        }
    }

    internal class ReadOnlyPackageCatalogEntryViewModel : PackageCatalogEntryViewModel
    {
        public ReadOnlyPackageCatalogEntryViewModel(IPackage package, IPackage localInstall)
            : base(
                package.Name ?? string.Empty,
                package.Version,
                package.AvailableVersions,
                package.Author == null ? string.Empty : package.Author.ToString(),
                package.Description ?? string.Empty,
                package.Homepages,
                (package.Keywords != null && package.Keywords.Any())
                    ? string.Join(", ", package.Keywords)
                    : Resources.NoKeywordsInPackage,
                localInstall != null ? (SemverVersion?)localInstall.Version : null
            )
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                throw new ArgumentNullException("package.Name");
            }
        }
    }
}

