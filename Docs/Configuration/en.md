# Configuration, files and privacy

[Česky](cs.md) · [Documentation index](../Documentation/en.md) · [Localization](../Localization/en.md)

Application preferences live in a JSON settings file, separate from the yt-dlp and FFmpeg executables. The source of truth is `AppSettings`, `SettingsStore` and `UserPaths` in `src/YtDlpUi.Core/`.

## Locations

| Item | Linux | Windows |
|---|---|---|
| Application settings | `~/.config/yt-dlp-ui/settings.json` or `$XDG_CONFIG_HOME/yt-dlp-ui/settings.json` | `%LOCALAPPDATA%\yt-dlp-ui\settings.json` |
| Local translation overrides | `~/.config/yt-dlp-ui/Locales/*.json` | `%LOCALAPPDATA%\yt-dlp-ui\Locales\*.json` |
| Application cache directory | `~/.cache/yt-dlp-ui/` or `$XDG_CACHE_HOME/yt-dlp-ui/` | `%LOCALAPPDATA%\yt-dlp-ui\` |
| App-managed yt-dlp | `~/.local/bin/yt-dlp` | `%LOCALAPPDATA%\yt-dlp-ui\tools\yt-dlp.exe` |
| App-managed FFmpeg | Typically distro-installed | `%LOCALAPPDATA%\yt-dlp-ui\tools\ffmpeg\ffmpeg.exe` and `ffprobe.exe` |
| Setup dependency log | Not applicable to DEB/RPM | `%LOCALAPPDATA%\yt-dlp-ui\logs\dependency-install.log` |

The **download destination** is user-selectable. On Linux the initial guess follows `user-dirs.dirs` when available, otherwise `~/Downloads` or the home directory. Do not assume every user downloads into the same location.

## Saved preferences

`AppSettings` includes custom paths for yt-dlp and FFmpeg, default download folder, clipboard monitoring, automatic analysis after paste, update checks for yt-dlp, remembering options, UI language, theme, concurrency and first-run/window state. Most settings persist automatically after a short delay or on normal shutdown.

Language setting `system` uses the current UI culture when a matching JSON exists, otherwise English; an explicit selection is stored as a language code.

The **Tools** tab lets you override detected tool paths if they are installed outside standard directories. The header shows their detection status. The application attempts to resolve runnable tools, not merely check for a filename.

## Security and privacy scope

- Supported password and 2FA fields are scrubbed from the **remembered download-options snapshot** before saving. This is not an encrypted secrets vault.
- Cookies, command-line arguments, external processes and logs may still contain sensitive information. Inspect what you share, especially command previews and verbose logs.
- The GUI contains no intentional advertisements or application telemetry. Network access is nevertheless required for video metadata/downloads and can be used for yt-dlp updates and optional dependency installation.
- DEB/RPM packaging uses package-manager recommendations, not an automatic post-install downloader. Windows setup uses explicit checkboxes and publisher SHA-256 checksums.
- Removing the Windows GUI does not automatically remove per-user externally installed tools; remove them manually if you no longer need them.

## Changing settings safely

1. Prefer the **Tools** tab for normal changes.
2. Close the app before editing `settings.json` by hand to avoid being overwritten by autosave.
3. Back up your file before experimenting with advanced options.
4. If settings are invalid or unreadable, the application falls back to defaults. It does not promise automatic recovery of the old file.
5. To add a new translation, put a culture-named JSON in `Locales` and reopen the language dropdown. See [Localization](../Localization/en.md).

[Installation](../Installation/en.md) · [Troubleshooting](../Troubleshooting/en.md)
