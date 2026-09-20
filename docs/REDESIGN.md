# Rejected first build → physical redesign
User acceptance: first build rejected (10/100). Reported unreadable loop, poor visuals, engines leading airflow, orbital speed at low altitude, enormous airport popping into existence.

Replacement acceptance criteria:
- Nose and tail anchors exported by Blender; imported nose points -Z and airflow +Z. Build-time asset test rejects reversed axes.
- SI planar spherical-Earth equations: height/downrange from velocity and flight-path angle, gravity plus lift/drag. Drag scales with air density and velocity squared. Exponential atmosphere; lower-altitude lift coefficients transition to subsonic wing behaviour.
- Thermal balance includes stagnation heating and radiative cooling. Dynamic pressure can destroy the vehicle, independently of temperature. A specific 8000 m/s at 3000 m test must immediately fail structurally.
- A permanently instantiated, dimension-tested Blender airport remains at its physical downrange location. Environment camera uses km, foreground uses m; their positions and orientations preserve projection equivalence. No altitude-triggered airport activation or scaling.
- Landing forecast and actual flown trajectory explain the consequence of changing angle of attack. Holding raises the nose and increases lift/drag; releasing lowers it. No scripted altitude countdown or target-distance calibration against an autopilot.
- Landing-system capture requires a 500m approach gate, speed <=180m/s, descent <=35m/s, and range error <=2km. Automatic final approach is explicitly identified to the player.
- Fixed 0.05s physical integration. Display is time-compressed: 8x hypersonic, 25x glide. The model remains unchanged by display speed.

Physical references: NASA Glenn drag/lift equations; planar re-entry equations (spherical Earth, nonrotating atmosphere). Coefficients and thermal mass describe a fictional vehicle and remain game tuning, not engineering certification.
Earth image: NASA/Goddard Space Flight Center Scientific Visualization Studio, Blue Marble; data courtesy Reto Stockli (NASA/GSFC) and NASA Earth Observatory. https://svs.gsfc.nasa.gov/2915

Presentation polish (2026-09-20): user accepted the physical core. Added subdivision-smoothed Blender fuselage/canopy, solid beveled winglets, identification markings, retracting landing gear, and reproducible 1024px panel/tile albedo and normal maps. Replaced nine line trails with a continuous turbulent plasma mesh, then softened density after inspecting native captures. Ordinary ablation sparks reduced from 420 to 12 per second at peak flux. Cloud horizon alpha now fades at grazing angles. Native and WebGL builds passed; native guided flight landed at 73.51m/s; browser restart and pause verified.
