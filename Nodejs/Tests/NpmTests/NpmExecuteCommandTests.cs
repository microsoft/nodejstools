/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.Npm.SPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests {
    [TestClass]
    class NpmExecuteCommandTests {
        // https://nodejstools.codeplex.com/workitem/1575
        [TestMethod, Priority(0), Timeout(180000)]
        public async Task TestNpmCommandProcessExitSucceeds() {
            var npmPath = NpmHelpers.GetPathToNpm();
            var redirector = new NpmCommand.NpmCommandRedirector(new NpmLsCommand(null, false));

            for (int j = 0; j < 200; j++) {
                await NpmHelpers.ExecuteNpmCommandAsync(
                    redirector,
                    npmPath,
                    null,
                    new[] {"config", "get", "registry"},
                    null);
            }
        }
    }
}
