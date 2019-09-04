<#
 .SYNOPSIS
    Sets environment variables containing version numbers

 .DESCRIPTION
    The script is a wrapper around gitversion tool and 
    requires dotnet installed.
#>

# Try install tool
& dotnet @("tool", "install", "-g", "GitVersion.Tool") 2>&1 | Out-Null

# Call tool
& dotnet-gitversion  @("/output", "buildserver")
if ($LastExitCode -ne 0) {
    throw "Error: 'dotnet-gitversion' failed with $($LastExitCode)."
}

return (& dotnet-gitversion  @("/output", "json")) | ConvertFrom-Json
