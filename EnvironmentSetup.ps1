<#
.Synopsis
    Sets up a Node.js Tools for Visual Studio development environment from this branch.
    
.Parameter vstarget
    [Optional] The VS version to build for. If omitted, checks for the Developer Tools environment variable.
    
    Valid values: "14.0", "15.0"
    
.Parameter vsroot
    [Optional] For VS 2017 only. Specifies the installation root directory of visual studio
    
    Example: "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise"
    
    Must be specified when building for "15.0", unless running from the Developer Tools Command Prompt.
    
.Parameter microbuild
    [Optional] Installs the microbuild packages and exits (other operations are skipped).

.Parameter skipTestHost
    [Optional] Does not attempt to install the TestHost package if missing.

.Parameter skipRestore
    [Optional] Does not attempt to restore the nuget packages. (Default is to restore when targeting "15.0").

.Example
    .\EnvironmentSetup.ps1
    
    Sets up NTVS development environment for the Visual Studio version matching the current Developer Tools prompt.

.Example
    .\EnvironmentSetup.ps1 -vstarget "14.0"
    
    Sets up NTVS development environment for specified Visual Studio versions installed on machine
#>

[CmdletBinding()]
param(
    [string] $vstarget,
    [string] $vsroot,
    [switch] $microbuild,
    [switch] $skipTestHost,
    [switch] $skipRestore
)

If ((-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(`
    [Security.Principal.WindowsBuiltInRole] "Administrator")) -and (-not $microbuild)) {
    Throw "You do not have Administrator rights to run this script. Please re-run as an Administrator."
}

$rootDir = $PSScriptRoot
Write-Output "Repository root: $($rootDir)"

$packagedir = if ($env:BUILD_BINARIESDIRECTORY) { "$env:BUILD_BINARIESDIRECTORY" } else { "$rootdir\packages" }

# Figure out which target to install for
if (-not $vstarget) {
    if(-not $env:VisualStudioVersion) {
        Throw "Please either specify the -vstarget option, or run from a Developer Tools Command Prompt"
    } else {
        # If getting the VS version from the environment, get the install path there too for consistency
        $vstarget = $env:VisualStudioVersion
    }
}

# Install microbuild packages
if ($microbuild -or ($vstarget -eq "15.0" -and -not $skipRestore)) {
    Write-Output ""
    Write-Output "Installing Nuget MicroBuild packages"

    & "$rootdir\Nodejs\.nuget\nuget.exe" restore "$rootdir\Nodejs\Setup\swix\packages.config" -PackagesDirectory "$packagedir"

    # If using the -microbuild switch, ONLY do the microbuild restore (this behavior is expected by the build servers).
    if($microbuild) {
        exit
    }
}

# Deduce the VS install path if not provided
if (-not $vsroot) {
    # Can use the registry for Dev14
    if ($vstarget -eq "14.0") {
        $vspath = Get-ItemProperty -Path "HKLM:\Software\Wow6432Node\Microsoft\VisualStudio\14.0" -EA 0
        if (-not $vspath) {
            $vspath = Get-ItemProperty -Path "HKLM:\Software\Microsoft\VisualStudio\14.0" -EA 0
        }
        if ($vspath) {
            $vsroot = $vspath.InstallDir
        }
    }

    # Else check the environment
    if (-not $vsroot) {
        $vsroot = $env:VSINSTALLDIR
    }
}

# Check the final value is valid
if (-not $vsroot -or -not (Test-Path -Path $vsroot)) { 
    Throw "Unable to determine the VS installation directory. Please specify -vsroot."
}

switch($vstarget) {
    "14.0" {
        $name = "VS 2015"
        $msbuildroot = "${env:ProgramFiles(x86)}\MSBuild\Microsoft\VisualStudio\v14.0"
    }
    "15.0" {
        $name = "VS 2017"
        $msbuildroot = "${vsroot}\MSBuild\Microsoft\VisualStudio\v15.0"
    }
    default {
        Throw "Invalid -vstarget of ${vstarget} specified"
    }
}

# Once to this point, the target version and install path are verified.

Write-Output "Setting up NTVS development environment for ${name}"
Write-Output "============================================================"

$from = "$rootDir\Nodejs\Product\Nodejs\Microsoft.NodejsTools.targets"
$to = "$msbuildroot\Node.js Tools\Microsoft.NodejsTools.targets"
Write-Output "Copying targets file: $($from) -> $($to)"
New-Item -Force $to > $null
Copy-Item -Force $from $to
Write-Output ""

# Need to use distinct install processes until the VSTestHost MSI is updated for 15.0.
function InstallDev14TestHost {
    $shouldInstallVSTestHost = $false
    if (-not $skipTestHost) {
        Write-Output ""
        $vsTestHostLocation = "$rootDir\Common\Tests\Prerequisites\VSTestHost.msi"
        Write-Output "    Installing VSTestHost from $($vsTestHostLocation)"

        # Check installed VSTestHost versions
        $gacDir = "${env:windir}\Microsoft.NET\assembly\GAC_MSIL"
        $currentVSTestHost = ls $gacDir -Recurse | ?{$_.Name -eq "Microsoft.VisualStudioTools.VSTestHost.$($version.number).dll"}
        
        $targetVSTestHostVersion = "14.0.1.0"
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

    if ($shouldInstallVSTestHost) {
        Start-Process msiexec -ArgumentList /i, $vsTestHostLocation, /lv, ${env:temp}\vstesthost.log, /quiet -Wait
        Write-Output "    Install completed"
    } else {
        Write-Output "    Skipping VSTestHost installation."
    }
}

function InstallDev15TestHost {
    Write-Output "Installing current VSTestHost build"
    $vstesthostdrop = "$rootDir\Common\Tests\Prerequisites\Dev15"
    Copy-Item -Force "$vstesthostdrop\Microsoft.VisualStudioTools.VSTestHost.15.0.dll" "${vsroot}\Common7\IDE\CommonExtensions\Platform"
    Copy-Item -Force "$vstesthostdrop\Microsoft.VisualStudioTools.VSTestHost.15.0.pkgdef" "${vsroot}\Common7\IDE\CommonExtensions\Platform"
    $regroot = mkdir -Force "HKLM:\Software\WOW6432Node\Microsoft\VisualStudio\15.0\EnterpriseTools\QualityTools\HostAdapters\VSTestHost";
    $regsupporttest = mkdir -Force "HKLM:$regroot\SupportedTestTypes";
    Set-ItemProperty HKLM:$regroot -Name "EditorType" -Value "Microsoft.VisualStudioTools.VSTestHost.TesterTestControl, Microsoft.VisualStudioTools.VSTestHost.15.0, Version=15.0.5.0, Culture=neutral, PublicKeyToken=B03F5F7F11D50A3A";
    Set-ItemProperty HKLM:$regroot -Name "Type" -Value "Microsoft.VisualStudioTools.VSTestHost.TesterTestAdapter, Microsoft.VisualStudioTools.VSTestHost.15.0, Version=15.0.5.0, Culture=neutral, PublicKeyToken=B03F5F7F11D50A3A";
    Set-ItemProperty HKLM:$regsupporttest -Name "{13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b}" -Value "Unit Test";
}

if (-not $skipTestHost) {
    switch ($vstarget) {
        "14.0" {InstallDev14TestHost}
        "15.0" {InstallDev15TestHost}
    }
}

Write-Output ""
Write-Output "Environment configuration succeeded."
Write-Output ""
