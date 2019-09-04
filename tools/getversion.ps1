<#
 .SYNOPSIS
    Sets environment variables containing version numbers

 .DESCRIPTION
    The script is a wrapper around gitversioning tool and 
    requires dotnet installed.
#>

# Try install tool
& dotnet @("tool", "install", "-g", "nbgv") 2>&1 | Out-Null

# Call tool for cloud build
& nbgv  @("cloud")
if ($LastExitCode -ne 0 -and $LastExitCode -ne 5) {
    throw "Error: 'nbgv' failed with $($LastExitCode)."
}

$result = @{}
(& nbgv  @("get-version")) | ForEach-Object {
    $key, $value = $_.Split(':')
    $result[$key.Trim()] = $value.Trim()
}
return $result