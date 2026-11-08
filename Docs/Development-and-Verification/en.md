# Development and verification

[Česky](cs.md) · [Index](../Documentation/en.md) · [Release checklist](../Release-0.1.1/en.md)

## Build requirements

- .NET **10 SDK**, selected via `global.json` (feature-band roll-forward enabled).
- Avalonia UI and CommunityToolkit.Mvvm dependencies resolved by normal NuGet restore.
- On Linux, a graphical desktop is required for manually running the application.
- Packaging tools: `dpkg-deb` (DEB), `rpmbuild` (RPM), `tar` (portable Linux), PowerShell and Inno Setup 6 (Windows setup).
- The GUI is a .NET desktop application; **yt-dlp and FFmpeg are external tools**, not NuGet dependencies or part of the compiled GUI.

## Compile and smoke-test

From the repository root:

~~~bash
dotnet restore YtDlpUi.slnx
dotnet build YtDlpUi.slnx -c Release
dotnet run --project tests/YtDlpUi.SmokeTests/YtDlpUi.SmokeTests.csproj -c Release
dotnet run --project src/YtDlpUi/YtDlpUi.csproj
~~~

Builds target `net10.0`. Do not enable publish trimming without first converting/validating reflective Avalonia bindings.

## Project map

~~~text
src/YtDlpUi.Core/
  Models/                  settings, download options, enums, media information
  Services/                process runner, argument builder, metadata, downloads,
                           tool detection / installation, settings persistence
  Localization/            SimpleLocalizer, discovered JSON cultures
  Locales/                 bundled en.json and cs.json dictionaries
src/YtDlpUi/
  Views/Pages/             nine Avalonia tab pages
  ViewModels/              main window, queue items, toolchain
  Localization/            XAML translation binding extension
  Styles/, Assets/         theme, icons and application resources
tests/YtDlpUi.SmokeTests/  executable smoke checks
scripts/                   source build, DEB, RPM, tar, Windows setup
packaging/windows/         Inno Setup definition and optional dependency script
Docs/<Topic>/en.md, cs.md   bilingual guides and release notes
~~~

## Current automated checks

The smoke suite validates dictionaryless English fallback, live Czech translation, dynamic addition/removal/override of locale JSON, regional/parent culture handling, and FFmpeg command-line argument construction. It is a lightweight executable **smoke test**, not a complete UI/integration test suite.

[GitHub Actions](https://github.com/afrowaveltd/yt-dlp-ui/actions) runs builds and smoke checks on Ubuntu and Windows. It also:

- Builds and inspects `.deb`, `.rpm` and `.tar.gz` on Linux.
- Creates a Windows x64 portable build and an Inno Setup `setup.exe`.
- Uploads CI artifacts so they can be manually installed and tested.

Success in CI confirms compilation and package generation, **not** that every Windows wizard download, every yt-dlp website or every RPM target environment was tested. For manual validation, follow the [0.1.1 patch notes](../Release-0.1.1/en.md).

## Localization conventions

- Add new English keys to `src/YtDlpUi.Core/Locales/en.json`; use the English text as key/value.
- Translate into `cs.json` and any other available dictionary.
- Use `{loc:Tr Key='English text'}` in XAML for observable runtime updates.
- Do not hard-code a list of culture codes; `SimpleLocalizer` discovers dictionaries automatically.
- Validate new JSON, full culture fallback and live switching before merging.

## Documentation conventions

- English root `README.md` is the primary GitHub entry point.
- `Docs/README/en.md` mirrors it with adjusted relative links; `Docs/README/cs.md` is the Czech counterpart.
- Every topic has `Docs/<Topic>/en.md` **and** `cs.md`. Keep cross-links relative.
- If behavior is not verified, label it **planned**, **unverified** or **manual-test required**.
- Keep package versions and examples aligned with `Directory.Build.props`. A Git tag or release is a separate publication step.
- Update both languages whenever features, configuration, build scripts or tests change.
- Follow the [MAJOR.MINOR.PATCH release policy](../Versioning/en.md); all 0.x releases are GitHub pre-releases.

## Proposed manual test matrix

Test on at least Debian/Ubuntu desktop, one RPM distribution and Windows x64. Cover clean setup with both dependency checkboxes, disabled checkboxes, existing external tools, cancellation/network errors, app uninstall, downloading a sample you own, audio conversion, custom JSON hot-add, persistent settings, light/dark theme, keyboard shortcuts and URLs/paths containing spaces.

[Source code](https://github.com/afrowaveltd/yt-dlp-ui) · [Troubleshooting](../Troubleshooting/en.md)
