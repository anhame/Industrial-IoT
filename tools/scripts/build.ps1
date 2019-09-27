<#
 .SYNOPSIS
    Builds docker images from all definition files in the tree

 .DESCRIPTION
    The script traverses the build root to find all folders with an mcr.json
    file builds each one

 .PARAMETER BuildRoot
    The root folder to start traversing the repository from (Optional).

 .PARAMETER Debug
    Whether to build debug images.
#>

Param(
    [string] $BuildRoot = $null,
    [switch] $Debug
)

if ([string]::IsNullOrEmpty($BuildRoot)) {
    $BuildRoot = & ./getroot.ps1 -fileName "*.sln"
}

# Traverse from build root and find all mcr.json metadata files and build
Get-ChildItem $BuildRoot -Recurse `
    | Where-Object Name -eq "mcr.json" `
    | ForEach-Object {

    # Get root
    $dockerFolder = $_.DirectoryName.Replace($BuildRoot, "").Substring(1)
    $metadata = Get-Content -Raw -Path (join-path $_.DirectoryName "mcr.json") `
        | ConvertFrom-Json
    $scriptPath = (Join-Path $PSScriptRoot "docker-build.ps1")
    if ($Debug.IsPresent) {
        & $scriptPath -image $metadata.name -path $dockerFolder -debug
    }
    else {
        & $scriptPath -image $metadata.name -path $dockerFolder
    }
}