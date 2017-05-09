<#
.Synopsis
    Builds a release of Node.js Tools for Visual Studio from this branch.

.Description
    This script is used to build a set of installers for Node.js Tools for
    Visual Studio based on the code in this branch.
    
    The assembly and file versions are generated automatically and provided by
    modifying .\Build\AssemblyVersion.cs.
    
    The source is determined from the location of this script; to build another
    branch, use its Copy-Item of BuildRelease.ps1.

.Parameter outdir
    Directory to store the build.
    
    If `release` is specified, defaults to '\\pytools\release\<build number>'.

.Parameter vstarget
    [Optional] The VS version to build for. If omitted, builds for all versions
    that are installed.
    
    Valid values: "14.0", "15.0"

.Parameter vsroot
    [Optional] For VS15 only. Specifies the installation root directory of visual studio
    
    Example: "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise"
    
    Must be specified when building for VS15.

.Parameter name
    [Optional] A suffix to append to the name of the build.
    
    Typical values: "2.1 Alpha", "1.5 RC1", "My Feature Name", "2014-02-11 Dev Build"
    (Avoid: "RTM", "2.0 RTM")

.Parameter release
    When specified:
    * `outdir` will default to \\pytools\release\Nodejs\x.y if unspecified
    * A build number is generated and appended to `outdir`
     - The build number includes an index
    * Debug configurations are not built
    * Binaries and symbols are sent for indexing
    * Binaries and installers are sent for signing
    
    This switch requires the code signing object to be installed, and a smart
    card and reader must be available.
    
    See also: `mockrelease`.

.Parameter internal
    When specified:
    * `outdir` will default to \\pytools\release\Nodejs\Internal\$name if
      unspecified
    * A build number is generated and appended to `outdir`
     - The build number includes an index
    * Both Release and Debug configurations are built
    * No binaries are sent for indexing or signing
    
    See also: `release`, `mockrelease`

.Parameter mockrelease
    When specified:
    * A build number is generated and appended to `outdir`
     - The build number includes an index
    * Both Release and Debug configurations are built
    * Indexing requests are displayed in the output but are not sent
    * Signing requests are displayed in the output but are not sent
    
    Note that `outdir` is required and has no default.
    
    This switch requires the code signing object to be installed, but no smart
    card or reader is necessary.
    
    See also: `release`, `internal`

.Parameter scorch
    If specified, the enlistment is cleaned before and after building.

.Parameter skiptests
    If specified, test projects are not built.

.Parameter skipclean
    If specified, the output directory is not cleaned before building. This has
    no effect when used with `release`, since the output directory will not
    exist before the build.

.Parameter skipcopy
    If specified, does not copy the source files to the output directory.

.Parameter skipdebug
    If specified, does not build Debug configurations.

.Parameter dev
    If specified, generates a build name from the current date.

.Example
    .\BuildRelease.ps1 -release
    
    Creates signed installers for public release in \\pytools\release\<version>

.Example
    .\BuildRelease.ps1 -name "Beta" -release
    
    Create installers for a public beta in \\pytools\release\<version>

.Example
    .\BuildRelease.ps1 -name "My Feature" -internal
    
    Create installers for an internal feature test in 
    \\pytools\release\Internal\My Feature\<version>

#>
[CmdletBinding()]
param(
    [string] $outdir,
    [string] $vsTarget,
    [string] $vsroot,
    [string] $name,
    [switch] $release,
    [switch] $internal,
    [switch] $mockrelease,
    [switch] $scorch,
    [switch] $skiptests,
    [switch] $skipclean,
    [switch] $skipcopy,
    [switch] $skipdebug,
    [switch] $skipbuild,
    [switch] $dev,
    [switch] $copytests
)

$buildroot = (Split-Path -Parent $MyInvocation.MyCommand.Definition)
while ((Test-Path $buildroot) -and -not (Test-Path "$buildroot\build.root")) {
    $buildroot = (Split-Path -Parent $buildroot)
}
Write-Output "Build Root: $buildroot"


# This value is used to determine the most significant digit of the build number.
$base_year = 2016
# This value is used to automatically generate outdir for -release and -internal builds
$base_outdir = "\\pytools\Release\Nodejs"

