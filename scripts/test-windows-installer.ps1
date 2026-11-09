# Static packaging regression checks. Real UAC/install tests still require a Windows VM.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$setup = Get-Content -LiteralPath (Join-Path $root 'packaging/windows/setup.iss') -Raw
$dependency = Join-Path $root 'packaging/windows/install-dependencies.ps1'
$script = Get-Content -LiteralPath $dependency -Raw
$locator = Get-Content -LiteralPath (Join-Path $root 'src/YtDlpUi.Core/Services/ToolLocator.cs') -Raw
$paths = Get-Content -LiteralPath (Join-Path $root 'src/YtDlpUi.Core/Services/UserPaths.cs') -Raw

function Assert-Contains([string]$Value, [string]$Pattern, [string]$Message) {
    if ($Value -notmatch $Pattern) { throw "INSTALLER TEST FAILED: $Message" }
}

Assert-Contains $setup '(?m)^PrivilegesRequired=admin\s*$' 'Installer must request UAC.'
Assert-Contains $setup '(?m)^DefaultDirName=\{autopf\}\\yt-dlp-gui\s*$' 'Application must install into Program Files.'
Assert-Contains $setup '(?m)^UsePreviousAppDir=no\s*$' 'Do not reuse an earlier per-user installation path.'
Assert-Contains $setup 'Flags: nowait postinstall skipifsilent runasoriginaluser' 'Launch desktop GUI with the original non-admin user.'
Assert-Contains $setup 'WizardIsTaskSelected\(''installYtDlp''\)' 'Preserve the optional yt-dlp task.'
Assert-Contains $setup 'WizardIsTaskSelected\(''installFfmpeg''\)' 'Preserve the optional FFmpeg task.'
Assert-Contains $setup 'WizardIsTaskSelected\(''installDeno''\)' 'Offer a selectable Deno JavaScript runtime.'
Assert-Contains $script '\$env:ProgramData' 'Machine tools must live in ProgramData, not the admin account profile.'
Assert-Contains $script 'IsInRole\(' 'Machine tool installs must require admin rights.'
Assert-Contains $script 'SHA-256 verification failed' 'Keep download integrity verification.'
Assert-Contains $script 'Test-LocalExecutable' 'Verify working binaries before skipping them.'
Assert-Contains $script 'Install-Deno' 'Install the Deno YouTube JavaScript runtime.'
Assert-Contains $script 'Test-CompatibleDeno' 'Require a supported Deno version.'
Assert-Contains $script '\$release \+ \$asset \+ ''\.sha256sum''' 'Verify Deno SHA-256 sidecar from publisher.'
Assert-Contains $paths 'MachineToolsDirectory' 'Core must expose machine tool directory.'
Assert-Contains $paths 'CommonApplicationData' 'Machine tool path must use the OS ProgramData location.'
Assert-Contains $locator 'MachineToolsDirectory' 'yt-dlp detection must find the machine copy.'
Assert-Contains $locator 'MachineFfmpegDirectory' 'FFmpeg detection must find the machine copy.'

# PowerShell must be parseable even before a real elevated install attempt.
$tokens = $null
$parseErrors = $null
$ast = [System.Management.Automation.Language.Parser]::ParseFile(
    $dependency, [ref]$tokens, [ref]$parseErrors)
if ($parseErrors.Count -ne 0) {
    throw ('INSTALLER TEST FAILED: invalid PowerShell: ' + ($parseErrors | Out-String))
}

# Test the real production parser without executing its elevated install entry point.
$checksumFunctions = @($ast.FindAll({
    param($node)
    $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $node.Name -in @('Get-PublisherSha256', 'Read-PublisherChecksum')
}, $true))
if ($checksumFunctions.Count -ne 2) { throw 'INSTALLER TEST FAILED: missing checksum functions.' }
foreach ($fn in $checksumFunctions) {
    Invoke-Expression $fn.Extent.Text
}

$digest = '1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef'
$expected = $digest.ToUpperInvariant()
$gnuSums = $digest + "  yt-dlp.exe`n" + ('a' * 64) + "  yt-dlp_arm64.exe`n"
if ((Get-PublisherSha256 $gnuSums 'yt-dlp.exe') -cne $expected) {
    throw 'INSTALLER TEST FAILED: GNU yt-dlp checksums must select the exact asset.'
}
$denoName = 'deno-x86_64-pc-windows-msvc.zip'
$windowsSums = "Algorithm : SHA256`r`nHash : $digest`r`nPath : C:\release\$denoName`r`n"
if ((Get-PublisherSha256 $windowsSums $denoName) -cne $expected) {
    throw 'INSTALLER TEST FAILED: PowerShell Deno checksums must be parsed.'
}
$checksumFile = Join-Path ([IO.Path]::GetTempPath()) ('afrowave-checksum-' + [guid]::NewGuid() + '.txt')
try {
    [IO.File]::WriteAllText($checksumFile, $gnuSums, [Text.UTF8Encoding]::new($false))
    if ((Get-PublisherSha256 (Read-PublisherChecksum $checksumFile) 'yt-dlp.exe') -cne $expected) {
        throw 'INSTALLER TEST FAILED: downloaded checksum file must be decoded as UTF-8.'
    }
}
finally {
    Remove-Item -LiteralPath $checksumFile -Force -ErrorAction SilentlyContinue
}
foreach ($case in @(
    @{ Text = ($digest + '  other.exe'); Asset = 'yt-dlp.exe' },
    @{ Text = ($digest + "`n" + $digest); Asset = 'yt-dlp.exe' },
    @{ Text = $windowsSums.Replace('SHA256', 'SHA512'); Asset = $denoName },
    @{ Text = $windowsSums.Replace($denoName, 'wrong.zip'); Asset = $denoName }
)) {
    $failed = $false
    try { $null = Get-PublisherSha256 $case.Text $case.Asset } catch { $failed = $true }
    if (-not $failed) { throw 'INSTALLER TEST FAILED: rejected checksum unexpectedly accepted.' }
}

Write-Host 'Windows machine-wide setup checks passed.'
