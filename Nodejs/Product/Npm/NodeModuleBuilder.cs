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
    public sealed class NodeModuleBuilder
    {
        private List<IPackage> _dependencies = new List<IPackage>();
        private readonly StringBuilder _descriptionBuff = new StringBuilder();
        private readonly StringBuilder _authorBuff = new StringBuilder();
        private List<string> _keywords = new List<string>();
        private List<string> _homepages = new List<string>();

        public NodeModuleBuilder()
        {
            Reset();
        }

        public void Reset()
        {
            this.Name = null;
            
            this.AvailableVersions = new List<SemverVersion>();

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
            this.PublishDateTimeString = null;
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

        public SemverVersion LatestVersion => this.AvailableVersions.FirstOrDefault();

        public IList<SemverVersion> AvailableVersions { get; private set; }

        public IEnumerable<string> Homepages => this._homepages;

        public void AddVersion(SemverVersion version)
        {
            this.AvailableVersions.Add(version);
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

        public void SetDate(string text)
        {
            this.PublishDateTimeString = text;
        }

        public string PublishDateTimeString { get; private set; }

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

