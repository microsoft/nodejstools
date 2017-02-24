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

using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Microsoft.NodejsTools.Profiling
{
    [ComVisible(true)]
    public sealed class AutomationSession : INodeProfileSession
    {
        private readonly SessionNode _node;

        internal AutomationSession(SessionNode session)
        {
            _node = session;
        }

        #region INodeProfileSession Members

        public string Name
        {
            get
            {
                return _node.Name;
            }
        }

        public string Filename
        {
            get
            {
                return _node.Filename;
            }
        }

        public INodePerformanceReport GetReport(object item)
        {
            if (item is int)
            {
                int id = (int)item - 1;
                if (id >= 0 && id < _node.Reports.Count)
                {
                    return new ReportWrapper(_node.Reports.Values.ToArray()[id]);
                }
            }
            else if (item is string)
            {
                string filename = (string)item;
                foreach (var report in _node.Reports.Values)
                {
                    if (filename == report.Filename || Path.GetFileNameWithoutExtension(report.Filename) == filename)
                    {
                        return new ReportWrapper(report);
                    }
                }
            }
            return null;
        }

        public void Launch(bool openReport)
        {
            _node.StartProfiling(openReport);
        }

        public void Save(string filename = null)
        {
            _node.Save(filename);
        }

        public bool IsSaved
        {
            get
            {
                return _node.IsSaved;
            }
        }

        #endregion
    }
}
