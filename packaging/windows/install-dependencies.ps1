<#
Installs external tools selected in the yt-dlp GUI setup wizard.
Uses publisher-provided SHA-256 checksums; requires HTTPS and elevated admin rights.
Installs to %ProgramData%\yt-dlp-ui\tools for all users; never changes PATH.
Logs to %ProgramData%\yt-dlp-ui\logs. Per-user settings are unaffected.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('yt-dlp','ffmpeg')]
    [string]$Tool
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
# Machine-level installation must never depend on the elevated admin's user profile.
$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Machine-wide dependency installation requires administrator privileges.'
}
$root = Join-Path $env:ProgramData 'yt-dlp-ui'
$tools = Join-Path $root 'tools'
$logs = Join-Path $root 'logs'
New-Item -ItemType Directory -Force -Path @($tools, $logs) | Out-Null
$logFile = Join-Path $logs 'dependency-install.log'

function Write-InstallLog([string]$Message) {
    Add-Content -LiteralPath $logFile -Value ("[{0}] {1}" -f (Get-Date -Format 's'), $Message) -Encoding UTF8
}

function Get-VerifiedDownload([string]$Uri, [string]$ChecksumUri, [string]$FileName) {
    $headers = @{'User-Agent'='yt-dlp-ui installer'}
    $hashText = [string](Invoke-WebRequest -Uri $ChecksumUri -UseBasicParsing -Headers $headers).Content
    $escapedName = [regex]::Escape($FileName)
    $match = [regex]::Match($hashText, "(?mi)^([0-9a-f]{64})\s+\*?$escapedName\s*$")
    if (-not $match.Success) {
        if ($FileName -ne 'ffmpeg-release-essentials.zip') {
            throw "Publisher checksum entry for $FileName was not found."
        }
        # Gyan's .sha256 may contain only a hexadecimal hash or a full filename.
        $match = [regex]::Match($hashText, '(?i)\b([0-9a-f]{64})\b')
        if (-not $match.Success) { throw 'Publisher FFmpeg checksum was not found.' }
    }

    $expected = $match.Groups[1].Value.ToUpperInvariant()
    $tempFile = Join-Path $tools ("download-" + [guid]::NewGuid().ToString('N') +
        [IO.Path]::GetExtension($FileName))
    try {
        Invoke-WebRequest -Uri $Uri -OutFile $tempFile -UseBasicParsing -Headers $headers
        $actual = (Get-FileHash -LiteralPath $tempFile -Algorithm SHA256).Hash.ToUpperInvariant()
        if ($actual -ne $expected) { throw "SHA-256 verification failed for $FileName." }
        Write-InstallLog "SHA-256 verified: $FileName"
        return $tempFile
    }
    catch {
        Remove-Item -LiteralPath $tempFile -Force -ErrorAction SilentlyContinue
        throw
    }
}

function Test-Available([string]$ExeName) {
    $command = Get-Command $ExeName -CommandType Application -ErrorAction SilentlyContinue
    if ($null -eq $command) { return $false }
    try {
        if ($ExeName -eq 'yt-dlp.exe') {
            & $command.Source --version | Out-Null
        } else {
            & $command.Source -version | Out-Null
        }
        return ($LASTEXITCODE -eq 0)
    } catch {
        return $false
    }
}

function Install-YtDlp {
    $path = Join-Path $tools 'yt-dlp.exe'
    if (Test-Path -LiteralPath $path) {
        Write-InstallLog 'Shared yt-dlp already present. Skipping download.'
        return
    }

    $asset = 'yt-dlp.exe'
    $release = 'https://github.com/yt-dlp/yt-dlp/releases/latest/download/'
    $download = Get-VerifiedDownload ($release + $asset) ($release + 'SHA2-256SUMS') $asset
    try {
        Move-Item -LiteralPath $download -Destination $path
        Write-InstallLog "Installed yt-dlp to $path"
    } finally {
        Remove-Item -LiteralPath $download -Force -ErrorAction SilentlyContinue
    }
}

function Install-Ffmpeg {
    $folder = Join-Path $tools 'ffmpeg'
    if ((Test-Path -LiteralPath (Join-Path $folder 'ffmpeg.exe')) -and
        (Test-Path -LiteralPath (Join-Path $folder 'ffprobe.exe'))) {
        Write-InstallLog 'Managed FFmpeg already present. Skipping download.'
        return
    }
    $base = 'https://www.gyan.dev/ffmpeg/builds/'
    $archive = Get-VerifiedDownload ($base + 'ffmpeg-release-essentials.zip') ($base + 'ffmpeg-release-essentials.zip.sha256') 'ffmpeg-release-essentials.zip'
    $stage = Join-Path $tools ('ffmpeg-stage-' + [guid]::NewGuid().ToString('N'))
    $backup = Join-Path $tools ('ffmpeg-backup-' + [guid]::NewGuid().ToString('N'))
    try {
        New-Item -ItemType Directory -Force -Path $stage | Out-Null
        Add-Type -AssemblyName System.IO.Compression
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = [IO.Compression.ZipFile]::OpenRead($archive)
        try {
            foreach ($exe in @('ffmpeg.exe','ffprobe.exe')) {
                $entries = @($zip.Entries | Where-Object {
                    $_.FullName.Replace('\','/') -match ('(?i)/bin/' + [regex]::Escape($exe) + '$')
                })
                if ($entries.Count -ne 1 -or $entries[0].Length -gt 350MB) {
                    throw "The archive contains an unexpected $exe entry."
                }
                # No arbitrary paths are extracted, preventing ZIP directory traversal.
                [IO.Compression.ZipFileExtensions]::ExtractToFile($entries[0], (Join-Path $stage $exe))
            }
        } finally {
            $zip.Dispose()
        }

        if (Test-Path -LiteralPath $folder) {
            Move-Item -LiteralPath $folder -Destination $backup
        }
        try {
            Move-Item -LiteralPath $stage -Destination $folder
            if (Test-Path -LiteralPath $backup) {
                Remove-Item -LiteralPath $backup -Recurse -Force
            }
        } catch {
            if (Test-Path -LiteralPath $backup) {
                Move-Item -LiteralPath $backup -Destination $folder
            }
            throw
        }
        Write-InstallLog "Installed FFmpeg and ffprobe to $folder"
    } finally {
        Remove-Item -LiteralPath $archive -Force -ErrorAction SilentlyContinue
        Remove-Item -LiteralPath $stage -Recurse -Force -ErrorAction SilentlyContinue
    }
}

try {
    Write-InstallLog "Starting optional installation: $Tool"
    switch ($Tool) {
        'yt-dlp' { Install-YtDlp }
        'ffmpeg' { Install-Ffmpeg }
    }
    Write-InstallLog "Completed optional installation: $Tool"
    exit 0
} catch {
    Write-InstallLog ("FAILED to install {0}: {1}" -f $Tool, $_.Exception.Message)
    exit 1
}