# This file is parsed to find version information
$version_file = gi "$buildroot\Nodejs\Product\AssemblyVersion.cs"

$build_project = gi "$buildroot\Nodejs\dirs.proj"
$setup_swix_project = gi "$buildroot\Nodejs\Setup\setup-swix.proj"

# Project metadata
$project_name = "Node.js Tools for Visual Studio"
$project_url = "https://github.com/Microsoft/nodejstools"
$project_keywords = "NTVS; Visual Studio; Node.js"

# These people are able to approve code signing operations
$approvers = "smortaz", "dinov", "stevdo", "pminaev", "huvalo", "jinglou", "sitani", "crwilcox"

# These people are the contacts for the symbols uploaded to the symbol server
$symbol_contacts = "$env:username;dinov;smortaz;jinglou"

# This single person or DL is the contact for virus scan notifications
$vcs_contact = "ntvscore"

# These options are passed to all MSBuild processes
$global_msbuild_options = @("/v:q", "/m", "/nologo", "/flp:verbosity=detailed")

if ($skiptests) {
    $global_msbuild_options += "/p:IncludeTests=false"
} else {
    $global_msbuild_options += "/p:IncludeTests=true"
}

if ($release -or $mockrelease) {
    $global_msbuild_options += "/p:ReleaseBuild=true"
}

# Get the path to msbuild for a configuration
function msbuild-exe($target) {
    return "$($target.vsroot)\MSBuild\$($target.VSTarget)\Bin\msbuild.exe"
}

# This function is used to get options for each configuration
#
# $target contains the following members:
#   VSTarget            e.g. 14.0
#   VSName              e.g. VS 2013
#   config              Name of the build configuration
#   msi_version         X.Y.Z.W installer version
#   release_version     X.Y install version
#   assembly_version    X.Y.Z assembly version
#   logfile             Build log file
#   destdir             Root directory of all outputs
#   unsigned_bindir     Output directory for unsigned binaries
#   unsigned_msidir     Output directory for unsigned installers
#   symboldir           Output directory for debug symbols
#   final_msidir        The directory where the final installers end up
#
# The following members are available if $release or $mockrelease
#   signed_logfile      Rebuild log file (after signing)
#   signed_bindir       Output directory for signed binaries
#   signed_msidir       Output directory for signed installers
#   signed_unsigned_msidir  Output directory for unsigned installers containing signed binaries
#   signed_swix_logfile Log file for vsman project
function msbuild-options($target) {
    @(
        "/p:VSTarget=$($target.VSTarget)",
        "/p:VisualStudioVersion=$($target.VSTarget)",
        "/p:CopyOutputsToPath=$($target.destdir)",
        "/p:Configuration=$($target.config)",
        "/p:MsiVersion=$($target.msi_version)",
        "/p:ReleaseVersion=$($target.release_version)",
        "/p:DevEnvDir=$($target.vsroot)\Common7\IDE\\"
    )
}

# This function is invoked after each target is built.
function after-build($buildroot, $target) {
    Copy-Item -Force "$buildroot\Nodejs\Prerequisites\*.reg" $($target.destdir)

    $setup15 = mkdir "$($target.destdir)\Setup15" -Force 
    Copy-Item -Recurse -Force "$buildroot\BuildOutput\$($target.config)$($target.VSTarget)\Binaries\**\*.json" $setup15 
    Copy-Item -Recurse -Force "$buildroot\BuildOutput\$($target.config)$($target.VSTarget)\Setup\*.vsman" $setup15 

    if ($copytests) {
        Copy-Item -Recurse -Force "$buildroot\BuildOutput\$($target.config)$($target.VSTarget)\Tests" "$($target.destdir)\Tests"
        Copy-Item -Recurse -Force "$buildroot\Nodejs\Tests\TestData" "$($target.destdir)\Tests\TestData"
    }
}

