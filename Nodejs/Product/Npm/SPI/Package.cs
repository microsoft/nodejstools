// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class Package : RootPackage, IPackage
    {
        private IRootPackage _parent;

        public Package(
            IRootPackage parent,
            string fullPathToRootDirectory,
            bool showMissingDevOptionalSubPackages,
            Dictionary<string, ModuleInfo> allModules = null,
            int depth = 0,
            int maxDepth = 1)
            : base(fullPathToRootDirectory, showMissingDevOptionalSubPackages, allModules, depth, maxDepth)
        {
            this._parent = parent;
        }

        public string PublishDateTimeString { get { return null; } }

        public IEnumerable<SemverVersion> AvailableVersions { get { throw new NotImplementedException(); } }

        public string RequestedVersionRange { get; internal set; }

        public IEnumerable<string> Keywords
        {
            get
            {
                var keywords = null == this.PackageJson ? null : this.PackageJson.Keywords;
                return keywords ?? (IEnumerable<string>)new List<string>();
            }
        }

        public bool IsListedInParentPackageJson
        {
            get
            {
                IPackageJson parentPackageJson = this._parent.PackageJson;
                return (null != parentPackageJson && parentPackageJson.AllDependencies.Contains(this.Name));
            }
        }

        public bool IsMissing
        {
            get { return this.IsListedInParentPackageJson && !Directory.Exists(this.Path); }
        }

        public bool IsDevDependency
        {
            get
            {
                IPackageJson parentPackageJson = this._parent.PackageJson;
                return null != parentPackageJson && parentPackageJson.DevDependencies.Contains(this.Name);
            }
        }

        public bool IsDependency
        {
            get
            {
                IPackageJson parentPackageJson = this._parent.PackageJson;
                return null != parentPackageJson && parentPackageJson.Dependencies.Contains(this.Name);
            }
        }

        public bool IsOptionalDependency
        {
            get
            {
                IPackageJson parentPackageJson = this._parent.PackageJson;
                return null != parentPackageJson && parentPackageJson.OptionalDependencies.Contains(this.Name);
            }
        }

        public bool IsBundledDependency
        {
            get
            {
                IPackageJson parentPackageJson = this._parent.PackageJson;
                return null != parentPackageJson && parentPackageJson.BundledDependencies.Contains(this.Name);
            }
        }

        public PackageFlags Flags
        {
            get
            {
                return (!this.IsListedInParentPackageJson ? PackageFlags.NotListedAsDependency : 0)
                       | (this.IsMissing ? PackageFlags.Missing : 0)
                       | (this.IsDevDependency ? PackageFlags.Dev : 0)
                       | (this.IsOptionalDependency ? PackageFlags.Optional : 0)
                       | (this.IsBundledDependency ? PackageFlags.Bundled : 0);
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1}", this.Name, this.Version);
        }
    }
}

