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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.NodejsTools.Analysis.Values {
    static class AnalysisValueExtensions {
        public static string GetStringValue(this AnalysisValue value) {
            StringValue str = value as StringValue;
            if (str != null) {
                return str._value;
            }
            return null;
        }

        public static double? GetNumberValue(this AnalysisValue value) {
            NumberValue number = value as NumberValue;
            if (number != null) {
                return number._value;
            }
            return null;
        }
    }
}
