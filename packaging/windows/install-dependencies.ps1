<#
Installs external tools selected in the yt-dlp GUI setup wizard.
Uses publisher-provided SHA-256 checksums; requires HTTPS and elevated admin rights.
Installs to %ProgramData%\yt-dlp-ui\tools for all users; never changes PATH.
Logs to %ProgramData%\yt-dlp-ui\logs. Per-user settings are unaffected.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [ValidateSet('yt-dlp','ffmpeg','deno')]
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

function Get-PublisherSha256([string]$ChecksumText, [string]$FileName) {
    $digestPattern = '^[0-9a-fA-F]{64}$'
    $matchesFound = [System.Collections.Generic.List[string]]::new()
    $algorithm = @()
    $hash = @()
    $publishedPath = @()
    foreach ($line in ($ChecksumText -split "\r?\n")) {
        $value = $line.Trim()
        if ($value -match '^([0-9a-fA-F]{64})(?:\s+\*?(.+))?$') {
            $entryName = $Matches[2]
            if (-not $entryName -or $entryName -ceq $FileName) {
                $matchesFound.Add($Matches[1])
            }
            continue
        }
        # Official Windows Deno sidecars use PowerShell Get-FileHash output.
        if ($value -match '^(Algorithm|Hash|Path)\s*:\s*(.*?)\s*$') {
            switch ($Matches[1].ToLowerInvariant()) {
                'algorithm' { $algorithm += $Matches[2] }
                'hash' { $hash += $Matches[2] }
                'path' { $publishedPath += $Matches[2] }
            }
        }
    }
    if ($algorithm.Count -gt 0 -or $hash.Count -gt 0 -or $publishedPath.Count -gt 0) {
        if ($algorithm.Count -ne 1 -or $hash.Count -ne 1 -or $publishedPath.Count -ne 1 -or
            $algorithm[0] -ne 'SHA256' -or $hash[0] -cnotmatch $digestPattern -or
            [IO.Path]::GetFileName($publishedPath[0].Replace('/', '\')) -ine $FileName) {
            throw "Invalid or mismatched PowerShell publisher checksum for $FileName."
        }
        $matchesFound.Add($hash[0])
    }
    if ($matchesFound.Count -ne 1) {
        throw "Missing or ambiguous publisher SHA-256 checksum for $FileName."
    }
    return $matchesFound[0].ToUpperInvariant()
}

function Get-VerifiedDownload([string]$Uri, [string]$ChecksumUri, [string]$FileName) {
    $headers = @{'User-Agent'='yt-dlp-ui installer'}
    $hashText = [string](Invoke-WebRequest -Uri $ChecksumUri -UseBasicParsing -Headers $headers).Content
    $expected = Get-PublisherSha256 $hashText $FileName

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

function Test-LocalExecutable([string]$Path, [string]$VersionFlag) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $false }
    try {
        & $Path $VersionFlag | Out-Null
        return ($LASTEXITCODE -eq 0)
    } catch {
        return $false
    }
}

function Install-YtDlp {
    $path = Join-Path $tools 'yt-dlp.exe'
    if (Test-LocalExecutable $path '--version') {
        Write-InstallLog 'Working shared yt-dlp already present. Skipping download.'
        return
    }

    $asset = 'yt-dlp.exe'
    $release = 'https://github.com/yt-dlp/yt-dlp/releases/latest/download/'
    $download = Get-VerifiedDownload ($release + $asset) ($release + 'SHA2-256SUMS') $asset
    $backup = Join-Path $tools ('yt-dlp-backup-' + [guid]::NewGuid().ToString('N') + '.exe')
    $hadExisting = Test-Path -LiteralPath $path
    try {
        if ($hadExisting) { Move-Item -LiteralPath $path -Destination $backup -ErrorAction Stop }
        try {
            Move-Item -LiteralPath $download -Destination $path -ErrorAction Stop
            if (-not (Test-LocalExecutable $path '--version')) {
                throw 'New yt-dlp did not start after installation.'
            }
        } catch {
            Remove-Item -LiteralPath $path -Force -ErrorAction SilentlyContinue
            if ($hadExisting -and (Test-Path -LiteralPath $backup)) {
                Move-Item -LiteralPath $backup -Destination $path -ErrorAction Stop
            }
            throw
        }
        Write-InstallLog "Installed working yt-dlp to $path"
        # A backup may only be removed after the new executable has been verified.
        Remove-Item -LiteralPath $backup -Force -ErrorAction SilentlyContinue
    } finally {
        Remove-Item -LiteralPath $download -Force -ErrorAction SilentlyContinue
        # Preserve an old backup if rollback failed, for manual recovery.
    }
}

function Install-Ffmpeg {
    $folder = Join-Path $tools 'ffmpeg'
    if ((Test-LocalExecutable (Join-Path $folder 'ffmpeg.exe') '-version') -and
        (Test-LocalExecutable (Join-Path $folder 'ffprobe.exe') '-version')) {
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

        if (-not (Test-LocalExecutable (Join-Path $stage 'ffmpeg.exe') '-version') -or
            -not (Test-LocalExecutable (Join-Path $stage 'ffprobe.exe') '-version')) {
            throw 'Verified FFmpeg package could not execute after extraction.'
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

function Test-CompatibleDeno([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $false }
    try {
        # Read the entire output; piping to Select-Object -First 1 can close
        # the native process pipe before the executable exits successfully.
        $output = @(& $Path --version 2>&1)
        if ($LASTEXITCODE -ne 0 -or $output.Count -lt 1) { return $false }
        $first = [string]$output[0]
        if ($first -notmatch '^deno\s+(\d+\.\d+\.\d+)') {
            return $false
        }
        return ([version]$Matches[1] -ge [version]'2.3.0')
    } catch {
        return $false
    }
}

function Install-Deno {
    $path = Join-Path $tools 'deno.exe'
    if (Test-CompatibleDeno $path) {
        Write-InstallLog 'Compatible shared Deno already present. Skipping download.'
        return
    }

    # Deno is the recommended JS runtime for the yt-dlp YouTube EJS solver.
    # The official release ZIP contains deno.exe; verify its matching SHA256 sidecar.
    $asset = 'deno-x86_64-pc-windows-msvc.zip'
    $release = 'https://github.com/denoland/deno/releases/latest/download/'
    $archive = Get-VerifiedDownload ($release + $asset) ($release + $asset + '.sha256sum') $asset
    $stage = Join-Path $tools ('deno-stage-' + [guid]::NewGuid().ToString('N') + '.exe')
    $backup = Join-Path $tools ('deno-backup-' + [guid]::NewGuid().ToString('N') + '.exe')
    try {
        Add-Type -AssemblyName System.IO.Compression
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = [IO.Compression.ZipFile]::OpenRead($archive)
        try {
            $entries = @($zip.Entries | Where-Object { $_.FullName -eq 'deno.exe' })
            if ($entries.Count -ne 1 -or $entries[0].Length -gt 350MB) {
                throw 'Deno archive must contain one expected deno.exe entry.'
            }
            [IO.Compression.ZipFileExtensions]::ExtractToFile($entries[0], $stage)
        } finally {
            $zip.Dispose()
        }
        if (-not (Test-CompatibleDeno $stage)) {
            throw 'Verified Deno failed its version or execution check (minimum 2.3.0).'
        }

        $hadExisting = Test-Path -LiteralPath $path
        if ($hadExisting) { Move-Item -LiteralPath $path -Destination $backup -ErrorAction Stop }
        try {
            Move-Item -LiteralPath $stage -Destination $path -ErrorAction Stop
            if (-not (Test-CompatibleDeno $path)) {
                throw 'Installed Deno failed its version check.'
            }
        } catch {
            Remove-Item -LiteralPath $path -Force -ErrorAction SilentlyContinue
            if ($hadExisting -and (Test-Path -LiteralPath $backup)) {
                Move-Item -LiteralPath $backup -Destination $path -ErrorAction Stop
            }
            throw
        }
        Write-InstallLog "Installed compatible Deno to $path"
        Remove-Item -LiteralPath $backup -Force -ErrorAction SilentlyContinue
    } finally {
        Remove-Item -LiteralPath $archive -Force -ErrorAction SilentlyContinue
        Remove-Item -LiteralPath $stage -Force -ErrorAction SilentlyContinue
        # If rollback failed, retain its backup for manual recovery.
    }
}

try {
    Write-InstallLog "Starting optional installation: $Tool"
    switch ($Tool) {
        'yt-dlp' { Install-YtDlp }
        'ffmpeg' { Install-Ffmpeg }
        'deno' { Install-Deno }
    }
    Write-InstallLog "Completed optional installation: $Tool"
    exit 0
} catch {
    Write-InstallLog ("FAILED to install {0}: {1}" -f $Tool, $_.Exception.Message)
    exit 1
}
