param (
    [string]$bump = "build" # or 'revision' or 'minor' or 'major'
 )

$me = $MyInvocation.MyCommand.Definition;

function Update-Version
{
  param([string] $path)

  $xml = [xml](Get-Content $path)
  $versionAttr = $xml.DocumentElement['Metadata']['Identity'].Attributes['Version']
  $oldVersion = $versionAttr.Value
  $parts = $oldVersion.Split('.');
  $incrementIndex = -1;
  switch($bump) {
    "build" {
      $incrementIndex = 3 
    }
    "revision" {
      $incrementIndex = 2
      $parts[3] = "0"
    }
    "minor" {
      $incrementIndex = 1
      $parts[3] = $parts[2] = "0"
    }
    "major" {
      $incrementIndex = 0
      $parts[3] = $parts[2] = $parts[1] = "0"
    }
    default {
      Write-Host 'Must specify "build", "revision", "minor", or "major"'
      exit 1
    }
  }
  $parts[$incrementIndex] = ([convert]::ToInt32($parts[$incrementIndex]) + 1).ToString()
  $newVersion = [string]::Join('.', $parts)
  Write-Host "Updated" ([IO.Path]::GetFileName([IO.Path]::GetDirectoryName($path))) "from" $oldVersion "to" $newVersion
  $versionAttr.Value = $newVersion
  $xml.Save($path)
}

function Update-Manifest
{
  param([string] $name)
  $path = [IO.Path]::Combine($me, '..\..\Nodejs\Product\', $name, 'source.extension.vsixmanifest')
  Update-Version $path
}

Update-Manifest 'InteractiveWindow'
Update-Manifest 'NodeJs'
