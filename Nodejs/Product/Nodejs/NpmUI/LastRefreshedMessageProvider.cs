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

namespace Microsoft.NodejsTools.NpmUI
{
    internal class LastRefreshedMessageProvider
    {
        public static readonly LastRefreshedMessageProvider RefreshFailed = new LastRefreshedMessageProvider
        {
            Days = int.MaxValue,
            Description = Resources.PackageCatalogRefreshFailed
        };

        public static readonly LastRefreshedMessageProvider RefreshInProgress = new LastRefreshedMessageProvider
        {
            Days = 0,
            Description = Resources.PackageCatalogRefreshing
        };

        public static readonly LastRefreshedMessageProvider NpmNotFound = new LastRefreshedMessageProvider
        {
            Days = int.MaxValue,
            Description = "npm not installed"
        };

        private LastRefreshedMessageProvider() { }

        public LastRefreshedMessageProvider(DateTime lastRefreshTime)
        {
            if (lastRefreshTime == DateTime.MinValue)
            {
                this.Days = int.MaxValue;
                this.Description = Resources.PackageCatalogRefreshFailed;
            }
            else
            {
                this.Days = (int)(DateTime.Now.Date - lastRefreshTime.Date).TotalDays;
                if (this.Days == 0)
                {
                    this.Description = string.Format(CultureInfo.CurrentCulture, Resources.PackageCatalogRefresh0Days, lastRefreshTime);
                }
                else if (this.Days == 1)
                {
                    this.Description = string.Format(CultureInfo.CurrentCulture, Resources.PackageCatalogRefresh1Day, lastRefreshTime);
                }
                else if (this.Days <= 7)
                {
                    this.Description = string.Format(CultureInfo.CurrentCulture, Resources.PackageCatalogRefresh2To7Days, this.Days);
                }
                else if (this.Days <= 14)
                {
                    this.Description = Resources.PackageCatalogRefresh1Week;
                }
                else if (this.Days <= 21)
                {
                    this.Description = Resources.PackageCatalogRefresh2Weeks;
                }
                else if (this.Days <= 31)
                {
                    this.Description = Resources.PackageCatalogRefresh3Weeks;
                }
                else if (this.Days <= 92)
                {
                    this.Description = Resources.PackageCatalogRefresh1Month;
                }
                else
                {
                    this.Description = Resources.PackageCatalogRefresh3Months;
                }
            }
        }

        public int Days { get; private set; }

        public string Description { get; private set; }
    }
}
