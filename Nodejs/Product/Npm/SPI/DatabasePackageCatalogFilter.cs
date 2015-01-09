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
using Microsoft.NodejsTools.Npm.SQLiteTables;
using SQLite;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class DatabasePackageCatalogFilter : IPackageCatalogFilter {

        private readonly string _dbFilename;

        public DatabasePackageCatalogFilter(string dbFilename) {
            _dbFilename = dbFilename;
        }

        public IEnumerable<IPackage> Filter(string filterString) {
            filterString = filterString ?? string.Empty;

            // Escape all quotes so that the sql query is valid. Also replace hyphens with spaces
            // so that we can search text including hyphens because FTS uses '-' as a NOT operator.
            // Lastly, trim whitespace because otherwise no results will be found.
            string escapedFilterString = filterString.Replace("'", "''").Replace('-', ' ').Trim();

            using (var db = new SQLiteConnection(_dbFilename)) {
                if (filterString.Length >= 3) {
                    return db.Query<CatalogEntry>(
                        string.Format("SELECT * from CatalogEntry WHERE CatalogEntry MATCH 'name:{0}* OR description:{0}* OR keywords:{0}*' COLLATE NOCASE", escapedFilterString)).
                        OrderBy(entry => entry, new CatalogEntryComparer(filterString)).AsEnumerable().Select(entry => entry.ToPackage());
                } else if (filterString.Length >= 1) {
                    return db.Query<CatalogEntry>(
                        string.Format("SELECT * from CatalogEntry WHERE name MATCH '{0}*' COLLATE NOCASE", escapedFilterString)).
                        OrderBy(entry => entry, new CatalogEntryComparer(filterString)).AsEnumerable().Select(entry => entry.ToPackage());
                }

                return db.Query<CatalogEntry>("SELECT * from CatalogEntry").OrderBy(entry => entry, new CatalogEntryComparer(filterString)).AsEnumerable().Select(entry => entry.ToPackage());
            }
        }

        /// <summary>
        /// Sorts by location of filter string in name, then keywords.
        /// The earlier the filter string occurs, the earlier it gets sorted.
        /// If the filter strings occur at the same location, the name with the shorter length gets sorted first.
        /// </summary>
        class CatalogEntryComparer : IComparer<CatalogEntry> {
            private readonly string _filterText;

            public CatalogEntryComparer(string filterText) {
                _filterText = filterText;
            }

            public int Compare(CatalogEntry x, CatalogEntry y) {
                if (string.IsNullOrEmpty(_filterText)) {
                    return String.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
                }
                return CompareEntryStrings(x.Name, y.Name) ?? CompareEntryStrings(x.Keywords, y.Keywords) ?? String.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            }

            private int? CompareEntryStrings(string x, string y) {
                var xIndex = string.IsNullOrEmpty(x) ? -1 : x.ToLower().IndexOf(_filterText, StringComparison.OrdinalIgnoreCase);
                var yIndex = string.IsNullOrEmpty(y) ? -1 : y.ToLower().IndexOf(_filterText, StringComparison.OrdinalIgnoreCase);

                int? xEqualsY = null;
                const int xBeforeY = -1;
                const int yBeforeX = 1;

                if (xIndex == -1 && yIndex == -1) {
                    return xEqualsY;
                } else if (xIndex == -1 && yIndex >= 0) {
                    return yBeforeX;
                } else if (yIndex == -1 && xIndex >= 0) {
                    return xBeforeY;
                } else if (xIndex < yIndex) {
                    return xBeforeY;
                } else if (yIndex < xIndex) {
                    return yBeforeX;
                } else if (x.Length < y.Length) {
                    return xBeforeY;
                } else if (y.Length < x.Length) {
                    return yBeforeX;
                }

                return xEqualsY;
            }
        }
    }
}