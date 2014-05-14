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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.Project;

namespace Microsoft.NodejsTools.NpmUI {
    abstract class PackageCatalogEntryViewModel {
        private readonly string _name;
        private readonly SemverVersion? _version;
        private readonly string _author;
        private readonly string _description;
        private readonly string _keywords;

        private readonly SemverVersion? _localVersion, _globalVersion;

        protected PackageCatalogEntryViewModel(
            string name,
            SemverVersion? version,
            string author,
            string description,
            string keywords,
            SemverVersion? localVersion,
            SemverVersion? globalVersion
        ) {
            _name = name;
            _version = version;
            _author = author;
            _description = description;
            _keywords = keywords;
            _localVersion = localVersion;
            _globalVersion = globalVersion;
        }

        public virtual string Name { get { return _name; } }
        public string Version { get { return ToString(_version); } }
        public string Author { get { return _author; } }
        public string Description { get { return _description; } }
        public string Keywords { get { return _keywords; } }
        public bool IsInstalledLocally { get { return _localVersion.HasValue; } }
        public bool IsInstalledGlobally { get { return _globalVersion.HasValue; } }
        public bool IsLocalInstallOutOfDate { get { return _localVersion.HasValue && _localVersion < _version; } }
        public bool IsGlobalInstallOutOfDate { get { return _globalVersion.HasValue && _globalVersion < _version; } }
        public string LocalVersion { get { return ToString(_localVersion); } }
        public string GlobalVersion { get { return ToString(_globalVersion); } }

        private static string ToString(SemverVersion? version) {
            return version.HasValue ? version.ToString() : string.Empty;
        }
    }

    internal class InstallCommandPackageCatalogEntryViewModel : PackageCatalogEntryViewModel, INotifyPropertyChanged {
        private string _name;

        public InstallCommandPackageCatalogEntryViewModel()
        : base(
            string.Empty,
            null,
            string.Empty,
            string.Empty,
            string.Empty,
            null,
            null
        ) { }

        public override string Name {
            get {
                return string.IsNullOrEmpty(_name)
                    ? SR.GetString(SR.NpmPackageInstallHelpMessage)
                    : _name;
            }
        }

        public string Command {
            get { return _name; }
            set {
                _name = value;
                OnPropertyChanged();
                OnPropertyChanged("Name");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    internal class ReadOnlyPackageCatalogEntryViewModel : PackageCatalogEntryViewModel {
        public ReadOnlyPackageCatalogEntryViewModel(IPackage package, IPackage localInstall, IPackage globalInstall)
        : base(
            package.Name ?? "",
            package.Version,
            package.Author == null ? "" : package.Author.ToString(),
            package.Description ?? "",
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
