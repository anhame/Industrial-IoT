<#
 .SYNOPSIS
    Sets environment variables containing version numbers

 .DESCRIPTION
    The script is a wrapper around nbgv tool and requires 
    dotnet installed.
#>

# Try install tool
& dotnet @("tool", "install", "-g", "nbgv") 2>&1 | Out-Null

# Call tool for cloud build
& nbgv  @("cloud", "-c", "-a")
if ($LastExitCode -ne 0 -and $LastExitCode -ne 5) {
    throw "Error: 'nbgv' failed with $($LastExitCode)."
}

return ((& nbgv  @("get-version", "-f", "json")) `
    | ConvertFrom-Json).CloudBuildAllVars