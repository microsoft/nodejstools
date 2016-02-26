//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Microsoft.NodejsTools.Npm.SPI;

namespace Microsoft.NodejsTools.Npm {
    /// <summary>
    /// Mutable class for building immutable node module descriptions
    /// </summary>
    internal class NodeModuleBuilder {
        private List<IPackage> _dependencies = new List<IPackage>();
        private readonly StringBuilder _descriptionBuff = new StringBuilder();
        private readonly StringBuilder _authorBuff = new StringBuilder();
        private readonly StringBuilder _publishDateTime = new StringBuilder();
        private List<string> _keywords = new List<string>();
        private List<string> _homepages = new List<string>();
        private List<SemverVersion> _availableVersions = new List<SemverVersion>(); 

        public NodeModuleBuilder() {
            Reset();
        }

        public void Reset() {
            Name = null;

            // We should double check, but I believe that the package no longer exists when "latest" is not set.
            // If that's the case, we should include an option to filter out those packages.
            // https://nodejstools.codeplex.com/workitem/1452
            LatestVersion = SemverVersion.UnknownVersion;
            _availableVersions = new List<SemverVersion>();

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

        public SemverVersion LatestVersion { get; set; }

        public IEnumerable<SemverVersion> AvailableVersions {
            get { return _availableVersions; }
            set { _availableVersions = value != null ? value.ToList() : new List<SemverVersion>(); }
        }

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
            var proxy = new PackageProxy {
                Author = Author,
                Name = Name,
                Version = LatestVersion,
                AvailableVersions = AvailableVersions,
                Description = Description,
                Homepages = Homepages,
                PublishDateTimeString = PublishDateTimeString,
                RequestedVersionRange = RequestedVersionRange,
                Flags = Flags,
                Keywords = _keywords
            };

            var modules = new NodeModulesProxy();
            foreach (var dep in Dependencies) {
                modules.AddModule(dep);
            }
            proxy.Modules = modules;
            return proxy;
        }
    }
}