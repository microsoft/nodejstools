function get_target_vs_versions {
    param($vstarget)
        
    $supported_vs_versions = (
        @{number="15.0"; name="VS 15"; build_by_default=$true},    
        @{number="14.0"; name="VS 2015"; build_by_default=$true}
    )

    $target_versions = @()

    if ($vstarget) {
        $vstarget = $vstarget | %{ "{0:00.0}" -f [float]::Parse($_) }
    }
    foreach ($target_vs in $supported_vs_versions) {
            if ((-not $vstarget -and $target_vs.build_by_default) -or ($target_vs.number -in $vstarget)) {
            $vspath = Get-ItemProperty -Path "HKLM:\Software\Wow6432Node\Microsoft\VisualStudio\$($target_vs.number)" -EA 0
            if (-not $vspath) {
                $vspath = Get-ItemProperty -Path "HKLM:\Software\Microsoft\VisualStudio\$($target_vs.number)" -EA 0
            }
            if ($vspath -and $vspath.InstallDir -and (Test-Path -Path $vspath.InstallDir)) {
                $target_versions += $target_vs
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