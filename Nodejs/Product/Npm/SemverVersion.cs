using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.NodejsTools.Npm{
    /// <summary>
    /// Represents a semantic version as defined at http://semver.org/
    /// and used by the npm semantic versioner: https://npmjs.org/doc/misc/semver.html.
    /// </summary>
    public struct SemverVersion{
        public static readonly SemverVersion UnknownVersion = new SemverVersion(0, 0, 0);

        private static readonly Regex RegexSemver = new Regex(
            "^(?<major>[0-9]+)"
            + "\\.(?<minor>[0-9]+)"
            + "\\.(?<patch>[0-9]+)"
            + "(?:-(?<prerelease>[0-9A-Za-z-]+(\\.[0-9A-Za-z-]+)*))?"
            + "(?:\\+(?<buildmetadata>[0-9A-Za-z-]+(\\.[0-9A-Za-z-]+)*))?$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex RegexOptionalFragment = new Regex(
            "^[0-9A-Za-z-]+(\\.[0-9A-Za-z-]+)*$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static SemverVersion Parse(string versionString){
            var matches = RegexSemver.Matches(versionString);
            if (matches.Count != 1){
                throw new SemverVersionFormatException(
                    string.Format(
                        "Invalid semantic version: '{0}'. The version number must consist of three non-negative numeric parts of the form MAJOR.MINOR.PATCH, with optional pre-release and/or build metadata. The optional parts may only contain characters in the set [0-9A-Za-z-].",
                        versionString));
            }

            var match = matches[0];
            var preRelease = match.Groups["prerelease"];
            var buildMetadata = match.Groups["buildmetadata"];

            try{
                return new SemverVersion(
                    ulong.Parse(match.Groups["major"].Value),
                    ulong.Parse(match.Groups["minor"].Value),
                    ulong.Parse(match.Groups["patch"].Value),
                    preRelease.Success ? preRelease.Value : null,
                    buildMetadata.Success ? buildMetadata.Value : null);
            } catch (OverflowException oe){
                throw new SemverVersionFormatException(
                    string.Format(
                        "Invalid semantic version: '{0}'. One or more of the integer parts is large enough to overflow a 64-bit int.",
                        versionString),
                    oe);
            }
        }

        //  You may very well ask why these are now all ulongs. Well, you can thank the author of the so-called
        //  classy package for that. He saw fit to give his package a version number of 0.3.130506190513602.
        //  Wait! What?!? Does that mean there have been 130506190513601 previous patch releases of classy 0.3?
        //  No. No it doesn't. It means he can't read the semver spec.
        private ulong _major;
        private ulong _minor;
        private ulong _patch;

        //  N.B. Both of these are series of dot separated identifiers, but since we don't really particularly
        //  care about them at the moment, can defer comparisons to semver, and won't need to do anything beyond
        //  let the user specify a value, I've just implemented them as strings.
        private string _preReleaseVersion;
        private string _buildMetadata;

        private static bool IsValidOptionalFragment(string optional){
            return string.IsNullOrEmpty(optional) || RegexOptionalFragment.IsMatch(optional);
        }

        public SemverVersion(
            ulong major,
            ulong minor,
            ulong patch,
            string preReleaseVersion = null,
            string buildMetadata = null){
            if (! IsValidOptionalFragment(preReleaseVersion)){
                throw new ArgumentException(
                    string.Format(
                        "Invalid pre-release version: '{0}'. Must be a dot separated sequence of identifiers containing only characters [0-9A-Za-z-].",
                        preReleaseVersion),
                    "preReleaseVersion");
            }

            if (! IsValidOptionalFragment(buildMetadata)){
                throw new ArgumentException(
                    string.Format(
                        "Invalid build metadata: '{0}'. Must be a dot separated sequence of identifiers containing only characters [0-9A-Za-z-].",
                        preReleaseVersion),
                    "buildMetadata");
            }

            _major = major;
            _minor = minor;
            _patch = patch;
            _preReleaseVersion = preReleaseVersion;
            _buildMetadata = buildMetadata;
        }

        public ulong Major{
            get { return _major; }
        }

        public ulong Minor{
            get { return _minor; }
        }

        public ulong Patch{
            get { return _patch; }
        }

        public bool HasPreReleaseVersion{
            get { return ! string.IsNullOrEmpty(PreReleaseVersion); }
        }

        public string PreReleaseVersion{
            get { return _preReleaseVersion; }
        }

        public bool HasBuildMetadata{
            get { return ! string.IsNullOrEmpty(BuildMetadata); }
        }

        public string BuildMetadata{
            get { return _buildMetadata; }
        }

        public override string ToString(){
            var builder = new StringBuilder(string.Format("{0}.{1}.{2}", Major, Minor, Patch));

            if (HasPreReleaseVersion){
                builder.Append('-');
                builder.Append(PreReleaseVersion);
            }

            if (HasBuildMetadata){
                builder.Append('+');
                builder.Append(BuildMetadata);
            }

            return builder.ToString();
        }

        public override int GetHashCode(){
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj){
            if (!(obj is SemverVersion)){
                return false;
            }

            var other = (SemverVersion) obj;
            return Major == other.Major
                   && Minor == other.Minor
                   && Patch == other.Patch
                   && PreReleaseVersion == PreReleaseVersion
                   && BuildMetadata == BuildMetadata;
        }

        public static bool operator ==(SemverVersion v1, SemverVersion v2){
            return v1.Equals(v2);
        }

        public static bool operator !=(SemverVersion v1, SemverVersion v2){
            return ! (v1 == v2);
        }
    }
}