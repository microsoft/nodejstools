// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI
{
    internal abstract class PackageCatalogEntryViewModel
    {
        private readonly SemverVersion? version;
        private readonly SemverVersion? localVersion;

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
            this.Name = name;
            this.version = version;
            this.AvailableVersions = availableVersions ?? Enumerable.Empty<SemverVersion>();
            this.Author = author;
            this.Description = description;
            this.Homepages = homepages ?? Enumerable.Empty<string>();
            this.Keywords = keywords;
            this.localVersion = localVersion;
        }

        public virtual string Name { get; }

        public string Version => this.version?.ToString() ?? string.Empty;

        public IEnumerable<SemverVersion> AvailableVersions { get; }

        public string Author { get; }
        public Visibility AuthorVisibility => string.IsNullOrEmpty(this.Author) ? Visibility.Collapsed : Visibility.Visible;

        public string Description { get; }
        public Visibility DescriptionVisibility => string.IsNullOrEmpty(this.Description) ? Visibility.Collapsed : Visibility.Visible;

        public IEnumerable<string> Homepages { get; }
        public Visibility HomepagesVisibility => this.Homepages.Any() ? Visibility.Visible : Visibility.Collapsed;

        public string Keywords { get; }

        public bool IsInstalledLocally => this.localVersion.HasValue;
        public bool IsLocalInstallOutOfDate => this.localVersion.HasValue && this.localVersion < this.version;
        public string LocalVersion => this.localVersion?.ToString() ?? string.Empty;

        public override string ToString()
        {
            return this.Name;
        }
    }

    internal class ReadOnlyPackageCatalogEntryViewModel : PackageCatalogEntryViewModel
    {
        public ReadOnlyPackageCatalogEntryViewModel(IPackage package, IPackage localInstall)
            : base(
                package.Name ?? string.Empty,
                package.Version,
                package.AvailableVersions,
                package.Author?.ToString() ?? string.Empty,
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
