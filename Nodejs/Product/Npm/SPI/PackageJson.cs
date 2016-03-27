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

using System.Linq;
using System.Collections.Generic;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class PackageJson : IPackageJson {
        private dynamic _package;
        private Scripts _scripts;
        private Bugs _bugs;

        public PackageJson(dynamic package) {
            _package = package;

            InitKeywords();
            InitHomepages();
            InitLicenses();
            InitFiles();
            InitMan();
            InitDependencies();
            InitDevDependencies();
            InitBundledDependencies();
            InitOptionalDependencies();
            InitAllDependencies();
            InitRequiredBy();
        }

        private void WrapRuntimeBinderExceptionAndRethrow(
            string errorProperty,
            RuntimeBinderException rbe) {
            throw new PackageJsonException(
                    string.Format(@"Exception occurred retrieving {0} from package.json. The file may be invalid: you should edit it to correct an errors.

The following error occurred:

{1}",
                        errorProperty,
                        rbe));
        }

        private void InitKeywords() {
            try {
                Keywords = new Keywords(_package);
            } catch (RuntimeBinderException rbe) {
                WrapRuntimeBinderExceptionAndRethrow(
                    "keywords",
                    rbe);
            }
        }

        private void InitHomepages() {
            try {
                Homepages = new Homepages(_package);
            }
            catch (RuntimeBinderException rbe) {
                WrapRuntimeBinderExceptionAndRethrow(
                    "homepage",
                    rbe);
            }
        }

        private void InitFiles() {
            try {
                Files = new PkgFiles(_package);
            } catch (RuntimeBinderException rbe) {
                WrapRuntimeBinderExceptionAndRethrow(
                    "files",
                    rbe);
            }
        }

        private void InitLicenses() {
            try {
                Licenses = new Licenses(_package);
            } catch (RuntimeBinderException rbe) {
                WrapRuntimeBinderExceptionAndRethrow(
                    "licenses",
                    rbe);
            }
        }

        private void InitMan() {
            try {
                Man = new Man(_package);
            } catch (RuntimeBinderException rbe) {
                WrapRuntimeBinderExceptionAndRethrow(
                    "man",
                    rbe);
            }
        }

        private void InitDependencies() {
            try {
                Dependencies = new Dependencies(_package, "dependencies");
            } catch (RuntimeBinderException rbe) {
                WrapRuntimeBinderExceptionAndRethrow(
                    "dependencies",
                    rbe);
            }
        }

        private void InitDevDependencies() {
            try {
                DevDependencies = new Dependencies(_package, "devDependencies");
            } catch (RuntimeBinderException rbe) {
                WrapRuntimeBinderExceptionAndRethrow(
                    "dev dependencies",
                    rbe);
            }
        }

        private void InitBundledDependencies() {
            try {
                BundledDependencies = new BundledDependencies(_package);
            } catch (RuntimeBinderException rbe) {
                WrapRuntimeBinderExceptionAndRethrow(
                    "bundled dependencies",
                    rbe);
            }
        }

        private void InitOptionalDependencies() {
            try {
                OptionalDependencies = new Dependencies(_package, "optionalDependencies");
            } catch (RuntimeBinderException rbe) {
                WrapRuntimeBinderExceptionAndRethrow(
                    "optional dependencies",
                    rbe);
            }
        }

        private void InitAllDependencies() {
            try {
                AllDependencies = new Dependencies(_package, "dependencies", "devDependencies", "optionalDependencies");
            } catch (RuntimeBinderException rbe) {
                WrapRuntimeBinderExceptionAndRethrow(
                    "all dependencies",
                    rbe);
            }
        }

        private void InitRequiredBy() {
            try {
                RequiredBy = (_package["_requiredBy"] as IEnumerable<JToken> ?? Enumerable.Empty<JToken>()).Values<string>();
            } catch (RuntimeBinderException rbe) {
                System.Diagnostics.Debug.WriteLine(rbe);
                WrapRuntimeBinderExceptionAndRethrow(
                    "required by",
                    rbe);
            }
        }

        public string Name {
            get { return null == _package.name ? null : _package.name.ToString(); }
        }

        public SemverVersion Version {
            get {
                return null == _package.version ? new SemverVersion() : SemverVersion.Parse(_package.version.ToString());
            }
        }

        public IScripts Scripts {
            get {
                if (null == _scripts) {
                    dynamic scriptsJson = _package.scripts;
                    if (null == scriptsJson) {
                        scriptsJson = new JObject();
                        _package.scripts = scriptsJson;
                    }
                    _scripts = new Scripts(scriptsJson);
                }

                return _scripts;
            }
        }

        public IPerson Author {
            get {
                var author = _package.author;
                return null == author ? null : Person.CreateFromJsonSource(author.ToString());
            }
        }

        public string Description {
            get { return null == _package.description ? null : _package.description.ToString(); }
        }

        public IKeywords Keywords { get; private set; }

        public IHomepages Homepages { get; private set; }

        public IBugs Bugs {
            get {
                if (null == _bugs && null != _package.bugs) {
                    _bugs = new Bugs(_package);
                }
                return _bugs;
            }
        }

        public ILicenses Licenses { get; private set; }

        public IFiles Files { get; private set; }

        public IMan Man { get; private set; }

        public IDependencies Dependencies { get; private set; }
        public IDependencies DevDependencies { get; private set; }
        public IBundledDependencies BundledDependencies { get; private set; }
        public IDependencies OptionalDependencies { get; private set; }
        public IDependencies AllDependencies { get; private set; }
        public IEnumerable<string> RequiredBy { get; private set; }
    }
}