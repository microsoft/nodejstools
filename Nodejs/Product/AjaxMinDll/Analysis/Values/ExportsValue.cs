using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Interpreter;

namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// The object that is created for a Node.js module's
    /// exports variable.  We create this so that we can show
    /// a different icon in intellisense for modules.

    /// </summary>
    class ExportsValue : ObjectValue {
        public ExportsValue(ProjectEntry projectEntry)
            : base(projectEntry) {
        }

        public override NodejsMemberType MemberType {
            get {
                return NodejsMemberType.Module;
            }
        }
    }
}
