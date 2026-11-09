# Troubleshooting

[Česky](cs.md) · [Index](../Documentation/en.md) · [Installation](../Installation/en.md)

## yt-dlp status shows missing

Open **Tools** and run detection again. Confirm that `yt-dlp --version` (or `yt-dlp.exe --version` on Windows) succeeds. Use a custom executable path when it is not on PATH.

Windows setup has a selectable **Install yt-dlp** task; a failed optional download does not uninstall the GUI. On Linux use the distribution's package manager or an approved yt-dlp installation method. The standalone tool is separate from the GUI.

## FFmpeg is missing or conversion fails

Check `ffmpeg -version` and, for relevant operations, `ffprobe -version`. Windows setup installs machine-wide verified FFmpeg/FFprobe with UAC; the **Tools** tab can install a separate per-user pair; Linux installations normally use system packages. If necessary, set the FFmpeg path on **Tools**, then recheck detection. A download that only copies an existing stream might work without FFmpeg, while merging or converting may not.

## YouTube: WARNING about missing JavaScript runtime

This message comes from yt-dlp, not from the GUI or FFmpeg. YouTube may omit some formats without a supported JavaScript runtime. **Deno (version 2.3.0 or newer)** is the recommended runtime, enabled by default in yt-dlp.

- Windows: install Deno with the new **Install Deno** checkbox in our UAC Setup, or run `winget install --id DenoLand.Deno -e`. Restart the GUI afterward.
- From v0.1.4 on Windows and Linux: on the **Tools** page, see the Deno status, version and path, and use **Install Deno for Windows / Linux** to install a personal verified copy without administrator rights.
- Linux: GUI launches now detect Deno in `~/.deno/bin`, `~/.local/bin`, `~/bin`, `DENO_INSTALL/bin` and PATH even when the desktop environment does not inherit your shell profile. Alternatively, install Deno from its official packages and verify `~/.deno/bin/deno --version`.
- The GUI can automatically locate the Deno executable inside Windows ProgramData, user-local locations or PATH, even if the directory is not in PATH.
- An explicit `--js-runtimes node` (Node 22+) or `--js-runtimes deno:/full/path` is also available through advanced yt-dlp options.
- Official yt-dlp standalone executables include `yt-dlp-ejs`. Some pip/distribution installations require `yt-dlp[default]` or separate EJS solver scripts. Follow the [official yt-dlp EJS guide](https://github.com/yt-dlp/yt-dlp/wiki/EJS).
- Do not use `--no-warnings` to hide the message: it would conceal an unmet requirement.

## Windows installer couldn't download tools

- Confirm network/DNS access to the upstream publisher, GitHub releases for yt-dlp and the FFmpeg archive host.
- Look in `%ProgramData%\yt-dlp-ui\logs\dependency-install.log` for the error.
- Publisher checksum files must be available. A SHA-256 mismatch should stop dependency installation rather than run a corrupted executable.
- Rerun Windows **setup with UAC** to retry shared tool installation, or use the **Tools** tab for a per-user copy. Old per-user GUI installations may need removal to avoid duplicate shortcuts.

## A video cannot be analyzed or downloaded

- Verify the media URL in a browser; check that access is permitted.
- Try updating the installed yt-dlp; sites change independently of this GUI.
- If login is required, configure the necessary authentication/cookie option in the **Authentication** tab. Do not paste private cookies or secrets into public bug reports.
- Enable verbose output on **Advanced** and inspect the log. The application can expose the generated command, which helps reproduce the issue with yt-dlp directly.

## Linux starts with a display/graphics error

- A graphical desktop session and native UI libraries are required; a text-only server cannot display the Avalonia window.
- In virtual machines or unusual OpenGL environments, try `YTDLP_GUI_SOFTWARE_RENDER=1 ./publish/yt-dlp-gui`.
- `YTDLP_GUI_TRACE=1` enables additional Avalonia diagnostics in the terminal.
- If a single-file build cannot extract its native dependencies, set a writable `DOTNET_BUNDLE_EXTRACT_BASE_DIR` or use the default folder-based publish.

## Missing or unrecognized language

- Place the dictionary in the correct `Locales` folder, for example `pt-BR.json`. Open the header 🌐 dropdown again to rescan.
- Verify the file is a valid JSON object with string keys and string values. Invalid JSON is ignored.
- Make sure the language code is recognizable by .NET cultures. If only `fr.json` exists, `fr-CA` may fall back to `fr`.
- A translated dictionary only affects already-localized controls; untranslated advanced tabs may still show Czech.

## Settings seem to reset

The settings path is platform-specific; see [Configuration](../Configuration/en.md). If the file cannot be read, the app uses defaults. Avoid editing while the app is open because autosave may overwrite your changes.

## Reporting a problem

Use the [GitHub Issues page](https://github.com/afrowaveltd/yt-dlp-ui/issues). Include OS/architecture, build type, steps to reproduce, versions of yt-dlp/FFmpeg/Deno, expected and actual behavior, and a **sanitized** log. Never publish cookies, access tokens, passwords or private media links.
