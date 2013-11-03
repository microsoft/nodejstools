using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    public class PackageComparer : IComparer<IPackage>
    {
        public int Compare(IPackage x, IPackage y)
        {
            if (x == y)
            {
                return 0;
            }
            else if (null == x)
            {
                return -1;
            }
            else if (null == y)
            {
                return 1;
            }

            return x.Name.CompareTo(y.Name);
        }
    }
}
