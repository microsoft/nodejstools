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

using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Microsoft.NodejsTools.Npm.SPI;

namespace Microsoft.NodejsTools.Npm {
    /// <summary>
    /// Mutable class for building immutable node module descriptions
    /// </summary>
    public class NodeModuleBuilder {
        private List<IPackage> _dependencies = new List<IPackage>();
        private readonly StringBuilder _descriptionBuff = new StringBuilder();
        private readonly StringBuilder _authorBuff = new StringBuilder();
        private readonly StringBuilder _publishDateTime = new StringBuilder();
        private List<string> _keywords = new List<string>();
        private List<string> _homepages = new List<string>();

        public NodeModuleBuilder() {
        }

        public void Reset() {
            Name = null;
            Version = new SemverVersion();
            Flags = PackageFlags.None;
            RequestedVersionRange = null;

            //  These *have* to be reinitialised or they'll be cleared
            //  in any packages that have been created using the builder
            //  because they're passed by reference.
            _dependencies = new List<IPackage>();
            _keywords = new List<string>();
            _homepages = new List<string>();

            _descriptionBuff.Length = 0;
            _authorBuff.Length = 0;
            _publishDateTime.Length = 0;
        }

        public void AddAuthor(string text) {
            if (_authorBuff.Length > 0) {
                _authorBuff.Append(' ');
            }
            _authorBuff.Append(text);
        }

        public IPerson Author {
            get {
                var text = _authorBuff.ToString().Trim();
                return string.IsNullOrEmpty(text) ? null : new Person(text);
            }
        }

        public string Name { get; set; }

        public SemverVersion Version { get; set; }

        public IEnumerable<string> Homepages {
            get {
                return _homepages;
            }
        }

        public void AddHomepage(string homepage) {
            _homepages.Add(homepage);
        }

        public void AppendToDescription(string text) {
            _descriptionBuff.Append(text);
        }

        public string Description {
            get {
                var text = _descriptionBuff.ToString().Trim();
                return string.IsNullOrEmpty(text) ? null : text;
            }
        }

        public void AppendToDate(string text) {
            if (_publishDateTime.Length > 0) {
                _publishDateTime.Append(' ');
            }
            _publishDateTime.Append(text);
        }

        public string PublishDateTimeString {
            get {
                var text = _publishDateTime.ToString().Trim();
                return string.IsNullOrEmpty(text) ? null : text;
            }
        }

        public IEnumerable<IPackage> Dependencies {
            get { return _dependencies; }
        }

        public void AddDependency(IPackage module) {
            _dependencies.Add(module);
        }

        public void AddDependencies(IEnumerable<IPackage> packages) {
            _dependencies.AddRange(packages);
        }

        public PackageFlags Flags { get; set; }

        public string RequestedVersionRange { get; set; }

        public void AddKeyword(string keyword) {
            _keywords.Add(keyword);
        }

        public IEnumerable<string> Keywords {
            get {
                return _keywords;
            }
        }

        public IPackage Build() {
            var proxy = new PackageProxy();
            proxy.Author = Author;
            proxy.Name = Name;
            proxy.Version = Version;
            proxy.Description = Description;
            proxy.Homepages = Homepages;
            proxy.PublishDateTimeString = PublishDateTimeString;
            proxy.RequestedVersionRange = RequestedVersionRange;
            proxy.Flags = Flags;

            proxy.Keywords = _keywords;

            var modules = new NodeModulesProxy();
            foreach (var dep in Dependencies) {
                modules.AddModule(dep);
            }
            proxy.Modules = modules;
            return proxy;
        }
    }
}