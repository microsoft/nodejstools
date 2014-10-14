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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.NodejsTools.Analysis.Analyzer {
    /// <summary>
    /// Provides an extra step to analyze the exported function as if it was
    /// called back with the grunt object.
    /// </summary>
    class GruntfileAnalysisUnit : AnalysisUnit {
        public GruntfileAnalysisUnit(Parsing.JsAst tree, ModuleEnvironmentRecord environment)
            : base(tree, environment) {
        }

        internal override void AnalyzeWorker(DDG ddg, System.Threading.CancellationToken cancel) {
            base.AnalyzeWorker(ddg, cancel);

            // perform the callback for the exported function so
            // we provide intellisense against the grunt parameter.
            var grunt = Analyzer.Modules.RequireModule(
                Ast,
                this,
                "grunt",
                DeclaringModuleEnvironment.Name
            );
            ProjectEntry.GetModule(this).Get(
                Ast,
                this,
                "exports",
                false
            ).Call(
                Ast,
                this,
                null,
                new[] { grunt }
            );
        }
    }
}
