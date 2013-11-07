using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Commands
{
    abstract class AbstractNpmCommand : Command
    {
        public NodeModulesNode ModulesNode { get; set; }

        public override EventHandler BeforeQueryStatus
        {
            get { return BeforeQueryStatusImpl; }
        }

        private void BeforeQueryStatusImpl( object source, EventArgs args )
        {
            var node = ModulesNode;
            if ( null != node )
            {
                node.BeforeQueryStatus( source, args );
            }
        }
    }
}
