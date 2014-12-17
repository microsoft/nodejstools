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

namespace Microsoft.NodejsTools.Interpreter {
    /// <summary>
    /// The options that may be passed to
    /// <see cref="IPythonInterpreterFactoryWithDatabase.GenerateDatabase"/>
    /// </summary>
    [Flags]
    public enum GenerateDatabaseOptions {
        /// <summary>
        /// Runs a full analysis for the interpreter's standard library and
        /// installed packages.
        /// </summary>
        None,
        /// <summary>
        /// Skips analysis if the modification time of every file in a package
        /// is earlier than the database's time. This option prefers false
        /// negatives (that is, analyze something that did not need it) if it is
        /// likely that the results could be outdated.
        /// </summary>
        SkipUnchanged
    }
}
