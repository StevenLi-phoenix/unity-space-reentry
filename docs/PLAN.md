# EMBER / Return to Earth
A one-button Unity re-entry game with original Blender spacecraft and landing-site models.

1. Test-first deterministic flight simulation: heating, drag, range, terminal energy, distinct failures and a reproducible winning policy.
2. Blender source + FBX: lifting body, thermal tiles, cockpit, winglets, engines; runway and terminal structures. Inspect rendered geometry in game.
3. Unity: curved planet, clouds, stars, plasma ribbons and sparks, cinematic camera, adaptive generated audio, native HUD, title/tutorial, pause, debrief, saved best, three missions.
4. Run model and content checks, build native QA and WebGL, inspect actual gameplay and controls, fix visual/functional defects.
5. Publish browser build on itch.io, create source repository and repeatable CI, verify live playable build.

Art direction: midnight navy / porcelain ceramic / incandescent orange / cold cyan. Large clean typography, sparse aerospace instrumentation. Gameplay uses Space, mouse or touch hold/release. Pause and sound settings are secondary UI actions. No runtime AI dependency. Physics are deliberately time-compressed game approximations, not a flight simulator.
