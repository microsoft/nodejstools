// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Npm.SPI;

namespace Microsoft.NodejsTools.Npm
{
    /// <summary>
    /// Mutable class for building immutable node module descriptions
    /// </summary>
    internal class NodeModuleBuilder
    {
        private List<IPackage> _dependencies = new List<IPackage>();
        private readonly StringBuilder _descriptionBuff = new StringBuilder();
        private readonly StringBuilder _authorBuff = new StringBuilder();
        private readonly StringBuilder _publishDateTime = new StringBuilder();
        private List<string> _keywords = new List<string>();
        private List<string> _homepages = new List<string>();
        private List<SemverVersion> _availableVersions = new List<SemverVersion>();

        public NodeModuleBuilder()
        {
            Reset();
        }

        public void Reset()
        {
            this.Name = null;

            // We should double check, but I believe that the package no longer exists when "latest" is not set.
            // If that's the case, we should include an option to filter out those packages.
            // https://nodejstools.codeplex.com/workitem/1452
            this.LatestVersion = SemverVersion.UnknownVersion;
            this._availableVersions = new List<SemverVersion>();

            this.Flags = PackageFlags.None;
            this.RequestedVersionRange = null;

            //  These *have* to be reinitialised or they'll be cleared
            //  in any packages that have been created using the builder
            //  because they're passed by reference.
            this._dependencies = new List<IPackage>();
            this._keywords = new List<string>();
            this._homepages = new List<string>();

            this._descriptionBuff.Length = 0;
            this._authorBuff.Length = 0;
            this._publishDateTime.Length = 0;
        }

        public void AddAuthor(string text)
        {
            if (this._authorBuff.Length > 0)
            {
                this._authorBuff.Append(' ');
            }
            this._authorBuff.Append(text);
        }

        public IPerson Author
        {
            get
            {
                var text = this._authorBuff.ToString().Trim();
                return string.IsNullOrEmpty(text) ? null : Person.CreateFromJsonSource(text);
            }
        }

        public string Name { get; set; }

        public SemverVersion LatestVersion { get; set; }

        public IEnumerable<SemverVersion> AvailableVersions
        {
            get { return this._availableVersions; }
            set { this._availableVersions = value != null ? value.ToList() : new List<SemverVersion>(); }
        }

        public IEnumerable<string> Homepages
        {
            get
            {
                return this._homepages;
            }
        }

        public void AddHomepage(string homepage)
        {
            this._homepages.Add(homepage);
        }

        public void AppendToDescription(string text)
        {
            this._descriptionBuff.Append(text);
        }

        public string Description
        {
            get
            {
                var text = this._descriptionBuff.ToString().Trim();
                return string.IsNullOrEmpty(text) ? null : text;
            }
        }

        public void AppendToDate(string text)
        {
            if (this._publishDateTime.Length > 0)
            {
                this._publishDateTime.Append(' ');
            }
            this._publishDateTime.Append(text);
        }

        public string PublishDateTimeString
        {
            get
            {
                var text = this._publishDateTime.ToString().Trim();
                return string.IsNullOrEmpty(text) ? null : text;
            }
        }

        public IEnumerable<IPackage> Dependencies
        {
            get { return this._dependencies; }
        }

        public void AddDependency(IPackage module)
        {
            this._dependencies.Add(module);
        }

        public void AddDependencies(IEnumerable<IPackage> packages)
        {
            this._dependencies.AddRange(packages);
        }

        public PackageFlags Flags { get; set; }

        public string RequestedVersionRange { get; set; }

        public void AddKeyword(string keyword)
        {
            this._keywords.Add(keyword);
        }

        public IEnumerable<string> Keywords
        {
            get
            {
                return this._keywords;
            }
        }

        public IPackage Build()
        {
            var proxy = new PackageProxy
            {
                Author = this.Author,
                Name = this.Name,
                Version = this.LatestVersion,
                AvailableVersions = this.AvailableVersions,
                Description = this.Description,
                Homepages = this.Homepages,
                PublishDateTimeString = this.PublishDateTimeString,
                RequestedVersionRange = this.RequestedVersionRange,
                Flags = this.Flags,
                Keywords = this._keywords
            };

            var modules = new NodeModulesProxy();
            foreach (var dep in this.Dependencies)
            {
                modules.AddModule(dep);
            }
            proxy.Modules = modules;
            return proxy;
        }
    }
}

