using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal abstract class AbstractNpmSearchComparer : IComparer<IPackage> {

        protected string BuildKeywordString(IPackage source) {
            var buffer = new StringBuilder();
            foreach (var keyword in source.Keywords) {
                buffer.Append(keyword);
            }
            return buffer.ToString();
        }

        protected int CompareBasedOnKeywords(IPackage x, IPackage y) {
            return string.Compare(BuildKeywordString(x), BuildKeywordString(y), StringComparison.CurrentCulture);
        }

        public abstract int Compare(IPackage x, IPackage y);
    }
}
