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

        public static SemverVersion Parse( string versionString )
        {
            string  normalPart = versionString,
                    optionalParts = null;
            var index = normalPart.IndexOf( '-' );

            if ( index >= 0 )
            {
                normalPart      = normalPart.Substring( 0, index );
                optionalParts   = versionString.Substring( index + 1 );
            }

            var normalParts = normalPart.Split('.');
            if (normalParts.Length != 3)
            {
                throw new SemverVersionFormatException(
                    string.Format("Invalid semantic version: '{0}'. Unable to parse normal version from token '{1}'. The version number must consist of three non-negative numeric parts of the form MAJOR.MINOR.PATCH, with optional pre-release and/or build metadata.",
                    versionString,
                    normalParts.Length > 0 ? normalParts[0] : "<<no token available>>" ));
            }

            int major,
                minor,
                patch;

            try
            {
                major = int.Parse(normalParts[0]);
                minor = int.Parse(normalParts[1]);
                patch = int.Parse( normalParts[ 2 ] );
            }
            catch ( FormatException fe )
            {
                throw new SemverVersionFormatException(
                    string.Format(
                        "Invalid semantic version: '{0}'. Unable to parse normal version from token '{1}'. The version number must consist of three numeric parts of the form MAJOR.MINOR.PATCH, with optional pre-release and/or build metadata, and may not contain negative numbers in any section.",
                        versionString,
                        normalPart[0]),
                    fe );
            }

            string  preReleaseVersion   = null,
                    buildMetadata       = null;

            if ( null != optionalParts )
            {
                var parts = optionalParts.Split('+');
                preReleaseVersion = parts[0];

                if (parts.Length > 1)
                {
                    if (parts.Length > 2)
                    {
                        throw new SemverVersionFormatException(
                            string.Format(
                                "Invalid semantic version: '{0}'. Cannot have more than one build metadata chunk in a semantic version.",
                                versionString) );
                    }

                    buildMetadata = parts[1];
                }
            }

            return new SemverVersion( major, minor, patch, preReleaseVersion, buildMetadata );
        }

        private int m_Major;
        private int m_Minor;
        private int m_Patch;

        //  N.B. Both of these are series of dot separated identifiers, but since we don't really particularly
        //  care about them at the moment, can defer comparisons to semver, and won't need to do anything beyond
        //  let the user specify a value, I've just implemented them as strings.
        private string m_PreReleaseVersion;
        private string m_BuildMetadata;

        private static bool IsValidOptionalFragment( string optional )
        {
            return string.IsNullOrEmpty( optional ) || optional.All( ch => char.IsLetterOrDigit( ch ) || ch == '.' || ch == '-' );
        }

        public SemverVersion( int major, int minor, int patch, string preReleaseVersion, string buildMetadata )
        {
            if ( major < 0 )
            {
                throw new ArgumentOutOfRangeException(
                    "major",
                    string.Format( "Invalid major version, {0}: may not be negative.", major ) );
            }

            if ( minor < 0 )
            {
                throw new ArgumentOutOfRangeException(
                    "minor",
                    string.Format( "Invalid minor version, {0}: may not be negative.", minor ) );
            }

            if ( patch < 0 )
            {
                throw new ArgumentOutOfRangeException(
                    "patch",
                    string.Format( "Invalid patch version, {0}: may not be negative.", patch ) );
            }

            if ( ! IsValidOptionalFragment( preReleaseVersion ) )
            {
                throw new ArgumentException(
                    string.Format( "Invalid pre-release version: '{0}'. Must be a dot separated sequence of identifiers containing only characters [0-9a-zA-Z].", preReleaseVersion ),
                    "preReleaseVersion" );
            }

            if ( ! IsValidOptionalFragment( buildMetadata ) )
            {
                throw new ArgumentException(
                    string.Format( "Invalid build metadata: '{0}'. Must be a dot separated sequence of identifiers containing only characters [0-9a-zA-Z].", preReleaseVersion ),
                    "buildMetadata" );
            }

            m_Major             = major;
            m_Minor             = minor;
            m_Patch             = patch;
            m_PreReleaseVersion = preReleaseVersion;
            m_BuildMetadata     = buildMetadata;
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
