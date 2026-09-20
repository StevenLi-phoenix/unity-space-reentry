# EMBER 1.1.2 delivery verification

Verified on 2026-09-20.

- [GitHub Release](https://github.com/StevenLi-phoenix/unity-space-reentry/releases/tag/v1.1.2): public, stable, all four platform archives and SHA256SUMS.txt attached.
- [Release workflow](https://github.com/StevenLi-phoenix/unity-space-reentry/actions/runs/35539102429): all checks, four builds, packaging and publication succeeded.
- [Final CI/CD workflow](https://github.com/StevenLi-phoenix/unity-space-reentry/actions/runs/35539674577): regression, WebGL build, validation and itch.io publish all succeeded after removing the problematic cross-runner Library cache.
- [Public game](https://stevenli-phoenix-work.itch.io/ember-reentry): anonymous page access and Run game verified. Upload 19326974, build 1998816, channel version 94e9dc589634744a0d1618aca2c7a9fa8322200a.

All four release archives were downloaded and matched SHA256SUMS.txt. Anonymous HEAD requests returned HTTP 200 for every archive and the checksum file. The WebGL release archive passed the compressed-bundle validator.

The final public CDN's four Unity resources exactly match the corresponding CI artifact after decoding compression. itch.io serves these resources decompressed. See [payload hashes](delivery-verification.json).

The downloaded Universal macOS player contains x86_64 and arm64, reports version 1.1.2 and ran on Apple Silicon through the landing QA fixture: flare, touchdown, rollout, zero-speed stop and the complete victory screen. Windows and Linux archives contain the executable, Unity runtime and player data; their OS runtimes were not playtested.

Chrome Work verified online startup, flight rendering, failure debrief, retry and pause; the final deployed build was loaded again after the CDN update. The public page has a 960x600 embed with fullscreen available, a plasma preview, dark space palette, gameplay description and four screenshots. Browser interaction ended with the final build on its title screen.

The accepted flight and landing gameplay was preserved during publication. PlayerSettings.bundleVersion remains the release version source. Main's subsequent changes repair CI caching and document delivery; the release tag remains immutable.
