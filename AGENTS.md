# EMBER / Return to Earth
Project: Reentry/, Unity 6000.6.0f1 + URP 17.6. Blender 5.2 source in art/Ember.blend; reproducible art/build_assets.py exports Resources/Ember.fbx. Blender is the Steam installation under ~/Library/Application Support/Steam/steamapps/common/Blender/Blender.app.

Plan before changes, back up existing files, test every feature. Do not read secrets or print credentials. Do not modify sibling game repositories.

FlightModel.cs is pure deterministic logic. FlightTests.Run validates three winning routes, constant-policy losses, thermal/energy/range outcomes, bounds and determinism before every build. Fixed gameplay step is .02 seconds. Flight units are intentionally compressed game approximations.

ReentryWorld handles visuals; ReentryHud uses static TMP SDF fonts to avoid dynamic glyph corruption. ReentryAudio generates its own PCM. ReentryGame handles input and menus. Runtime has no AI/network dependency.

Run bash scripts/build.sh Desktop or Web; logs /tmp/ember-build.log. Native --qa captures title, entry, approach and debrief to /tmp/ember-*.png without modifying best scores. Run node --test scripts/*.test.mjs for the WebGL validator. Verify actual screenshots and runtime logs as well as build success.

itch.io project: stevenli-phoenix-work/ember-reentry, numeric ID 5032527. Chrome Work native controls work; the other Chrome profile's browser extension timed out. Do not assume setValue persists rich text: use native paste and verify saved content. Verify actual playable live upload separately from creating the page.
