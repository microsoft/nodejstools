using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PythonTools.Navigation;
using System.Runtime.InteropServices;
using Microsoft.PythonTools.Project;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Project {

    /// <summary>
    /// Implementation of the service that builds the information to expose to the symbols
    /// navigation tools (class view or object browser) from the Python files inside a
    /// hierarchy.
    /// </summary>
    [Guid("1CCB584B-2876-4416-99B0-60C91B938147")]
    internal class NodeLibraryManager : LibraryManager {
        private readonly NodePackage/*!*/ _package;

        public NodeLibraryManager(NodePackage/*!*/ package)
            : base(package) {
            _package = package;
        }

        protected override LibraryNode CreateLibraryNode(IScopeNode subItem, string namePrefix, IVsHierarchy hierarchy, uint itemid) {
            return new NodeLibraryNode(subItem, namePrefix, hierarchy, itemid);
        }

        public override LibraryNode CreateFileLibraryNode(HierarchyNode hierarchy, string name, string filename, LibraryNodeType libraryNodeType) {
            return new NodeFileLibraryNode(hierarchy, hierarchy.Caption, filename, libraryNodeType);
        }

        protected override void OnNewFile(LibraryTask task) {
        }
    }
}
