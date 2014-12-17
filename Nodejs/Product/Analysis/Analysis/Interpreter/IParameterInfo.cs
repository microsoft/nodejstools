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

namespace Microsoft.NodejsTools.Interpreter {
#if FALSE
    /// <summary>
    /// Represents information about an individual parameter.  Used for providing
    /// signature help.
    /// </summary>
    public interface IParameterInfo {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        string Name {
            get;
        }

        /// <summary>
        /// The types of the parameter.
        /// </summary>
        IList<IPythonType> ParameterTypes {
            get;
        }

        /// <summary>
        /// Documentation for the parameter.
        /// </summary>
        string Documentation {
            get;
        }

        /// <summary>
        /// True if the parameter is a *args parameter.
        /// </summary>
        bool IsParamArray {
            get;
        }

        /// <summary>
        /// True if the parameter is a **args parameter.
        /// </summary>
        bool IsKeywordDict {
            get;
        }

        /// <summary>
        /// Default value.  Returns String.Empty for optional parameters, or a string representation of the default value
        /// </summary>
        string DefaultValue {
            get;
        }
    }
#endif
}
