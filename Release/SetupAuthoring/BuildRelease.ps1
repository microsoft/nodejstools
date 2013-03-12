param( [string] $outdir, [switch] $skiptests, [switch] $noclean, [switch] $uninstall, [string] $reinstall, [switch] $scorch, [string] $vsTarget, [switch] $nocopy, [switch] $skipdebug)

if (-not (get-command msbuild -EA 0))
{
    Write-Error "Visual Studio x86 build tools are required."
    exit 1
}

if (-not $outdir)
{
    Write-Error "Must provide valid output directory: '$outdir'"
    exit 1
}

if (-not $noclean)
{
    if (Test-Path $outdir)
    {
        "Cleaning previous release..."
        rmdir -Recurse -Force $outdir
        if (-not $?)
        {
            Write-Error "Could not clean output directory: $outdir"
            exit 1
        }
    }
    mkdir $outdir | Out-Null
    if (-not $?)
    {
        Write-Error "Could not make output directory: $outdir"
        exit 1
    }
}

# Add new products here:

$products = @{name="NodejsTools"; wxi=".\NodejsToolsInstaller\NodejsToolsInstallerVars.wxi"; msi="NodejsToolsInstaller.msi"}

$buildroot = (Get-Location).Path
while ((Test-Path $buildroot) -and -not (Test-Path ([System.IO.Path]::Combine($buildroot, "build.root")))) {
    $buildroot = [System.IO.Path]::Combine($buildroot, "..")
}
$buildroot = [System.IO.Path]::GetFullPath($buildroot)
"Build Root: $buildroot"
Push-Location $buildroot

$asmverfileBackedUp = 0
$asmverfile = Get-ChildItem Build\AssemblyVersion.cs

try {
    if ($uninstall)
    {
        $guidregexp = "<\?define InstallerGuid=(.*)\?>"
        foreach ($product in $products)
        {
            foreach ($line in ( Get-Content ([System.IO.Path]::Combine($buildroot, "Release\SetupAuthoring", $product.wxi)) ))
            {
                if ($line -match $guidregexp) { $guid = $matches[1] ; break }
            }
            "Got product guid for $($product.name): $guid"
            start -wait msiexec "/uninstall","{$guid}","/passive"
        }
    }
    
    $dev11InstallDir64 = Get-ItemProperty -path "HKLM:\Software\Wow6432Node\Microsoft\VisualStudio\11.0" -name InstallDir -EA 0
    $dev11InstallDir = Get-ItemProperty -path "HKLM:\Software\Microsoft\VisualStudio\11.0" -name InstallDir -EA 0
    
    $targetVersions = New-Object System.Collections.ArrayList($null)
    
    if ($dev11InstallDir64 -or $dev11InstallDir) {
        if (-not $vsTarget -or $vsTarget -eq "11.0") {
            echo "Will build for VS 2012"
            $targetVersions.Add(@{number="11.0"; name="VS 2012"}) | Out-Null
        }
    }
    
    $targetConfigs = ("Release", "Debug")
    if ($skipdebug) { $targetConfigs = ("Release") }
    
    foreach ($targetVs in $targetVersions) {
        $version = "0.5." + ([DateTime]::Now.Year - 2013).ToString() + [DateTime]::Now.Month.ToString('00') + [DateTime]::Now.Day.ToString('00') + ".0"
        
        $asmverfileBackedUp = 0
        tf edit $asmverfile
        if ($LASTEXITCODE -gt 0) {
            # running outside of MS
            attrib -r $asmverfile
            copy -force $asmverfile $($asmverfile.FullName).bak
            $asmverfileBackedUp = 1
        }
        (Get-Content $asmverfile) | %{ $_ -replace "0.7.4100.000", $version } | Set-Content $asmverfile
        
        Get-Content $asmverfile
        
        foreach ($config in $targetConfigs)
        {
            if (-not $skiptests)
            {
                msbuild /m /v:m /fl /flp:"Verbosity=n;LogFile=BuildRelease.$config.$($targetVs.number).tests.log" /p:Configuration=$config /p:WixVersion=$version /p:VSTarget=$($targetVs.number) /p:VisualStudioVersion=$($targetVs.number) Release\SetupAuthoring\dirs.proj
                if ($LASTEXITCODE -gt 0)
                {
                    Write-Error "Test build failed: $config"
                    exit 4
                }
            }
            
            msbuild /v:n /m /fl /flp:"Verbosity=n;LogFile=BuildRelease.$config.$($targetVs.number).log" /p:Configuration=$config /p:WixVersion=$version /p:VSTarget=$($targetVs.number) /p:VisualStudioVersion=$($targetVs.number) Release\SetupAuthoring\dirs.proj
            if ($LASTEXITCODE -gt 0) {
                Write-Error "Build failed: $config"
                exit 3
            }
            
            $bindir = "$buildroot\Binaries\$config$($targetVs.number)"
            $destdir = "$outdir\$($targetVs.name)\$config"
            
            if (-not (Test-Path $destdir)) { mkdir $destdir }
            copy -force $bindir\*.msi $destdir\
            copy -force Prerequisites\*.reg $destdir\
            
            if (-not (Test-Path $destdir\Symbols)) { mkdir $destdir\Symbols }
            copy -force -recurse $bindir\*.pdb $destdir\Symbols\
            
            if (-not (Test-Path $destdir\Binaries)) { mkdir $destdir\Binaries }
            copy -force -recurse $bindir\*.dll $destdir\Binaries\
            copy -force -recurse $bindir\*.exe $destdir\Binaries\
            copy -force -recurse $bindir\*.pkgdef $destdir\Binaries\
            
            if (-not (Test-Path $destdir\Binaries\ReplWindow)) { mkdir $destdir\Binaries\ReplWindow }
            copy -force -recurse Release\Product\Python\ReplWindow\obj\Dev$($targetVs.number)\$config\extension.vsixmanifest $destdir\Binaries\ReplWindow
        }
        
        if ($asmverfileBackedUp) {
            copy -force ($asmverfile.FullName + ".bak") $asmverfile
            attrib +r $asmverfile
            del ($asmverfile.FullName + ".bak")
            $asmverfileBackedUp = 0
        } else {
            tf undo /noprompt $asmverfile
        }
    }
    
    if ($scorch) { 
        "Scorching enlistment ...."
        tfpt scorch /noprompt
    }
    
    if (-not $nocopy) { robocopy /s . $outdir\Sources /xd TestResults Binaries Servicing | Out-Null }
} finally {
    if ($asmverfileBackedUp) {
        copy -force ($asmverfile.FullName + ".bak") $asmverfile
        attrib +r $asmverfile
        del ($asmverfile.FullName + ".bak")
    } else {
        tf undo /noprompt $asmverfile
    }
    
    Pop-Location
}

if ($reinstall -eq "Debug" -or $reinstall -eq "Release")
{
    foreach ($product in $products)
    {
        "Installing $($product.name) from $outdir\$reinstall\$($product.msi)"
        start -wait msiexec "/package","$outdir\$reinstall\$($product.msi)","/passive"
    }
}
