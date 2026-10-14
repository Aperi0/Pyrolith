<#
.SYNOPSIS
    Builds Pyrolith end to end.

.DESCRIPTION
    1. Publishes PyrolithLoader with Native AOT (needs the "Desktop development with C++" workload).
    2. Copies the result to Pyrolith\Resources\PyrolithLoader.exe, where Pyrolith embeds it.
    3. Publishes Pyrolith as a single framework-dependent executable.

    Output: artifacts\Pyrolith\Pyrolith.exe

.EXAMPLE
    ./build.ps1
.EXAMPLE
    ./build.ps1 -SkipLoader      # reuse the PyrolithLoader.exe already in Pyrolith\Resources
#>
param(
    [string] $Configuration = "Release",
    [ValidateSet("win-x64", "win-arm64")]
    [string] $Runtime = "win-x64",
    [switch] $SkipLoader
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$artifacts = Join-Path $root "artifacts"

function Invoke-Dotnet {
    & dotnet @args
    if ($LASTEXITCODE -ne 0) { throw "dotnet $($args[0]) failed with exit code $LASTEXITCODE" }
}

if (-not $SkipLoader) {
    Write-Host "==> Publishing PyrolithLoader (Native AOT, $Runtime)" -ForegroundColor Cyan
    $loaderOut = Join-Path $artifacts "loader"
    Invoke-Dotnet publish "$root/PyrolithLoader/PyrolithLoader.csproj" -c $Configuration -r $Runtime -o $loaderOut

    Copy-Item (Join-Path $loaderOut "PyrolithLoader.exe") (Join-Path $root "Pyrolith/Resources/PyrolithLoader.exe") -Force
    Write-Host "    Pyrolith/Resources/PyrolithLoader.exe updated"
}

Write-Host "==> Publishing Pyrolith ($Runtime)" -ForegroundColor Cyan
Invoke-Dotnet publish "$root/Pyrolith/Pyrolith.csproj" -c $Configuration -r $Runtime `
    --self-contained false -p:PublishSingleFile=true -o (Join-Path $artifacts "Pyrolith")

Write-Host "==> Done: $(Join-Path $artifacts 'Pyrolith\Pyrolith.exe')" -ForegroundColor Green
