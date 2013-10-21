using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{

    /// <summary>
    /// Represents a semantic version as defined at http://semver.org/
    /// and used by the npm semantic versioner: https://npmjs.org/doc/misc/semver.html.
    /// </summary>
    public struct SemverVersion
    {

        private int m_Major;
        private int m_Minor;
        private int m_Patch;

        public SemverVersion( string versionString )
        {
            var parts = versionString.Split( '.' );
            m_Major = int.Parse( parts[ 0 ] );
            m_Minor = int.Parse( parts[ 1 ] );
            m_Patch = int.Parse( parts[ 2 ] );
        }

        public int Major { get { return m_Major; } }
        public int Minor { get { return m_Minor; } }
        public int Patch { get { return m_Patch; } }

        public override string ToString()
        {
            return string.Format( "{0}.{1}.{2}", Major, Minor, Patch );
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