# Fixes hashing information in the .vsman file
function fix-vs-manifest($vsmanFile, $bindir) {
    Write-Output "Patching Manifest $vsmanFile"
    $json = Get-Content $vsmanFile -raw | ConvertFrom-Json
    foreach ($pkg in $json.packages) {
        Write-Output "Patching package $($pkg.id)"

        # Assume only one payload
        $payload = $pkg.payloads[0]

        # Remove the _buildInfo block
        $payload = $payload | Select-Object * -ExcludeProperty _buildInfo
        $pkg.payloads[0] = $payload

        # Check that the file exists
        $vsixFilename = "$bindir\$($pkg.payloads[0].fileName)"
        if (!(Test-Path $vsixFilename)) {
            Write-Output "File $vsixFilename does not exist; continuing"
            continue
        }

        # Get its length and hash
        $length = (Get-Item $vsixFilename).Length
        $sha256 = (Get-Filehash $vsixFilename -Algorithm SHA256).Hash.ToLower()
        Write-Output "Hashed $vsixFilename (size $length) to $($sha256.Substring(0, 34))..."

        # Set properties
        $payload.sha256 = $sha256
        $payload.size = $length
    }
    $json | ConvertTo-Json -depth 100 | Out-File $vsmanFile
}

# This function is invoked after the entire build process but before scorching
function after-build-all($buildroot, $outdir) {
    if (-not $release) {
        Copy-Item -Force "$buildroot\Nodejs\Prerequisites\*.reg" $outdir
    }
 
    $logDrop = mkdir "$($outdir)\Logs" -Force 
    Copy-Item -Recurse -Force "$buildroot\Logs\*.*" $logDrop   
    
    $vsdrop = mkdir "$env:BUILD_STAGINGDIRECTORY\vsdrop" -Force
    Copy-Item -Force "$outdir\**.vsman" $vsdrop
    Copy-Item -Force "$outdir\**.json" $vsdrop
    Copy-Item -Force "$outdir\**.vsix" $vsdrop
}

# Manually clean up an output directory
function clean-outdir($outdir) {
    if ((Test-Path $outdir) -and (Get-ChildItem $outdir)) {
        Write-Output "Cleaning previous release in $outdir"
        del -Recurse -Force $outdir\* -EA 0
        while (Get-ChildItem $outdir) {
            Write-Output "Failed to clean release. Retrying in five seconds. (Press Ctrl+C to abort)"
            Sleep -Seconds 5
            del -Recurse -Force $outdir\* -EA 0
        }
    }
}

# Add product name mappings here
#   {0} will be replaced by the major version preceded by a space
#   {1} will be replaced by the build name preceded by a space
#   {2} will be replaced by the VS name preceded by a space
#   {3} will be replaced by the config ('Debug') marker preceded by a space
$installer_names = @{
    'NodejsToolsInstaller.msi'="NTVS{1}{2}{3}.msi";
    'Microsoft.NodejsTools.vsix' = 'Microsoft.NodejsTools.vsix';
    'Microsoft.NodejsTools.Profiling.vsix' = 'Microsoft.NodejsTools.Profiling.vsix';
    'Microsoft.NodejsTools.InteractiveWindow.vsix' = 'Microsoft.NodejsTools.InteractiveWindow.vsix';
    'Microsoft.VisualStudio.NodejsTools.Targets.vsix' = 'Microsoft.VisualStudio.NodejsTools.Targets.vsix';
    'NodejsTools.vsman' = 'NodejsTools.vsman';
    'Microsoft.VisualStudio.NodejsTools.NodejsTools.json' = 'Microsoft.VisualStudio.NodejsTools.NodejsTools.json';
    'Microsoft.VisualStudio.NodejsTools.Profiling.json' = 'Microsoft.VisualStudio.NodejsTools.Profiling.json';
    'Microsoft.VisualStudio.NodejsTools.InteractiveWindow.json' = 'Microsoft.VisualStudio.NodejsTools.InteractiveWindow.json';
    'Microsoft.VisualStudio.NodejsTools.Targets.json' = 'Microsoft.VisualStudio.NodejsTools.Targets.json';
}

$locales = ("cs", "de", "en", "es", "fr", "it", "ja", "ko", "pl", "pt-BR", "ru", "tr", "zh-Hans", "zh-Hant")

$localized_files = (
    "Microsoft.NodejsTools.InteractiveWindow.resources.dll",
    "Microsoft.NodejsTools.Npm.resources.dll",
    "Microsoft.NodejsTools.ProjectWizard.resources.dll",
    "Microsoft.NodejsTools.resources.dll"
)

