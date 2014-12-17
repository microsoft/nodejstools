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

using System;
using System.Globalization;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Microsoft.VisualStudioTools.TestAdapter {
    internal static class ServiceProviderExtensions {
        public static T GetService<T>(this IServiceProvider serviceProvider)
            where T : class {
            return serviceProvider.GetService<T>(typeof(T));
        }

        public static T GetService<T>(this IServiceProvider serviceProvider, Type serviceType)
            where T : class {
            ValidateArg.NotNull(serviceProvider, "serviceProvider");
            ValidateArg.NotNull(serviceType, "serviceType");

            var serviceInstance = serviceProvider.GetService(serviceType) as T;
            if (serviceInstance == null) {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, serviceType.Name));
            }

            return serviceInstance;
        }
    }
}
