<#
.Synopsis
    Sets up a Node.js Tools for Visual Studio development environment from this branch.
    
.Parameter vstarget
    [Optional] The VS version to build for. If omitted, builds for all versions
    that are installed.
    
    Valid values: "14.0", "15.0"
    
.Parameter vsroot
    [Optional] For VS15 only. Specifies the installation root directory of visual studio
    
    Example: "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise"
    
    Must be specified when building for VS15.
    
.Example
    .\EnvironmentSetup.ps1
    
    Sets up NTVS development environmentironment for all compatible Visual Studio versions installed on machine

.Example
    .\EnvironmentSetup.ps1 -vstarget "15.0"
    
    Sets up NTVS development environmentironment for specified Visual Studio versions installed on machine
#>

[CmdletBinding()]
param(
    [string] $vstarget,
    [string] $vsroot,
    [switch] $microbuild,
    [switch] $skipTestHost
)

If ((-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(`
    [Security.Principal.WindowsBuiltInRole] "Administrator")) -and (-not $microbuild)) {
    Throw "You do not have Administrator rights to run this script. Please re-run as an Administrator."
}

$rootDir = $PSScriptRoot
Write-Output "Repository root: $($rootDir)"


Import-Module -Force $rootDir\Build\VisualStudioHelpers.psm1
$target_versions = get_target_vs_versions $vstarget $vsroot

Write-Output "Setting up NTVS development environment for $([String]::Join(", ", ($target_versions | % { $_.name })))"
Write-Output "============================================================"
$packagedir = if ($env:BUILD_BINARIESDIRECTORY) { "$env:BUILD_BINARIESDIRECTORY" } else { "$rootdir\packages" }

# Install microbuild packages
if ($microbuild) {
    Write-Output ""
    Write-Output "Installing Nuget MicroBuild packages"

    & "$rootdir\Nodejs\.nuget\nuget.exe" restore "$rootdir\Nodejs\Setup\swix\packages.config" -PackagesDirectory "$packagedir"
    exit
}

# Disable strong name verification for the Node.js Tools binaries
$skipVerificationKey = If ( $ENV:PROCESSOR_ARCHITECTURE -eq "AMD64") {"EnableSkipVerification.reg" } Else {"EnableSkipVerification86.reg" }
$skipVerificationKey = Join-Path $rootDir $("Nodejs\Prerequisites\" + $skipVerificationKey)
Write-Output "Disabling strong name verification for Node.js Tools binaries"
Write-Output "    $($skipVerificationKey)"
regedit /s $($skipVerificationKey)

Write-Output ""
Write-Output "Copying required files"
foreach ($version in $target_versions) {    
    # Copy Microsoft.NodejsTools.targets file to relevant location
    $from = "$rootDir\Nodejs\Product\Nodejs\Microsoft.NodejsTools.targets"
    $to = "$($version.msbuildroot)\Node.js Tools\Microsoft.NodejsTools.targets"
    Write-Output $version
    Write-Output "    $($from) -> $($to)"
    New-Item -Force $to > $null
    Copy-Item -Force $from $to
}

# Install VSTestHost
$shouldInstallVSTestHost = $false
if (-not $skipTestHost) {
    Write-Output ""
    Write-Output "Installing VSTestHost automation"
    $vsTestHostLocation = "$rootDir\Common\Tests\Prerequisites\VSTestHost.msi"
    Write-Output "    $($vsTestHostLocation)"

    # Check installed VSTestHost versions
    $gacDir = "${env:windir}\Microsoft.NET\assembly\GAC_MSIL"
    foreach ($version in $target_versions) {
        $currentVSTestHost = ls $gacDir -Recurse | ?{$_.Name -eq "Microsoft.VisualStudioTools.VSTestHost.$($version.number).dll"}
        
        $targetVSTestHostVersion = "$($version.number).1.0"
        if (-not $currentVSTestHost) {
            Write-Output "VSTestHost not installed. Installing version $targetVSTestHostVersion"
            $shouldInstallVSTestHost = $true
            break
        }
        if ($currentVSTestHost.VersionInfo.FileVersion -ne $targetVSTestHostVersion) {
            Write-Warning "Incorrect VSTestHost version already installed. Overriding VSTestHost version $($currentVSTestHost.VersionInfo.FileVersion) with target VSTestHostVersion $targetVSTestHostVersion"
            $shouldInstallVSTestHost = $true
            break
        }
    }
}

if ($shouldInstallVSTestHost) {
    # TODO: This doesn't appear to install correctly on some machines.
    Start-Process msiexec -ArgumentList /i, $vsTestHostLocation, /lv, ${env:temp}\vstesthost.log, /quiet -Wait
    Write-Output "    Install completed"
} else {
    Write-Output "    Skipping VSTestHost installation (compatible version of VSTestHost already installed.)"
}

Write-Output ""
Write-Output "Environment configuration succeeded."
Write-Output ""