<#
.Synopsis
    Runs the full NTVS test suite from this branch.
    
.Parameter vstarget
    [Optional] The VS version to test. If omitted, tests for latest version installed.
    
    Valid values: "14.0", "15.0"
    
.Example
    .\RunTests.ps1
    
    Runs NTVS tests for all versions of VS installed on the machine.

.Example
    .\RunTests.ps1 -vstarget "15.0"
    
    Runs NTVS tests for specified Visual Studio versions installed on machine
#>

[CmdletBinding()]
param(
    [string[]] $vstarget,
    [Parameter(Position=0)][string[]] $args
)

$rootDir = $PSScriptRoot
Write-Output "Repository root: $($rootDir)"
Write-Output ""

Import-Module -Force $rootDir\Build\VisualStudioHelpers.psm1
$target_versions = get_target_vs_versions $vstarget

Write-Output "Running NTVS tests for $([String]::Join(", ", ($target_versions | % { $_.name })))"
Write-Output "============================================================"

# Set of tests to run
# AzurePublishing.Tests.UI are currently skipped.
$tests = @("JSAnalysisTests", "Nodejs.Tests.UI", "NodeTests", "NpmTests", "SharedProjectTests")

# Try to get the path to the vstest.console test runner
if (-not $target_versions) {
    throw "Could not find any valid versions of Visual Studio installed"
}

foreach($vsVersion in $target_versions) {
	$vstest = "${env:ProgramFiles(x86)}\Microsoft Visual Studio $($vsVersion.number)\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
	if (Test-Path $vstest) {   
        $testFiles = $tests | % { ".\BuildOutput\Release$($vsVersion.number)\Tests\$_.dll" }
        & "$vstest" /settings:$("$rootDir\Build\default." + $vstarget + "Exp.testsettings") @args $testFiles
	} else {
        Write-Warning "Could not find valid vstest.console for VS $($vsVersion)"
    }
}