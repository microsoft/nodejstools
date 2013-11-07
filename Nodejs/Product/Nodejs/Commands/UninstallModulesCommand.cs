using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Commands
{
    class UninstallModulesCommand : AbstractNpmCommand
    {
        public override void DoCommand( object sender, EventArgs args )
        {
            var node = ModulesNode;
            if ( null != node )
            {
                node.UninstallModules();
            }
        }

        public override int CommandId
        {
            get { return PkgCmdId.cmdidNpmUninstallModule; }
        }
    }
}
