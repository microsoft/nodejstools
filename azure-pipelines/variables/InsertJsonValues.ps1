$vstsDropNames = & "$PSScriptRoot\VstsDropNames.ps1"

# $BasePath = "$PSScriptRoot\..\..\Nodejs\Setup\NodejsTools.vsmanproj"
$BasePath = "$PSScriptRoot\..\..\BuildOutput\Release\Setup" #this seemed to work?

if (Test-Path $BasePath) {
    $vsmanFiles = @()
    Get-ChildItem $BasePath *.vsman -Recurse -File |% {
        $version = (Get-Content $_.FullName | ConvertFrom-Json).info.buildVersion
        $fn = $_.Name
        $vsmanFiles += "$fn=https://vsdrop.corp.microsoft.com/file/v1/$vstsDropNames;$fn"
        # $vsmanFiles += "NodeJsTools.vsman=https://vsdrop.corp.microsoft.com/file/v1/$vstsDropNames;$fn"
                        #NodejsTools.vsman=https://vsdrop.corp.microsoft.com/file/v1/Products/DevDiv/microsoft/nodejstools/$(Build.SourceBranchName)/$(Build.BuildNumber);NodejsTools.vsman
    }

    [string]::join(',',$vsmanFiles)
}