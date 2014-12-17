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


namespace Microsoft.NodejsTools.Interpreter {
#if FALSE
    /// <summary>
    /// Represents a built-in property which has a getter/setter.  
    /// </summary>
    public interface IBuiltinProperty : IMember {
        /// <summary>
        /// The type of the value the property gets/sets.
        /// </summary>
        IPythonType Type {
            get;
        }

        /// <summary>
        /// True if the property is static (declared on the class) not the instance.
        /// </summary>
        bool IsStatic {
            get;
        }

        /// <summary>
        /// Documentation for the property.
        /// </summary>
        string Documentation {
            get;
        }

        /// <summary>
        /// A user readable description of the property.
        /// </summary>
        string Description {
            get;
        }
    }
#endif
}
