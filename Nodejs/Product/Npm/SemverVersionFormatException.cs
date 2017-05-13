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
using System.Runtime.Serialization;

namespace Microsoft.NodejsTools.Npm {
    [Serializable]
    public class SemverVersionFormatException : FormatException {
        //  I created this class mainly for the purposes of testability. Semver parsing might fail for any
        //  number of reasons with a format exception, which is what I originally used, but since that may
        //  also be thrown by methods called by SemverVersion.Parse, tests can't differentiate correct handling
        //  of bad input versus behaviour that might be a bug.

        public SemverVersionFormatException() { }

        public SemverVersionFormatException(string message) : base(message) { }

        public SemverVersionFormatException(string message, Exception innerException) : base(message, innerException) { }

        protected SemverVersionFormatException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}