/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;

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

        private LastRefreshedMessageProvider() { }

        public LastRefreshedMessageProvider(DateTime lastRefreshTime) {
            if (lastRefreshTime == DateTime.MinValue) {
                Days = -1;
                Description = Resources.PackageCatalogRefreshNever;
            } else {
                Days = (int)(DateTime.Now.Date - lastRefreshTime.Date).TotalDays;
                if (Days == 0) {
                    Description = string.Format(Resources.PackageCatalogRefresh0Days, lastRefreshTime);
                } else if (Days == 1) {
                    Description = string.Format(Resources.PackageCatalogRefresh1Day, lastRefreshTime);
                } else if (Days <= 7) {
                    Description = string.Format(Resources.PackageCatalogRefresh2To7Days, Days);
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
