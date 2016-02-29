# Run the basic sets of tests using powershell
$vstest = "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"

$tests = @("NpmTests", "NodeTests", "ProfilerTests", "JSAnalysisTests", "MockVsTests", "Nodejs.Tests.UI")
$testFiles = $tests | % { ".\BuildOutput\Release14.0\Tests\$_.dll" }

& $vstest @args $testFiles
