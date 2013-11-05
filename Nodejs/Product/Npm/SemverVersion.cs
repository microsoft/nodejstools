using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{

    /// <summary>
    /// Represents a semantic version as defined at http://semver.org/
    /// and used by the npm semantic versioner: https://npmjs.org/doc/misc/semver.html.
    /// </summary>
    public struct SemverVersion
    {

        public static readonly SemverVersion UnknownVersion = new SemverVersion(0,0,0);

        private static readonly Regex RegexSemver = new Regex(
            "^(?<major>[0-9]+)"
            + "\\.(?<minor>[0-9]+)"
            + "\\.(?<patch>[0-9]+)"
            + "(?:-(?<prerelease>[0-9A-Za-z-]+(\\.[0-9A-Za-z-]+)*))?"
            + "(?:\\+(?<buildmetadata>[0-9A-Za-z-]+(\\.[0-9A-Za-z-]+)*))?$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline );

        private static readonly Regex RegexOptionalFragment = new Regex(
            "^[0-9A-Za-z-]+(\\.[0-9A-Za-z-]+)*$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline );

        public static SemverVersion Parse( string versionString )
        {
            var matches = RegexSemver.Matches(versionString);
            if ( matches.Count != 1 )
            {
                throw new SemverVersionFormatException(
                    string.Format("Invalid semantic version: '{0}'. The version number must consist of three non-negative numeric parts of the form MAJOR.MINOR.PATCH, with optional pre-release and/or build metadata. The optional parts may only contain characters in the set [0-9A-Za-z-].",
                    versionString ) );
            }

            var match           = matches[ 0 ];
            var preRelease      = match.Groups[ "prerelease" ];
            var buildMetadata   = match.Groups[ "buildmetadata" ];

            return new SemverVersion(
                int.Parse( match.Groups[ "major" ].Value ),
                int.Parse( match.Groups[ "minor" ].Value ),
                int.Parse( match.Groups[ "patch" ].Value ),
                preRelease.Success ? preRelease.Value : null,
                buildMetadata.Success ? buildMetadata.Value : null );
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
            return string.IsNullOrEmpty(optional) || RegexOptionalFragment.IsMatch(optional);
        }

        public SemverVersion(
            int major,
            int minor,
            int patch,
            string preReleaseVersion = null,
            string buildMetadata = null )
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
                    string.Format( "Invalid pre-release version: '{0}'. Must be a dot separated sequence of identifiers containing only characters [0-9A-Za-z-].", preReleaseVersion ),
                    "preReleaseVersion" );
            }

            if ( ! IsValidOptionalFragment( buildMetadata ) )
            {
                throw new ArgumentException(
                    string.Format( "Invalid build metadata: '{0}'. Must be a dot separated sequence of identifiers containing only characters [0-9A-Za-z-].", preReleaseVersion ),
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
            var builder = new StringBuilder(string.Format( "{0}.{1}.{2}", Major, Minor, Patch ));
            
            if (HasPreReleaseVersion)
            {
                builder.Append('-');
                builder.Append(PreReleaseVersion);
            }

            if (HasBuildMetadata)
            {
                builder.Append('+');
                builder.Append(BuildMetadata);
            }

            return builder.ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SemverVersion))
            {
                return false;
            }

            var other = (SemverVersion) obj;
            return Major == other.Major
                   && Minor == other.Minor
                   && Patch == other.Patch
                   && PreReleaseVersion == PreReleaseVersion
                   && BuildMetadata == BuildMetadata;
        }

        public static bool operator ==(SemverVersion v1, SemverVersion v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(SemverVersion v1, SemverVersion v2)
        {
            return ! (v1 == v2);
        }
    }
}
