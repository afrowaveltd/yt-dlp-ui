param(
    [ValidateSet('win-x64', 'win-arm64')]
    [string]$Runtime = 'win-x64',
    [string]$Output = ''
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
if (-not $Output) { $Output = Join-Path $root "publish/$Runtime" }

dotnet publish (Join-Path $root 'src/YtDlpUi/YtDlpUi.csproj') `
    --configuration Release `
    --runtime $Runtime `
    --self-contained true `
    -p:PublishTrimmed=false `
    -p:PublishSingleFile=false `
    --output $Output
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed ($LASTEXITCODE)" }

Write-Host "Ready: $(Join-Path $Output 'yt-dlp-gui.exe')"
