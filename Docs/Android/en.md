# Android prototype — development towards 0.2.0

[Česky](cs.md) · [Documentation index](../Documentation/en.md) · [Versioning](../Versioning/en.md)

Our Android application has its **own touch-optimized Avalonia interface** and references the existing, platform-neutral `YtDlpUi.Core`. The mobile host uses Avalonia 12.1.4 independently of the Avalonia 11 desktop app, as both reference only the Avalonia-free shared Core. This is an experimental **0.2.0 development prototype**, not part of the published desktop [v0.1.1](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.1).

## Implemented now

- Separate `src/YtDlpUi.Android/` Android host and `YtDlpUi.Android.slnx`; desktop builds remain independent of Android workloads.
- Single touch-friendly screen: paste a URL, explicit download action, status and recent queue entries.
- Receives links shared from other Android apps using the system **Share** menu (`ACTION_SEND` with `text/plain`); sharing a URL never auto-starts a download.
- Uses the Android **DownloadManager** for **direct HTTPS media-file URLs** ending in `.mp4`, `.webm`, `.mp3` and other recognized media extensions. Saves to standard **Downloads**; Android owns background progress, retries and notifications.
- Shares the tested link-parser implementation in `YtDlpUi.Core`.

- **Video-site URLs** are now routed to a separate in-process yt-dlp adapter. It uses the MIT-licensed [yt-dlp-android 2.0.2](https://github.com/ffmpegkit-maintained/yt-dlp-android) library, with embedded Python 3.13/Chaquopy, accessed via a .NET Android binding project. **No remote server or executable downloaded to app storage.**
- A small set of **FFmpeg-free quality presets** (best combined stream, up to 720p, up to 480p, original audio stream), a download percentage and a **Check local yt-dlp engine** diagnostic button.
- yt-dlp temporarily writes to app-private storage and uses Android **MediaStore** to export completed files into **Downloads/Afrowave**. No broad storage permission is requested.

**Testing limitation:** this build now includes a real local yt-dlp library, but its Python startup, website extraction and MediaStore export **have not yet been validated on a physical phone**. Some sites require cookies or browser impersonation; our free engine cannot guarantee every site. Playlist handling, FFmpeg merging/transcoding, cancellation and a persistent background service are still missing.

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
- **Download backend:** separate Android DownloadManager adapter for direct files, plus a pinned local yt-dlp Android Java binding for websites.

Before public release, verify native runtime packaging and source/licenses on a real phone. The pinned library is MIT, yt-dlp is Unlicense, Chaquopy is MIT and Python has the PSF license; see [upstream third-party notices](https://github.com/ffmpegkit-maintained/yt-dlp-android/blob/main/THIRD-PARTY-NOTICES.txt). The embedded runtime supports arm64-v8a and x86_64 only. No in-app executable updates. Later steps include real-device download tests, persistent progress/cancellation, mobile localization, FFmpeg license analysis and a foreground service when necessary.

[Source](https://github.com/afrowaveltd/yt-dlp-ui) · [Desktop release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.1)
