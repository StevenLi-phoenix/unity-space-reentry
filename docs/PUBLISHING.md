# Publishing EMBER

Repository: https://github.com/StevenLi-phoenix/unity-space-reentry

Browser game: https://stevenli-phoenix-work.itch.io/ember-reentry

## Continuous delivery

Push `main` to run deterministic C# regressions and Node package checks. GitHub-hosted GameCI builds Unity 6000.6.0f1 WebGL, validates the compressed bundle, uploads a workflow artifact, and uses butler to update `stevenli-phoenix-work/ember-reentry:html5`. Publishing only runs for trusted main builds. Fork pull requests run secret-free regressions.

The itch.io project must remain public, and the `html5` upload must stay selected as playable in the browser. These are page settings; uploading a build alone does not prove the public game is playable.

## Versioned releases

1. Change `bundleVersion` in `Reentry/ProjectSettings/ProjectSettings.asset` and commit it.
2. Run `node --test scripts/*.test.mjs`, the C# checks in `tests/FlightChecks.csproj`, and relevant native/browser QA.
3. Create and push a matching annotated version tag such as `v1.2.0`.
4. Wait for the Release workflow. It checks version parity and builds WebGL, Universal macOS, Windows x64 and Linux x64. Publication waits for all four packages.
5. Verify the release assets, `SHA256SUMS.txt`, and an actual downloaded player. Add player-facing release notes describing changes and verification limits.

An existing tag can be rebuilt with the Release workflow's manual input. Never move a published tag to different source. BuildReentry preserves the serialized project version. `Desktop()` builds ARM64 for local QA; `MacOS()` builds the Universal release.

GitHub Releases contain complete players, not lone executables. macOS is an independent, unnotarized build. Windows/Linux builds are structurally validated in CI; OS runtime testing should be recorded separately.

## Secrets

Configure `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`, and `BUTLER_API_KEY` in this repository's Actions secrets. Pipe credential values from their existing private files directly to `gh secret set`; never put values in source, command output, documentation or chat. GitHub secrets cannot be read back from another repository.

No persistent self-hosted runner is used. No credentials or runtime network service are bundled into the game.
