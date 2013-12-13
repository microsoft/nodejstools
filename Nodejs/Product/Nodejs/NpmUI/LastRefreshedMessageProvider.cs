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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.NpmUI
{
    internal static class LastRefreshedMessageProvider
    {

        public static int GetNumberOfDaysSinceLastRefresh(DateTime lastRefreshTime){
            if (DateTime.MinValue == lastRefreshTime){
                return -1;
            }


        }

        public static string GetMessageFor(DateTime lastRefreshTime){
            var days = GetNumberOfDaysSinceLastRefresh(lastRefreshTime);
            switch (days){
                case -1:
                    return Resources.PackageCatalogRefreshNever;

                case 0:
                    return string.Format(
                        Resources.PackageCatalogRefreshODays,
                        lastRefreshTime.ToShortTimeString());

                case 1:
                    return string.Format(
                        Resources.PackageCatalogRefresh1Day,
                        lastRefreshTime.ToShortTimeString());

                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    return string.Format(
                        Resources.PackageCatalogRefresh2To7Days,
                        days);

                default:
                    if (days > 183){
                        return Resources.PackageCatalogRefresh6Months;
                    }
                    
                    if (days > 92){
                        return Resources.PackageCatalogRefresh3Months;
                    }

                    if (days > 31){
                        return Resources.PackageCatalogRefresh1Month;
                    }

                    if (days > 21){
                        return Resources.PackageCatalogRefresh3Weeks;
                    }

                    if (days > 14){
                        return Resources.PackageCatalogRefresh2Weeks;
                    }

                    return Resources.PackageCatalogRefresh1Week;
            }
        }
    }
}
