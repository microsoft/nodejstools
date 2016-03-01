# Run the basic sets of tests using powershell
$vsversions = @("12.0", "14.0", "15.0")

# Set of tests to run
# AzurePublishing.Tests.UI are currently skipped.
$tests = @("JSAnalysisTests", "Nodejs.Tests.UI", "NodeTests","NpmTests", "ProfilerTests", "SharedProjectTests")

# Try to get the path to the vstest.console test runner
$vstest =  ''
foreach($vsversion in $vsversions) {
	$path = "C:\Program Files (x86)\Microsoft Visual Studio $vsversion\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
	if (Test-Path $path) {
		$vstest = $path
	}
}

if ($vstest -eq '') {
	echo "Could not find valid vstest.console"
	return
}

$testFiles = $tests | % { ".\BuildOutput\Release14.0\Tests\$_.dll" }

& "$vstest" @args $testFiles
