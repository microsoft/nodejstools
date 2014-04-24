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
        private readonly string _name;  // for debugging

        public ExportsValue(string name, ProjectEntry projectEntry)
            : base(projectEntry) {
            _name = name;
        }

        public override JsMemberType MemberType {
            get {
                return JsMemberType.Module;
            }
        }
    }
}
