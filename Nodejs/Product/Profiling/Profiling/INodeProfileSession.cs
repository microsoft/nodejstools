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
using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools.Profiling {
    [Guid("7C711031-50B4-4263-901E-9EF86DD6DC57")]
    public interface INodeProfileSession {
        string Name {
            get;
        }

        string Filename {
            get;
        }

        INodePerformanceReport GetReport(object item);

        void Save(string filename = null);

        void Launch(bool openReport = false);

        bool IsSaved {
            get;
        }
    }
}
