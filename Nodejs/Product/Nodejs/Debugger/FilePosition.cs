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

namespace Microsoft.NodejsTools.Debugger
{
    /// <summary>
    /// Stores line and column position in the file.
    /// </summary>
    internal sealed class FilePosition
    {
        public readonly int Column;
        public readonly string FileName;
        public readonly int Line;

        public FilePosition(string fileName, int line, int column)
        {
            this.FileName = fileName;
            this.Line = line;
            this.Column = column;
        }
    }
}