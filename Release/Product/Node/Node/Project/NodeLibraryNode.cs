using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PythonTools.Navigation;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodeTools.Project {
    class NodeLibraryNode : CommonLibraryNode {
        public NodeLibraryNode(IScopeNode scope, string namePrefix, IVsHierarchy hierarchy, uint itemId) :
            base(scope, namePrefix, hierarchy, itemId) {
        }
    }
}