# Add list of files requiring signing here
$managed_files = (
    "Microsoft.NodejsTools.NodeLogConverter.exe", 
    "Microsoft.NodejsTools.dll", 
    "Microsoft.NodejsTools.InteractiveWindow.dll",
    "Microsoft.NodejsTools.Profiling.dll",
    "Microsoft.NodejsTools.ProjectWizard.dll",
    "Microsoft.NodejsTools.WebRole.dll",
    "Microsoft.NodejsTools.Npm.dll",
    "Microsoft.NodejsTools.TestAdapter.dll",
    "Microsoft.NodejsTools.PressAnyKey.exe",
    "Microsoft.NodejsTools.Telemetry.15.0.dll"
)

$native_files = @()

$supported_vs_versions = (
    @{number="15.0"; name="VS 2017"; build_by_default=$true}
)

# #############################################################################
# #############################################################################
#
# The remainder of this file is product independent.
#
# #############################################################################
# #############################################################################

if (-not $outdir -and -not $release) {
    if (-not $outdir) {
        Throw "Invalid output directory '$outdir'"
    }
}

if ($dev) {
    if ($name) {
        Throw "Cannot specify both -dev and -name"
    }
    $name = "Dev {0:yyyy-MM-dd}" -f (Get-Date)
}

if ($name -match "[0-9.]*\s*RTM") {
    $result = $host.ui.PromptForChoice(
        "Build Name",
        "'RTM' is not a recommended build name. Final releases should have a blank name.",
        [System.Management.Automation.Host.ChoiceDescription[]](
            (New-Object System.Management.Automation.Host.ChoiceDescription "&Continue", "Continue anyway"),
            (New-Object System.Management.Automation.Host.ChoiceDescription "&Abort", "Abort the build"),
            (New-Object System.Management.Automation.Host.ChoiceDescription "C&lear", "Clear the build name and continue")
        ),
        2
    )
    if ($result -eq 1) {
        exit 0
    } elseif ($result -eq 2) {
        $name = ""
    }
}

$signedbuild = $release -or $mockrelease
if ($signedbuild) {
    $approvers = @($approvers | Where-Object {$_ -ne $env:USERNAME})

    Push-Location (Split-Path -Parent $MyInvocation.MyCommand.Definition)
    if ($mockrelease) {
        Set-Variable -Name DebugPreference -Value "Continue" -Scope "global"
        Import-Module -Force $buildroot\Build\BuildReleaseMockHelpers.psm1
    } else {
        Import-Module -Force $buildroot\Build\BuildReleaseHelpers.psm1
    }
    Pop-Location
}

$spacename = ""
if ($name) {
    $spacename = " $name"
    $global_msbuild_options += "/p:CustomBuildIdentifier=$name"
} elseif ($internal) {
    Throw "'-name [build name]' must be specified when using '-internal'"
}

$version_file_backed_up = 0
# Force use of a backup if there are pending changes to $version_file
$version_file_force_backup = 0
$has_tf_workspace = (Get-Command tf -errorAction SilentlyContinue) -and (-not (tf workspaces | Select-String -pattern "No workspace", "Unable to determine the workspace"))
if ($has_tf_workspace) {
    if (-not (tf status $version_file /format:detailed | Select-String "There are no pending changes.")) {
        Write-Output "$version_file has pending changes. Using backup instead of tf undo."
        $version_file_force_backup = 1
    }
}
$version_file_is_readonly = $version_file.Attributes -band [io.FileAttributes]::ReadOnly

$assembly_version = [regex]::Match((Get-Content $version_file), 'ReleaseVersion = "([0-9.]+)";').Groups[1].Value
$release_version = [regex]::Match((Get-Content $version_file), 'FileVersion = "([0-9.]+)";').Groups[1].Value

if ($internal) {
    $base_outdir = "$base_outdir\Internal\$name"
} elseif ($release) {
    $base_outdir = "$base_outdir\$release_version"
}

if (-not $outdir) {
    $outdir = $base_outdir
}

