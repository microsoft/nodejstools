using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools.Commands
{
    internal class MigrateToJspsCommand : Command
    {
        public override int CommandId => (int)PkgCmdId.cmdidJspsProjectMigrate;

        public override void DoCommand(object sender, EventArgs args)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                //Package.GetGlobalService(SVsServiceProvider)
            });
        }
    }
}
