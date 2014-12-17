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

namespace Microsoft.NodejsTools.SourceMapping {

    internal class FunctionInformation {
        internal readonly string Namespace;
        internal readonly string Function;
        internal readonly string Filename;
        internal readonly int? LineNumber;
        internal readonly bool IsRecompilation;

        internal FunctionInformation(string ns, string methodName, int? lineNo, string filename) : this(ns, methodName, lineNo, filename, false) { }

        internal FunctionInformation(string ns, string methodName, int? lineNo, string filename, bool isRecompilation) {
            Namespace = ns;
            Function = methodName;
            LineNumber = lineNo;
            Filename = filename;
            IsRecompilation = isRecompilation;
        }
    }
}
