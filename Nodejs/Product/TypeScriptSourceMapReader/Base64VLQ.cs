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

namespace TypeScriptSourceMapReader
{
    /// <summary>
    /// Class that decodes Base64 format
    /// </summary>
    internal class Base64Format
    {
        private static string encodedValues = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        /// <summary>
        /// Decode given Base64 encoded character
        /// </summary>
        /// <param name="inChar">Bas64 encoded character</param>
        /// <returns>Decoded value</returns>
        static internal int decodeChar(char inChar)
        {
            return Base64Format.encodedValues.IndexOf(inChar);
        }
    }
}
