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
using System.Text.RegularExpressions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace TestUtilities.Mocks {
    public class MockActivityLog : IVsActivityLog {
        public readonly List<string> Items = new List<string>();

        private readonly static Dictionary<uint, string> ActivityType = new Dictionary<uint, string> {
            { (uint)__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR, "Error" },
            { (uint)__ACTIVITYLOG_ENTRYTYPE.ALE_WARNING, "Warning" },
            { (uint)__ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION, "Information" }
        };

        public IEnumerable<string> AllItems {
            get {
                return Items.Select(t => Regex.Replace(t, "(\\r\\n|\\r|\\n)", "\\n"));
            }
        }

        public IEnumerable<string> Errors {
            get {
                return Items
                    .Where(t => t.StartsWith("Error"))
                    .Select(t => Regex.Replace(t, "(\\r\\n|\\r|\\n)", "\\n"));
            }
        }

        public IEnumerable<string> ErrorsAndWarnings {
            get {
                return Items
                    .Where(t => t.StartsWith("Error") || t.StartsWith("Warning"))
                    .Select(t => Regex.Replace(t, "(\\r\\n|\\r|\\n)", "\\n"));
            }
        }

        public int LogEntry(uint actType, string pszSource, string pszDescription) {
            Items.Add(string.Format("{0}//{1}//{2}", ActivityType[actType], pszSource, pszDescription));
            return VSConstants.S_OK;
        }

        public int LogEntryGuid(uint actType, string pszSource, string pszDescription, Guid guid) {
            Items.Add(string.Format("{0}//{1}//{2}//{3:B}", ActivityType[actType], pszSource, pszDescription, guid));
            return VSConstants.S_OK;
        }

        public int LogEntryGuidHr(uint actType, string pszSource, string pszDescription, Guid guid, int hr) {
            Items.Add(string.Format("{0}//{1}//{2}//{3:B}//{4:X8}", ActivityType[actType], pszSource, pszDescription, guid, hr));
            return VSConstants.S_OK;
        }

        public int LogEntryGuidHrPath(uint actType, string pszSource, string pszDescription, Guid guid, int hr, string pszPath) {
            Items.Add(string.Format("{0}//{1}//{2}//{3:B}//{4:X8}//{5}", ActivityType[actType], pszSource, pszDescription, guid, hr, pszPath));
            return VSConstants.S_OK;
        }

        public int LogEntryGuidPath(uint actType, string pszSource, string pszDescription, Guid guid, string pszPath) {
            Items.Add(string.Format("{0}//{1}//{2}//{3:B}//{4}", ActivityType[actType], pszSource, pszDescription, guid, pszPath));
            return VSConstants.S_OK;
        }

        public int LogEntryHr(uint actType, string pszSource, string pszDescription, int hr) {
            Items.Add(string.Format("{0}//{1}//{2}//{3:X8}", ActivityType[actType], pszSource, pszDescription, hr));
            return VSConstants.S_OK;
        }

        public int LogEntryHrPath(uint actType, string pszSource, string pszDescription, int hr, string pszPath) {
            Items.Add(string.Format("{0}//{1}//{2}//{3:X8}//{4}", ActivityType[actType], pszSource, pszDescription, hr, pszPath));
            return VSConstants.S_OK;
        }

        public int LogEntryPath(uint actType, string pszSource, string pszDescription, string pszPath) {
            Items.Add(string.Format("{0}//{1}//{2}//{3}", ActivityType[actType], pszSource, pszDescription, pszPath));
            return VSConstants.S_OK;
        }
    }
}
