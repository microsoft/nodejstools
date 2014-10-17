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
using Microsoft.NodejsTools.Project;

namespace Microsoft.NodejsTools.NpmUI {
    internal class LastRefreshedMessageProvider {
        public static readonly LastRefreshedMessageProvider RefreshFailed = new LastRefreshedMessageProvider {
            Days = int.MaxValue,
            Description = SR.GetString(SR.PackageCatalogRefreshFailed)
        };

        public static readonly LastRefreshedMessageProvider RefreshInProgress = new LastRefreshedMessageProvider {
            Days = 0,
            Description = SR.GetString(SR.PackageCatalogRefreshing)
        };

        public static readonly LastRefreshedMessageProvider NpmNotFound = new LastRefreshedMessageProvider {
            Days = int.MaxValue,
            Description = "npm not installed"
        };

        private LastRefreshedMessageProvider() { }

        public LastRefreshedMessageProvider(DateTime lastRefreshTime) {
            if (lastRefreshTime == DateTime.MinValue) {
                Days = -1;
                Description = SR.GetString(SR.PackageCatalogRefreshNever);
            } else {
                Days = (int)(DateTime.Now.Date - lastRefreshTime.Date).TotalDays;
                if (Days == 0) {
                    Description = SR.GetString(SR.PackageCatalogRefresh0Days, lastRefreshTime);
                } else if (Days == 1) {
                    Description = SR.GetString(SR.PackageCatalogRefresh1Day, lastRefreshTime);
                } else if (Days <= 7) {
                    Description = SR.GetString(SR.PackageCatalogRefresh2To7Days, Days);
                } else if (Days <= 14) {
                    Description = SR.GetString(SR.PackageCatalogRefresh1Week);
                } else if (Days <= 21) {
                    Description = SR.GetString(SR.PackageCatalogRefresh2Weeks);
                } else if (Days <= 31) {
                    Description = SR.GetString(SR.PackageCatalogRefresh3Weeks);
                } else if (Days <= 92) {
                    Description = SR.GetString(SR.PackageCatalogRefresh1Month);
                } else {
                    Description = SR.GetString(SR.PackageCatalogRefresh3Months);
                }
            }
        }

        public int Days { get; private set; }

        public string Description { get; private set; }

        public bool IsOld { get { return Days > 7; } }
        public bool IsAncient { get { return Days > 14; } }
    }
}
