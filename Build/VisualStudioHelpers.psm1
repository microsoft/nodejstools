function get_target_vs_versions {
    param($vstarget)
 
    $supported_vs_versions = (
        @{number="15.0"; name="VS 15"; build_by_default=$true},    
        @{number="14.0"; name="VS 2015"; build_by_default=$true}
    )

    $target_versions = @()

    if ($vstarget) {
        $vstarget = "{0:00.0}" -f [float]::Parse($vstarget)
    }
    foreach ($target_vs in $supported_vs_versions) {
        if ((-not $vstarget -and $target_vs.build_by_default) -or ($target_vs.number -in $vstarget)) {
            $vspath = Get-ItemProperty -Path "HKLM:\Software\Wow6432Node\Microsoft\VisualStudio\$($target_vs.number)" -EA 0
            if (-not $vspath) {
                $vspath = Get-ItemProperty -Path "HKLM:\Software\Microsoft\VisualStudio\$($target_vs.number)" -EA 0
            }
            if ($vspath -and $vspath.InstallDir -and (Test-Path -Path $vspath.InstallDir)) {
                $msbuildroot = "${env:ProgramFiles(x86)}\MSBuild\Microsoft\VisualStudio\v$($vstarget)"
                $target_versions += @{
                    number=$target_vs.number;
                    name=$target_vs.name;
                    vsoot=$vspath.InstallDir;
                    msbuildroot=$msbuildroot
                }
            }
        }
    }
    
    if ($vstarget.Count -gt $target_versions.Count) {
        Write-Warning "Not all specified VS versions are available. Targeting only $($target_versions | %{$_.number})"
    }
    
    if (-not $target_versions) {
        Throw "No supported versions of Visual Studio installed."
    }
    
    return $target_versions
}

function get_target_vs15_version {
    param($vsroot)
    $msbuildroot="${vsroot}\MSBuild\Microsoft\VisualStudio\v15.0\Node.js Tools\Microsoft.NodejsTools.targets"
    return @{
        number="15.0";
        name="VS 15";
        vsoot=$root;
        msbuildroot=$msbuildroot
    }; 
}