$serverBuildNumber = ${ENV:Build_BuildNumber}
if (-not $serverBuildNumber) {
    $buildindex = 0
} else {
    $buildindex = "$serverBuildNumber".Split(".")[1]
}

$buildnumber = '{0}{1:MMdd}.{2:D2}' -f (((Get-Date).Year - $base_year), (Get-Date), $buildindex)

if ($release -or $mockrelease -or $internal) {
    for (; $buildindex -lt 10000; $buildindex += 1) {
        $buildnumber = '{0}{1:MMdd}.{2:D2}' -f (((Get-Date).Year - $base_year), (Get-Date), $buildindex)
        if (-not (Test-Path $outdir\$buildnumber)) {
            break
        }
        $buildnumber = ''
    }
}
if (-not $buildnumber) {
    Throw "Cannot create version number. Try another output folder."
}
if ([int]::Parse([regex]::Match($buildnumber, '^[0-9]+').Value) -ge 65535) {
    Throw "Build number $buildnumber is invalid. Update `$base_year in this script.
(If the year is not yet $($base_year + 7) then something else has gone wrong.)"
}

$msi_version = "$release_version.$buildnumber"

if ($internal -or $release -or $mockrelease) {

    if (-not $serverBuildNumber) {
       $outdir = "$outdir\$buildnumber"
    } else {
       $outdir = "$outdir\$serverBuildNumber"
    }
}

Import-Module -Force $buildroot\Build\VisualStudioHelpers.psm1
$target_versions = get_target_vs_versions $vstarget $vsroot

if ($skipdebug) {
    $target_configs = ("Release")
} else {
    $target_configs = ("Debug", "Release")
}

Write-Output ""
Write-Output "============================================================"
Write-Output ""
if ($name) {
    Write-Output "Build Name: $name"
}
Write-Output "Output Dir: $outdir"
if ($mockrelease) {
    Write-Output "Auto-generated release outdir: $outdir"
}
Write-Output ""
Write-Output "Product version: $assembly_version.`$(VS version)"
Write-Output "MSI version: $msi_version"
Write-Output "Building for $([String]::Join(", ", ($target_versions | % { $_.name })))"
Write-Output "============================================================"
Write-Output ""

if (-not $skipclean) {
    clean-outdir $outdir
    clean-outdir $buildroot\BuildOutput
    
    if (-not (Test-Path $outdir)) {
        mkdir $outdir -EA 0 | Out-Null
        if (-not $?) {
            Throw "Could not make output directory: $outdir"
        }
    }
}

$logdir = mkdir "$buildroot\Logs" -Force

if ($scorch -and $has_tf_workspace) {
    tfpt scorch $buildroot /noprompt
}

$failed_logs = @()

