using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class NodeModulesProxy : AbstractNodeModules
    {
        public new void AddModule( IPackage package )
        {
            base.AddModule( package );
        }
    }
}
