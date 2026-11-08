param([switch]$FromPublish)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$publish = Join-Path $root 'publish/win-x64'
if (-not $FromPublish) {
    & (Join-Path $PSScriptRoot 'build-windows.ps1') -Runtime 'win-x64' -Output $publish
    if ($LASTEXITCODE -ne 0) { throw 'Windows publish failed.' }
}
if (-not (Test-Path (Join-Path $publish 'yt-dlp-gui.exe'))) {
    throw 'No published Windows app. First run scripts/build-windows.ps1.'
}
$compilerPaths = @(
    (Join-Path ([Environment]::GetFolderPath('ProgramFilesX86')) 'Inno Setup 6/ISCC.exe'),
    (Join-Path $env:ProgramFiles 'Inno Setup 6/ISCC.exe')
)
$compiler = $compilerPaths | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1
if (-not $compiler) {
    $command = Get-Command 'ISCC.exe' -ErrorAction SilentlyContinue
    if ($command) { $compiler = $command.Source }
}
if (-not $compiler) { throw 'Inno Setup 6 not found. Install from jrsoftware.org or Chocolatey.' }
[xml]$props = Get-Content (Join-Path $root 'Directory.Build.props')
$version = [string]$props.Project.PropertyGroup.Version
if (-not $version) { throw 'Could not determine the application version.' }
& $compiler "/DAppVersion=$version" (Join-Path $root 'packaging/windows/setup.iss')
if ($LASTEXITCODE -ne 0) { throw "Inno Setup failed: $LASTEXITCODE" }
$setup = Join-Path $root "artifacts/yt-dlp-gui-setup-$version-win-x64.exe"
if (-not (Test-Path $setup)) { throw "Installer was not created: $setup" }
Write-Host "Windows installer created: $setup"