Push-Location $buildroot
try {
    $successful = $false
    if ((-not $version_file_force_backup) -and $has_tf_workspace) {
        tf edit $version_file | Out-Null
    }
    if ($version_file_force_backup -or -not $?) {
        # running outside of MS
        Copy-Item -Force $version_file "$($version_file).bak"
        $version_file_backed_up = 1
    }
    Set-ItemProperty $version_file -Name IsReadOnly -Value $false
    (Get-Content $version_file) | %{ $_ -replace ' = "4100.00"', (' = "' + $buildnumber + '"') } | Set-Content $version_file

    foreach ($config in $target_configs) {
        # See the description near the msbuild_config function
        $target_info = @($target_versions | %{ 
            $i = @{
                VSTarget=$($_.number);
                VSName=$($_.name);
                destdir=mkdir "$outdir\$($_.name)\$config" -Force;
                logfile="$logdir\BuildRelease.$config.$($_.number).log";
                config=$config;
                msi_version=$msi_version;
                release_version=$release_version;
                vsroot=$($_.vsroot)
            }
            $i.unsigned_bindir = mkdir "$($i.destdir)\UnsignedBinaries" -Force
            $i.unsigned_msidir = mkdir "$($i.destdir)\UnsignedMsi" -Force
            $i.symboldir = mkdir "$($i.destdir)\Symbols" -Force
            if ($signedBuild) {
                $i.signed_bindir = mkdir "$($i.destdir)\SignedBinaries" -Force
                $i.signed_unsigned_msidir = mkdir "$($i.destdir)\SignedBinariesUnsignedMsi" -Force
                $i.signed_msidir = mkdir "$($i.destdir)\SignedMsi" -Force
                $i.final_msidir = $i.signed_msidir
                $i.signed_logfile = "$logdir\BuildRelease_Signed.$config.$($_.number).log"
                $i.signed_swix_logfile = "$logdir\BuildRelease_Swix_Signed.$config.$($_.number).log"
            } else {
                $i.final_msidir = $i.unsigned_msidir
            }
            $i
        })

        foreach ($i in $target_info) {
            if (-not $skipbuild) {
                $target_msbuild_exe = msbuild-exe $i
                $target_msbuild_options = msbuild-options $i
                if (-not $skipclean) {
                    & $target_msbuild_exe /t:Clean $global_msbuild_options $target_msbuild_options $build_project
                }
                & $target_msbuild_exe $global_msbuild_options $target_msbuild_options /fl /flp:logfile=$($i.logfile) $build_project

                if (-not $?) {
                    Write-Error "Build failed: $($i.VSName) $config"
                    $failed_logs += $i.logfile
                    continue
                }
            }
            
            after-build $buildroot $i
        }
        
        ######################################################################
        ##  BEGIN SIGNING CODE
        ######################################################################
        if ($signedBuild) {
            $jobs = @()
            
            foreach ($i in $target_info) {
                if ($i.logfile -in $failed_logs) {
                    Write-Output "Skipping signing for $($i.VSName) because the build failed"
                    continue
                }
                Write-Output "Submitting signing jobs for $($i.VSName)"

                $jobs += begin_sign_files `
                    @($managed_files | %{@{path="$($i.unsigned_bindir)\$_"; name=$_}} | ?{Test-Path $_.path}) `
                    $i.signed_bindir $approvers `
                    $project_name $project_url "$project_name $($i.VSName) - managed code" $project_keywords `
                    "authenticode;strongname" `
                    -delaysigned

                $jobs += begin_sign_files `
                    @($native_files | %{@{path="$($i.unsigned_bindir)\$_"; name=$project_name}} | ?{Test-Path $_.path}) `
                    $i.signed_bindir $approvers `
                    $project_name $project_url "$project_name $($i.VSName) - native code" $project_keywords `
                    "authenticode" 


                foreach ($loc in $locales) {
                    $jobs += begin_sign_files `
                            @($localized_files | %{@{path="$($i.unsigned_bindir)\$loc\$_"; name=$_}} | ?{Test-Path $_.path}) `
                            "$($i.signed_bindir)\$loc" $approvers `
                            $project_name $project_url "$project_name $($i.VSName) - managed code - $loc" $project_keywords `
                            "authenticode;strongname" `
                            -delaysigned
                }
            }
            
            end_sign_files $jobs
            
            foreach ($i in $target_info) {
                Write-Output "Begin symbol submission for $($i.VSName)"
                if ($i.logfile -in $failed_logs) {
                    Write-Output "Skipping symbol submission for $($i.VSName) because the build failed"
                    continue
                }
                submit_symbols "$project_name$spacename" "$buildnumber $($i.VSName) $config" "binaries" $i.signed_bindir $symbol_contacts
                submit_symbols "$project_name$spacename" "$buildnumber $($i.VSName) $config" "symbols" $i.symboldir $symbol_contacts

                Write-Output "End symbol submission for $($i.VSName)"                

                Write-Output "Begin Setup build for $($i.VSName)"
                $target_msbuild_exe = msbuild-exe $i
                $target_msbuild_options = msbuild-options $i

                & $target_msbuild_exe $global_msbuild_options $target_msbuild_options `
                    /fl /flp:logfile=$($i.signed_swix_logfile) `
                    /p:SignedBinariesPath=$($i.signed_bindir) `
                    /p:RezipVSIXFiles=true `
                    $setup_swix_project

                Write-Output "End Setup build for $($i.VSName)"
            }

            $jobs = @()
            
            foreach ($i in $target_info) {
                if ($i.logfile -in $failed_logs) {
                    continue
                }

                $msi_files = @((Get-ChildItem "$($i.signed_unsigned_msidir)\*.msi") | %{ @{
                    path="$_";
                    name="$project_name - $($_.Name)"
                }})

                if ($msi_files.Count -gt 0) {
                    Write-Output "Submitting MSI signing job for $($i.VSName)"

                    $jobs += begin_sign_files $msi_files $i.signed_msidir $approvers `
                        $project_name $project_url "$project_name $($i.VSName) - installer" $project_keywords `
                        "msi"
                }

                $vsix_files = @((Get-ChildItem "$($i.signed_unsigned_msidir)\*.vsix") | %{ @{
                    path="$_";
                    name="$project_name - $($_.Name)"
                }})

                if ($vsix_files.Count -gt 0) {
                    Write-Output "Submitting VSIX signing job for $($i.VSName)"

                    $jobs += begin_sign_files $vsix_files $i.signed_msidir $approvers `
                        $project_name $project_url "$project_name $($i.VSName) - VSIX" $project_keywords `
                        "vsix"
                }
            }

            end_sign_files $jobs
        }
        ######################################################################
        ##  END SIGNING CODE
        ######################################################################
        
        $fmt = @{}
        if ($release_version) { $fmt.release_version = " $release_version"} else { $fmt.release_version = "" }
        if ($name) { $fmt.name = " $name" } else { $fmt.name = "" }
        if ($config -match "debug") { $fmt.config = " Debug" } else { $fmt.config = "" }
        
        foreach ($i in $target_info) {
            if ($i.logfile -in $failed_logs) {
                continue
            }
            
            if ($i.VSName) {$fmt.VSName = " $($i.VSName)"} else {$fmt.VSName = ""}
            
            Get-ChildItem "$($i.final_msidir)\*.msi", "$($i.final_msidir)\*.vsix", "$($i.destdir)\Setup15\*.json", "$($i.destdir)\Setup15\*.vsman" | `
                ?{ $installer_names[$_.Name] } | `
                %{ @{
                    src=$_;
                    dest="$outdir\" + ($installer_names[$_.Name] -f
                        $fmt.release_version,
                        $fmt.name,
                        $fmt.VSName,
                        $fmt.config
                    ); 
                } } | `
                %{ Copy-Item $_.src $_.dest -Force -EA 0; $_ } | `
                %{ "Copied $($_.src) -> $($_.dest)" }

            $vsmanFile = "$($outdir)\NodejsTools.vsman"
            if (Test-Path $vsmanFile) {
                Write-Output "Patching VS Manifest $vsmanFile"
                fix-vs-manifest $vsmanFile $i.final_msidir
            } else {
                Write-Output "Skipping VS Manifest patching because $vsmanFile does not exist"
            }
        }
    }

    after-build-all $buildroot $outdir
    
    if ($scorch) {
        tfpt scorch $buildroot /noprompt
    }
    
    if (-not $skipcopy) {
        Write-Output "Copying source files"
        robocopy /s . $outdir\Sources /xd BuildOutput TestResults | Out-Null
    }
    
    if ($signedbuild) {
        start_virus_scan "$project_name$spacename" $vcs_contact $outdir
    }
    
    $successful = $true
} finally {
    try {
        if ($version_file_backed_up) {
            Move-Item "$version_file.bak" $version_file -Force
            if ($version_file_is_readonly) {
                Set-ItemProperty $version_file -Name IsReadOnly -Value $true
            }
            Write-Output "Restored $version_file"
        } elseif ((-not $version_file_force_backup) -and $has_tf_workspace) {
            tf undo /noprompt $version_file | Out-Null
        }
        
        if (-not (Get-Content $version_file) -match ' = "4100.00"') {
            Write-Error "Failed to undo $version_file"
        }
    } finally {
        Pop-Location
    }
}

if ($successful) {
    Write-Output ""
    Write-Output "Build complete"
    Write-Output ""
    Write-Output "Installers were output to:"
    Write-Output "    $outdir"
    if ($failed_logs.Count -gt 0) {
        Write-Output ""
        Write-Warning "Some configurations failed to build."
        Write-Output "Review these log files for details:"
        foreach ($name in $failed_logs) {
            Write-Output "    $name"
        }
        exit 1
    }
    exit 0
} else {
    exit 1
}
