[CmdletBinding()]
param(
    [string]$Configuration = 'Release'
)

Push-Location (Resolve-Path "$PSScriptRoot\..")
try {
    dotnet build KoreForge.Metrics.AspNet.sln --force -c $Configuration
} finally {
    Pop-Location
}