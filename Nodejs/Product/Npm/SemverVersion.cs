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

        //  N.B. Both of these are series of dot separated identifiers, but since we don't really particularly
        //  care about them at the moment, can defer comparisons to semver, and won't need to do anything beyond
        //  let the user specify a value, I've just implemented them as strings.
        private string m_PreReleaseVersion;
        private string m_BuildMetadata;

        public SemverVersion( string versionString )
        {
            var parts       = versionString.Split( '-' );
            var normalParts = parts[ 0 ].Split( '.' );

            m_Major = int.Parse( normalParts[0] );
            m_Minor = int.Parse( normalParts[1] );
            m_Patch = int.Parse( normalParts[2] );

            m_PreReleaseVersion = null;
            m_BuildMetadata = null;

            if (parts.Length > 1)
            {
                if ( parts.Length > 2 )
                {
                    throw new ArgumentException(
                        string.Format(
                            "Invalid semantic version: '{0}'. Cannot have more than one pre-release version in a semantic version.",
                            versionString ),
                        "versionString" );
                }

                parts = parts[ 1 ].Split( '+' );
                m_PreReleaseVersion = parts[ 0 ];

                if ( parts.Length > 1 )
                {
                    if ( parts.Length > 2 )
                    {
                        throw new ArgumentException(
                            string.Format(
                                "Invalid semantic version: '{0}'. Cannot have more than one build metadata chunk in a semantic version.",
                                versionString ),
                            "versionString" );
                    }

                    m_BuildMetadata = parts[ 1 ];
                }
            }
        }

        public int Major { get { return m_Major; } }
        public int Minor { get { return m_Minor; } }
        public int Patch { get { return m_Patch; } }

        public bool HasPreReleaseVersion { get { return ! string.IsNullOrEmpty( PreReleaseVersion ); } }
        public string PreReleaseVersion { get { return m_PreReleaseVersion; } }
        public bool HasBuildMetadata { get { return ! string.IsNullOrEmpty( BuildMetadata ); } }
        public string BuildMetadata { get { return m_BuildMetadata; } }

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
