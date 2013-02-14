using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PythonTools.Project;
using System.Runtime.InteropServices;

namespace Microsoft.NodeTools.Project {
    [Guid("62E8E091-6914-498E-A47B-6F198DC1873D")]
    class NodeGeneralPropertyPage : CommonPropertyPage {
        public override System.Windows.Forms.Control Control {
            get { throw new NotImplementedException(); }
        }

        public override void Apply() {
        }

        public override void LoadSettings() {
        }

        public override string Name {
            get { return "General"; }
        }
    }
}
