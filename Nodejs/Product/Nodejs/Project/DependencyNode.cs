using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project
{
    class DependencyNode : HierarchyNode
    {

        private readonly NodejsProjectNode _projectNode;
        private readonly DependencyNode _parent;
        private readonly string _displayString;

        public DependencyNode(
            NodejsProjectNode root,
            DependencyNode parent,
            IPackage package )
        {
            _projectNode = root;
            _parent = parent;
            Package = package;

            var buff = new StringBuilder(package.Name);
            if ( package.IsMissing )
            {
                buff.Append( " (missing)" );
            }
            else
            {
                buff.Append( '@' );
                buff.Append( package.Version );

                if ( ! package.IsListedInParentPackageJson )
                {
                    buff.Append( " (not listed)" );
                }
                else if ( package.IsDevDependency )
                {
                    buff.Append( " (dev)" );
                }
                else if ( package.IsOptionalDependency )
                {
                    buff.Append( " (optional)" );
                }
            }

            if ( package.IsBundledDependency )
            {
                buff.Append( "[bundled]" );
            }

            _displayString = buff.ToString();
            ExcludeNodeFromScc = true;
        }

        public IPackage Package { get; private set; }

        #region HierarchyNode implementation

        private string GetRelativeUrlFragment()
        {
            var buff = new StringBuilder();
            if ( null != _parent )
            {
                buff.Append( _parent.GetRelativeUrlFragment() );
                buff.Append( '/' );
            }
            buff.Append( "node_modules/" );
            buff.Append( Package.Name );
            return buff.ToString();
        }

        public override string Url
        {
            get
            {
                return new Url( ProjectMgr.BaseURI, GetRelativeUrlFragment() ).AbsoluteUrl;
            }
        }

        public override string Caption
        {
            get { return _displayString; }
        }

        public override Guid ItemTypeGuid
        {
            get { return VSConstants.GUID_ItemType_VirtualFolder; }
        }

        public override object GetIconHandle( bool open )
        {
            int imageIndex = _projectNode.ImageIndexDependency;
            if (Package.IsMissing)
            {
                imageIndex = Package.IsDevDependency
                    ? _projectNode.ImageIndexDependencyDevMissing
                    : ( Package.IsOptionalDependency
                        ? _projectNode.ImageIndexDependencyOptionalMissing
                        : ( Package.IsBundledDependency
                            ? _projectNode.ImageIndexDependencyOptionalMissing
                            : _projectNode.ImageIndexDependencyMissing ) );
            }
            else
            {
                imageIndex = Package.IsListedInParentPackageJson
                    ? _projectNode.ImageIndexDependencyNotListed
                    : ( Package.IsDevDependency
                        ? _projectNode.ImageIndexDependencyDev
                        : ( Package.IsOptionalDependency
                            ? _projectNode.ImageIndexDependnecyOptional
                            : ( Package.IsBundledDependency
                                ? _projectNode.ImageIndexDependencyBundled
                                : _projectNode.ImageIndexDependencyMissing ) ) );
            }

            return _projectNode.ImageHandler.GetIconHandle( imageIndex );
        }

        public override string GetEditLabel()
        {
            return null;
        }

        #endregion

        #region Dependency actions

        public async void Uninstall()
        {
            var modulesNode = _projectNode.ModulesNode;
            if ( null != modulesNode )
            {
                await modulesNode.NpmController.UninstallPackageAsync(Package.Name);
            }
        }

        #endregion
    }
}
