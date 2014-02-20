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
using System.Windows;
using System.Windows.Media;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI {
    internal class PackageCatalogEntryViewModel {

        private IPackage _package;
        private IRootPackage _local;
        private IGlobalPackages _global;

        public PackageCatalogEntryViewModel(
            IPackage package,
            IRootPackage local,
            IGlobalPackages global) {
            _package = package;
            _local = local;
            _global = global;
        }

        public string Name {
            get { return _package.Name; }
        }

        public string VersionString {
            get { return string.Format( "@ {0}", _package.Version); }
        }

        public Color VersionColor {
            get { return KeywordsColor; }
        }

        public string Author {
            get { return string.Format("by {0}", _package.Author); }
        }

        public Color AuthorColor {
            get { return KeywordsColor; }
        }

        public string Description {
            get { return _package.Description; }
        }

        public string Keywords {
            get {
                var keywords = KeywordStringBuilder.BuildKeywordString(_package);
                return string.IsNullOrEmpty(keywords)
                    ? Resources.NoKeywordsInPackage
                    : keywords;
            }
        }

        public Color KeywordsColor {
            get { return WpfColorUtils.MidPoint(SystemColors.WindowColor, SystemColors.WindowTextColor); }
        }

        private IPackage GetFromRoot(IRootPackage root) {
            if (null == root) {
                return null;
            }

            return root.Modules[_package.Name];
        }

        private IPackage LocallyInstalledPackage {
            get { return GetFromRoot(_local); }
        }

        private IPackage GloballyInstalledPackage {
            get { return GetFromRoot(_global); }
        }

        private bool IsInstalledLocally {
            get { return LocallyInstalledPackage != null; }
        }

        private bool IsInstalledGlobally {
            get { return GloballyInstalledPackage != null; }
        }

        private bool IsInstalledPackageOutOfDate(IPackage installed) {
            return null != installed && installed.Version < _package.Version;
        }

        private bool IsLocalInstallOutOfDate {
            get {
                return IsInstalledPackageOutOfDate(LocallyInstalledPackage);
            }
        }

        private bool IsGlobalInstallOutOfDate {
            get {
                return IsInstalledPackageOutOfDate(GloballyInstalledPackage);
            }
        }

        //  This doesn't work as I'd like
        //public Color BackgroundColor {
        //    get {
        //        return IsInstalledGlobally || IsInstalledLocally
        //            ? (IsGlobalInstallOutOfDate || IsLocalInstallOutOfDate
        //                ? WpfColorUtils.MidPoint(Colors.Transparent, Colors.Yellow)
        //                : WpfColorUtils.MidPoint(Colors.Transparent, Colors.LightGreen))
        //            : Colors.Transparent;
        //    }
        //}

        public Visibility InstallMessageVisibility {
            get {
                return IsInstalledLocally || IsInstalledGlobally
                    ? Visibility.Visible
                    : Visibility.Hidden;
            }
        }

        public string InstalledLocallyMessage {
            get {
                if (IsInstalledLocally) {
                    return IsLocalInstallOutOfDate
                        ? string.Format(Resources.PackageInstalledLocallyOldVersion, _package.Version)
                        : Resources.PackageInstalledLocally;
                }

                return string.Empty;
            }
        }

        public string InstalledGloballyMessage {
            get {
                if (IsInstalledGlobally) {
                    return IsGlobalInstallOutOfDate
                        ? string.Format(Resources.PackageInstalledGloballyOldVersion, _package.Version)
                        : Resources.PackageInstalledGlobally;
                }

                return string.Empty;
            }
        }

        public string InstallMessage {
            get {
                var buff = new StringBuilder();
                buff.Append(InstalledLocallyMessage);
                var msg = InstalledGloballyMessage;
                if (!string.IsNullOrEmpty(msg)) {
                    if (buff.Length > 0) {
                        buff.Append("; ");
                    }
                    buff.Append(msg);
                }
                return buff.ToString();
            }
        }

        public Color InstallMessageColor {
            get {
                return IsInstalledLocally && IsLocalInstallOutOfDate
                    || IsInstalledGlobally && IsGlobalInstallOutOfDate
                    ? Colors.Red
                    : Colors.MediumSeaGreen;
            }
        }
    }
}
