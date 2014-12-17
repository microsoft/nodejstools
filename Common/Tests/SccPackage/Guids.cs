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

// Guids.cs
// MUST match guids.h
using System;

namespace Microsoft.TestSccPackage
{
    static class Guids
    {
        public const string guidSccPackagePkgString = "49e63f87-6b69-42ca-9496-1e20a919ef1f";
        public const string guidSccPackageCmdSetString = "045cf08e-e640-42c4-af80-0251d6f553a1";

        public static readonly Guid guidSccPackageCmdSet = new Guid(guidSccPackageCmdSetString);
    };
}