# Versioning and release policy

[Česky](cs.md) · [Documentation index](../Documentation/en.md) · [Release history](https://github.com/afrowaveltd/yt-dlp-ui/releases)

We use a simple **MAJOR.MINOR.PATCH** version format for yt-dlp GUI. The policy is inspired by [Semantic Versioning](https://semver.org/), with one explicit project convention: **every `0.x.y` release is published as a GitHub pre-release**, and **`1.0.0` marks our first stable release**.

## What the numbers mean

| Example | Meaning | GitHub designation |
| --- | --- | --- |
| `0.1.0` | First public preview; first feature generation | Pre-release |
| `0.1.1`, `0.1.2` | Bug fixes and maintenance without new user-facing functionality | Pre-release |
| `0.2.0` | Second preview generation, adding or significantly extending features | Pre-release |
| `0.2.1` | Fixes within the `0.2` generation | Pre-release |
| `0.3.0` | Third preview generation | Pre-release |
| `1.0.0` | First stable release, after the project passes its acceptance criteria | Stable |
| `1.0.1` | Backwards-compatible bug-fix release | Stable |
| `1.1.0` | New backwards-compatible features for stable users | Stable |
| `2.0.0` | A major / backwards-incompatible stable release | Stable |

The `0.2.0` designation means **second preview generation**, not necessarily the second published GitHub Release: `0.1.1`, `0.1.2` and similar maintenance releases may come first.

## Choosing the next version

1. **PATCH** — `0.1.0 → 0.1.1`: bug fixes, packaging repairs, performance/correctness fixes and documentation corrections that do not add user-visible functionality. When documentation-only changes need no new binary, update the repository documentation without publishing a new binary release.
2. **MINOR before 1.0** — `0.1.x → 0.2.0`: new capabilities, substantial workflow/UI additions, or deliberate behavior changes during preview development. Reset PATCH to zero.
3. **First stable** — `0.x.y → 1.0.0`: not automatic. Move to 1.0 only when installation, core downloads, tests and documentation meet agreed acceptance criteria.
4. **After 1.0** — increment PATCH for backwards-compatible fixes, MINOR for backwards-compatible features and MAJOR for breaking public behavior or compatibility changes. Reset smaller numbers to zero.

During `0.x`, compatibility can evolve more freely, but release notes must explicitly call out breaking configuration, data or workflow changes. A fix that introduces a new feature belongs in the next MINOR release rather than quietly changing a PATCH release.

## Release lifecycle

- The canonical version is defined in `Directory.Build.props`; update Windows installer metadata, distribution package names and documentation to match.
- Each published version receives a dedicated Git tag with the `v` prefix: `v0.1.0`, `v0.1.1`, `v0.2.0`, `v1.0.0` …
- **Published tags and release assets are immutable.** Fixes get a new version, not a modified previous download.
- Create a GitHub **pre-release** for every `0.x.y` version; publish `1.x.y` as a **stable release** unless specifically marked otherwise.
- Run CI for Linux and Windows, verify build artifacts/checksums, and publish release notes describing changes, known limitations and relevant upgrade steps.
- Maintain the English README plus its mirror in `Docs/README/en.md`, the Czech README in `Docs/README/cs.md`, and paired `Docs/<Topic>/en.md` / `cs.md` pages for changed behavior.
- Community bug reports belong in [GitHub Issues](https://github.com/afrowaveltd/yt-dlp-ui/issues).

## First public release

**[v0.1.0](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0)** is the published first public pre-release. **[v0.1.1](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.1)** is the first maintenance patch; **[v0.1.2](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.2)** fixes Windows setup; the next patch can be **v0.1.3**, while new features are reserved for **v0.2.0**. New platform support, such as a future Android client, is a feature and therefore belongs to a MINOR preview release (or another explicitly versioned product), not a patch update.

**Important:** GitHub's *pre-release* checkbox is separate from the version number. We intentionally use it for all `0.x.y` releases; this is a project policy, not a rule enforced by the SemVer specification.
