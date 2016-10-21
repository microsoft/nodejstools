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
using Microsoft.NodejsTools.Project;
using System.Globalization;

namespace Microsoft.NodejsTools.NpmUI {
    internal class LastRefreshedMessageProvider {
        public static readonly LastRefreshedMessageProvider RefreshFailed = new LastRefreshedMessageProvider {
            Days = int.MaxValue,
            Description = Resources.PackageCatalogRefreshFailed
        };

        public static readonly LastRefreshedMessageProvider RefreshInProgress = new LastRefreshedMessageProvider {
            Days = 0,
            Description = Resources.PackageCatalogRefreshing
        };

        public static readonly LastRefreshedMessageProvider NpmNotFound = new LastRefreshedMessageProvider {
            Days = int.MaxValue,
            Description = "npm not installed"
        };

        private LastRefreshedMessageProvider() { }

        public LastRefreshedMessageProvider(DateTime lastRefreshTime) {
            if (lastRefreshTime == DateTime.MinValue) {
                Days = int.MaxValue;
                Description = Resources.PackageCatalogRefreshFailed;
            } else {
                Days = (int)(DateTime.Now.Date - lastRefreshTime.Date).TotalDays;
                if (Days == 0) {
                    Description = string.Format(CultureInfo.CurrentCulture, Resources.PackageCatalogRefresh0Days, lastRefreshTime);
                } else if (Days == 1) {
                    Description = string.Format(CultureInfo.CurrentCulture, Resources.PackageCatalogRefresh1Day, lastRefreshTime);
                } else if (Days <= 7) {
                    Description = string.Format(CultureInfo.CurrentCulture, Resources.PackageCatalogRefresh2To7Days, Days);
                } else if (Days <= 14) {
                    Description = Resources.PackageCatalogRefresh1Week;
                } else if (Days <= 21) {
                    Description = Resources.PackageCatalogRefresh2Weeks;
                } else if (Days <= 31) {
                    Description = Resources.PackageCatalogRefresh3Weeks;
                } else if (Days <= 92) {
                    Description = Resources.PackageCatalogRefresh1Month;
                } else {
                    Description = Resources.PackageCatalogRefresh3Months;
                }
            }
        }

        public int Days { get; private set; }

        public string Description { get; private set; }

        public bool IsOld { get { return Days > 7; } }
        public bool IsAncient { get { return Days > 14; } }
    }
}
