using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class GenericNpmCommand : NpmCommand {
        public GenericNpmCommand(
            string fullPathToRootPackageDirectory,
            string arguments,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true) : base(
            fullPathToRootPackageDirectory,
            pathToNpm,
            useFallbackIfNpmNotFound) {
            Arguments = arguments;
        }
    }
}
