using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class NpmUpdateCommand : NpmCommand
    {
        public NpmUpdateCommand( string fullPathToRootPackageDirectory, string pathToNpm = null )
            : this( fullPathToRootPackageDirectory, new List<IPackage>(), pathToNpm )
        {
        }

        public NpmUpdateCommand(
            string fullPathToRootPackageDirectory,
            IEnumerable<IPackage> packages, 
            string pathToNpm = null ) : base( fullPathToRootPackageDirectory, pathToNpm )
        {
            var buff = new StringBuilder( "update" );
            foreach ( var package in packages )
            {
                buff.Append( ' ' );
                buff.Append( package.Name );
            }
            buff.Append( " --save" );
            Arguments = buff.ToString();
        }
    }
}
