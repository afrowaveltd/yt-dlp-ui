# Android prototype — development towards 0.2.0

[Česky](cs.md) · [Documentation index](../Documentation/en.md) · [Versioning](../Versioning/en.md)

Our Android application has its **own touch-optimized Avalonia interface** and references the existing, platform-neutral `YtDlpUi.Core`. This is an experimental **0.2.0 development prototype**, not part of the published desktop [v0.1.0](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0).

## Implemented now

- Separate `src/YtDlpUi.Android/` Android host and `YtDlpUi.Android.slnx`; desktop builds remain independent of Android workloads.
- Single touch-friendly screen: paste a URL, explicit download action, status and recent queue entries.
- Receives links shared from other Android apps using the system **Share** menu (`ACTION_SEND` with `text/plain`); sharing a URL never auto-starts a download.
- Uses the Android **DownloadManager** for **direct HTTPS media-file URLs** ending in `.mp4`, `.webm`, `.mp3` and other recognized media extensions. Saves to standard **Downloads**; Android owns background progress, retries and notifications.
- Shares the tested link-parser implementation in `YtDlpUi.Core`.

**Not yet a yt-dlp Android engine:** a YouTube/Vimeo video-page link is not a direct media file. The prototype explains it cannot download such pages yet. Site extraction, format analysis, playlists, FFmpeg and custom quality presets need a real native Android-compatible backend. Do not advertise this prototype as a complete yt-dlp frontend.

## Permissions — minimum necessary

Only the normal `android.permission.INTERNET` is declared. The system DownloadManager handles saving into Downloads on Android 10+ without broad filesystem access. We do **not** request `MANAGE_EXTERNAL_STORAGE`, photo/media-library access, microphone, camera, contacts, location or root. Internet permission normally does not display a separate runtime prompt.

When genuinely necessary in future (such as a custom notification/foreground service), request new permissions when that feature is used, with a clear explanation.

## Build the test APK

Requires **.NET 10 SDK**, installed Android workload, Android SDK platform 36 and JDK 17:

~~~bash
dotnet workload install android
dotnet build YtDlpUi.Android.slnx -c Debug -p:AndroidPackageFormat=apk
~~~

The [Android CI workflow](https://github.com/afrowaveltd/yt-dlp-ui/actions/workflows/android-preview.yml) builds a **debug-signed test APK only**. It is neither a public Android release nor an app-store-ready signed build. Real-device install, sharing and download tests are still necessary. Android 10 (API 29) is the current minimum in project settings but version support has **not** been fully validated yet.

## Architecture and next steps

- **Core:** reusable link parsing, shared options and future download/queue contracts.
- **Android host:** activity lifecycle, share intents, permissions, Android Downloads.
- **Mobile UI:** distinct Avalonia views and view models; no copy of nine desktop tabs.
- **Download backend:** an independent, replaceable adapter; currently *direct-file-only*.

Before implementing genuine yt-dlp support, evaluate Android-compatible Python/yt-dlp packaging or a native library, native ABI constraints and third-party licensing (some candidate libraries are GPL). Never attempt to run executables downloaded into Android app-writable storage. Introduce scoped storage, system file picker, progress/cancellation, locale-driven mobile messages and foreground background-processing support only when required.

[Source](https://github.com/afrowaveltd/yt-dlp-ui) · [Desktop release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0)
