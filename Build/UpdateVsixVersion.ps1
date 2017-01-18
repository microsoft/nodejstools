$me = $MyInvocation.MyCommand.Definition;

function Update-Version
{
  param([string] $path)

  $xml = [xml](Get-Content $path)
  $versionAttr = $xml.DocumentElement['Metadata']['Identity'].Attributes['Version']
  $oldVersion = $versionAttr.Value
  $parts = $oldVersion.Split('.');
  $parts[3] = ([convert]::ToInt32($parts[3]) + 1).ToString()
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
Update-Manifest 'Profiling'
