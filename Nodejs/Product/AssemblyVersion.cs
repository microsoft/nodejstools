//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//
//    Apache 2.0 License
//
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
//    implied. See the License for the specific language governing
//    permissions and limitations under the License.
//
//*********************************************************//

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// If you get compiler errors CS0579, "Duplicate '<attributename>' attribute", check your
// Properties\AssemblyInfo.cs file and remove any lines duplicating the ones below.
// (See also AssemblyInfoCommon.cs in this same directory.)

#if !SUPPRESS_COMMON_ASSEMBLY_VERSION 
[assembly: AssemblyVersion(AssemblyVersionInfo.StableVersion)] 
#endif 
[assembly: AssemblyFileVersion(AssemblyVersionInfo.Version)] 

class AssemblyVersionInfo {

    // This version string (and the comment for StableVersion) should be
    // updated manually between major releases (e.g. from 1.0 to 2.0).
    // Servicing branches and minor releases should retain the value.
    public const string ReleaseVersion = "1.0";

    // This version string (and the comment for Version) should be updated
    // manually between minor releases (e.g. from 1.0 to 1.1).
    // Servicing branches and prereleases should retain the value.
    public const string FileVersion = "1.2";

    // This version should never change from "4100.00"; BuildRelease.ps1
    // will replace it with a generated value.
    public const string BuildNumber = "4100.00";
#if DEV11
    public const string VSMajorVersion = "11";
    const string VSVersionSuffix = "2012";
#elif DEV12
    public const string VSMajorVersion = "12";
    const string VSVersionSuffix = "2013";
#elif DEV14
    public const string VSMajorVersion = "14";
    const string VSVersionSuffix = "2015";
#elif DEV15
    public const string VSMajorVersion = "15";
    const string VSVersionSuffix = "15";
#else
#error Unrecognized VS Version.
#endif

    public const string VSVersion = VSMajorVersion + ".0";

    // Defaults to "1.0.0.(2012|2013|2015)"
    public const string StableVersion = ReleaseVersion + "." + VSVersionSuffix;

    // Defaults to "1.2.4100.00"
    public const string Version = FileVersion + "." + BuildNumber;
}