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


// This file contains Reflectio-based shims for interfaces from the Microsoft.VisualStudio.Web.WindowsAzure.Contracts.
// They are used for the time being because the interfaces in question are only going to be stabilized in Visual Studio 2013 Update 2,
// and until then using them directly requires private builds of Azure Tools to build against. Using Reflection avoids that dependency.
// The interfaces correspond directly to those in the contracts assembly, but only those members that are actually used by our code are
// exposed and shimmed.
//
// TODO: get rid of the shims and use the contracts assembly directly once Update 2 RTM is out.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.WindowsAzure.Authentication;

namespace Microsoft.VisualStudio.Web.WindowsAzure.Contracts.Shims {
    internal class ContractShim<T> {
        private static readonly Assembly contractAssembly = Assembly.Load("Microsoft.VisualStudio.Web.WindowsAzure.Contracts, Version=2.3.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
        private static readonly Type _interface;
        private readonly object _impl;

        static ContractShim() {
            string shimName = typeof(T).FullName;
            Debug.Assert(shimName.StartsWith("Microsoft.VisualStudio.Web.WindowsAzure.Contracts.Shims."));
            string realName = shimName.Replace(".Shims.", ".");
            _interface = contractAssembly.GetType(realName, throwOnError: true);
        }

        protected ContractShim(object impl) {
            _impl = impl;
        }

        public static bool CanShim(object impl) {
            return impl != null && impl.GetType().GetInterfaces().Contains(_interface);
        }

        private object InvokeByName(string name, object[] args) {
            Type[] types = args != null ?
                args.Select(arg => { return arg.GetType(); }).ToArray() :
                new Type[] { };
            var method =
                new[] { _interface }
                .Concat(_interface.GetInterfaces())
                .Select(t => t.GetMethod(name, types))
                .Where(m => m != null)
                .FirstOrDefault();
            if (method == null) {
                throw new MissingMethodException(_interface.FullName, name);
            }
            try {
                return method.Invoke(_impl, args);
            } catch (TargetInvocationException tex) {
                ExceptionDispatchInfo.Capture(tex.InnerException ?? tex).Throw();
                return null;
            }
        }

        protected object Invoke([CallerMemberName] string name = null) {
            return InvokeByName(name, null);
        }

        protected object Invoke(object arg1, [CallerMemberName] string name = null) {
            return InvokeByName(name, new[] { arg1 });
        }

        protected object Invoke(object arg1, object arg2, [CallerMemberName] string name = null) {
            return InvokeByName(name, new[] { arg1, arg2 });
        }

        protected object Get([CallerMemberName] string name = null) {
            var prop =
                new[] { _interface }
                .Concat(_interface.GetInterfaces())
                .Select(t => t.GetProperty(name))
                .Where(p => p != null)
                .FirstOrDefault();
            if (prop == null) {
                throw new MissingMemberException(_interface.FullName, name);
            }
            try {
                return prop.GetValue(_impl);
            } catch (TargetInvocationException tex) {
                ExceptionDispatchInfo.Capture(tex.InnerException ?? tex).Throw();
                return null;
            }
        }
    }

    [Guid("D756EDAE-0998-42AA-AA02-6B4FD029E731")]
    internal interface IVsAzureServices {
        IAzureWebSitesService GetAzureWebSitesService();
    }

    internal class VsAzureServicesShim : ContractShim<IVsAzureServices>, IVsAzureServices {
        public VsAzureServicesShim(object impl)
            : base(impl) {
        }

        public IAzureWebSitesService GetAzureWebSitesService() {
            return new AzureWebSitesServiceShim(Invoke());
        }
    }

    internal interface IAzureService {
        Task<List<IAzureSubscription>> GetSubscriptionsAsync();
    }

    internal interface IAzureWebSitesService : IAzureService {
    }

    internal class AzureWebSitesServiceShim : ContractShim<IAzureWebSitesService>, IAzureWebSitesService {
        public AzureWebSitesServiceShim(object impl)
            : base(impl) {
        }
        
        public Task<List<IAzureSubscription>> GetSubscriptionsAsync() {
            return ((Task)Invoke()).ContinueWith(task =>
                ((IEnumerable<object>)((dynamic)task).Result)
                .Select(item => (IAzureSubscription)new AzureSubscriptionShim(item))
                .ToList()
            );
        }
    }

    internal interface IAzureSubscription {
        string SubscriptionId { get; }
        Uri ServiceManagementEndpointUri { get; }
        IAzureSubscriptionContext AzureCredentials { get; }
        Task<List<IAzureResource>> GetResourcesAsync(bool refresh);
    }

    internal class AzureSubscriptionShim : ContractShim<IAzureSubscription>, IAzureSubscription {
        public AzureSubscriptionShim(object impl)
            : base(impl) {
        }

        public string SubscriptionId {
            get { return (string)Get(); }
        }

        public Uri ServiceManagementEndpointUri {
            get { return (Uri)Get(); }
        }

        public IAzureSubscriptionContext AzureCredentials {
            get { return (IAzureSubscriptionContext)Get(); }
        }

        public Task<List<IAzureResource>> GetResourcesAsync(bool refresh) {
            // The caller will only use websites from this list, and we don't
            // want to shim other resource types, so filter them out.
            return ((Task)Invoke(refresh)).ContinueWith(task =>
                ((IEnumerable<object>)((dynamic)task).Result)
                .Where(item => AzureWebSiteShim.CanShim(item))
                .Select(item => (IAzureResource)new AzureWebSiteShim(item))
                .ToList()
            );
        }
    }

    internal interface IAzureResource {
        string Name { get; }
    }

    internal interface IAzureWebSite : IAzureResource {
        string WebSpace { get; }
        string BrowseURL { get; }
    }

    internal class AzureWebSiteShim : ContractShim<IAzureWebSite>, IAzureWebSite {
        public AzureWebSiteShim(object impl)
            : base(impl) {
        }

        public string WebSpace {
            get { return (string)Get(); }
        }

        public string BrowseURL {
            get { return (string)Get(); }
        }

        public string Name {
            get { return (string)Get(); }
        }
    }
}