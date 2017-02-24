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

using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class PackageJson : IPackageJson
    {
        private string _versionString;

        public PackageJson(dynamic package)
        {
            Keywords = LoadKeywords(package);
            Homepages = LoadHomepages(package);
            Files = LoadFiles(package);
            Dependencies = LoadDependencies(package);
            DevDependencies = LoadDevDependencies(package);
            BundledDependencies = LoadBundledDependencies(package);
            OptionalDependencies = LoadOptionalDependencies(package);
            AllDependencies = LoadAllDependencies(package);
            RequiredBy = LoadRequiredBy(package);

            Name = package.name == null ? null : package.name.ToString();
            _versionString = package.version;
            Description = package.description == null ? null : package.description.ToString();
            Author = package.author == null ? null : Person.CreateFromJsonSource(package.author.ToString());
        }

        private static PackageJsonException WrapRuntimeBinderException(string errorProperty, RuntimeBinderException rbe)
        {
            return new PackageJsonException(
                string.Format(CultureInfo.CurrentCulture, @"Exception occurred retrieving {0} from package.json. The file may be invalid: you should edit it to correct an errors.

The following error occurred:

{1}",
                        errorProperty,
                        rbe));
        }

        private static IKeywords LoadKeywords(dynamic package)
        {
            try
            {
                return new Keywords(package);
            }
            catch (RuntimeBinderException rbe)
            {
                throw WrapRuntimeBinderException("keywords", rbe);
            }
        }

        private static IHomepages LoadHomepages(dynamic package)
        {
            try
            {
                return new Homepages(package);
            }
            catch (RuntimeBinderException rbe)
            {
                throw WrapRuntimeBinderException("homepage", rbe);
            }
        }

        private static IFiles LoadFiles(dynamic package)
        {
            try
            {
                return new PkgFiles(package);
            }
            catch (RuntimeBinderException rbe)
            {
                throw WrapRuntimeBinderException("files", rbe);
            }
        }

        private static IDependencies LoadDependencies(dynamic package)
        {
            try
            {
                return new Dependencies(package, "dependencies");
            }
            catch (RuntimeBinderException rbe)
            {
                throw WrapRuntimeBinderException("dependencies", rbe);
            }
        }

        private static IDependencies LoadDevDependencies(dynamic package)
        {
            try
            {
                return new Dependencies(package, "devDependencies");
            }
            catch (RuntimeBinderException rbe)
            {
                throw WrapRuntimeBinderException("dev dependencies", rbe);
            }
        }

        private static IBundledDependencies LoadBundledDependencies(dynamic package)
        {
            try
            {
                return new BundledDependencies(package);
            }
            catch (RuntimeBinderException rbe)
            {
                throw WrapRuntimeBinderException("bundled dependencies", rbe);
            }
        }

        private static IDependencies LoadOptionalDependencies(dynamic package)
        {
            try
            {
                return new Dependencies(package, "optionalDependencies");
            }
            catch (RuntimeBinderException rbe)
            {
                throw WrapRuntimeBinderException("optional dependencies", rbe);
            }
        }

        private static IDependencies LoadAllDependencies(dynamic package)
        {
            try
            {
                return new Dependencies(package, "dependencies", "devDependencies", "optionalDependencies");
            }
            catch (RuntimeBinderException rbe)
            {
                throw WrapRuntimeBinderException("all dependencies", rbe);
            }
        }

        private static IEnumerable<string> LoadRequiredBy(dynamic package)
        {
            try
            {
                return (package["_requiredBy"] as IEnumerable<JToken> ?? Enumerable.Empty<JToken>()).Values<string>().ToList();
            }
            catch (RuntimeBinderException rbe)
            {
                System.Diagnostics.Debug.WriteLine(rbe);
                throw WrapRuntimeBinderException("required by", rbe);
            }
        }

        public string Name { get; private set; }

        public SemverVersion Version
        {
            get
            {
                return _versionString == null ? new SemverVersion() : SemverVersion.Parse(_versionString);
            }
        }

        public IPerson Author { get; private set; }

        public string Description { get; private set; }

        public IKeywords Keywords { get; private set; }

        public IHomepages Homepages { get; private set; }

        public ILicenses Licenses { get; private set; }

        public IFiles Files { get; private set; }

        public IDependencies Dependencies { get; private set; }
        public IDependencies DevDependencies { get; private set; }
        public IBundledDependencies BundledDependencies { get; private set; }
        public IDependencies OptionalDependencies { get; private set; }
        public IDependencies AllDependencies { get; private set; }
        public IEnumerable<string> RequiredBy { get; private set; }
    }
}