param([string]$RuntimeDirectory = "$env:USERPROFILE\.dotnet-hackaton8")
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$localDotnet = Join-Path $RuntimeDirectory 'dotnet.exe'
if (-not (Test-Path -LiteralPath $localDotnet)) { throw 'Runtime local não encontrado. Instale ASP.NET Core 8 ou informe RuntimeDirectory.' }
Push-Location $projectRoot
try {
    & dotnet build HackatonApi/HackatonApi.csproj
    if ($LASTEXITCODE -ne 0) { throw 'Build falhou.' }
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    & $localDotnet 'HackatonApi/bin/Debug/net8.0/HackatonApi.dll' --urls http://localhost:5286 --contentRoot (Join-Path $projectRoot 'HackatonApi')
} finally { Pop-Location }
