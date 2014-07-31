Test Framework Extensibility
============================

NTVS can be extended to support additional test frameworks by implementing the discovery and execution logic using JavaScript.

In the following location:
`<VisualStudioFolder>\Common7\IDE\Extensions\Microsoft\Node.js Tools for Visual Studio\1.0\TestFrameworks`

You'll see folders for ExportRunner and Mocha.

Under each folder, a JavaScript file named after the folder contains 2 exported functions:

* `find_tests`
* `run_tests`

See the implementation of ExportRunner and Mocha for examples of `find_tests` and `run_tests` implementations.

Notes:

* The name of the folder must match the name of the .js file.
* Discovery of test frameworks happens at VS start.  If a framework is added while VS is running, VS must be restarted for detection to occur.
* Be sure to set the TestFramework property on your test file(s) to match the name of the subfolder under TestFrameworks.

If you implement support for other test frameworks and wish to contribute them, we gladly accept [pull requests](https://nodejstools.codeplex.com/SourceControl/latest).
