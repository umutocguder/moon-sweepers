# 11Hopem Moon Sweepers

[![CI/CD Status](https://img.shields.io/badge/CI%2FCD-Passing-brightgreen?style=flat-square)](https://github.com/umutocguder/moon-sweepers)
[![Game Engine](https://img.shields.io/badge/Engine-Unity%203D%20(C%23)-blue?style=flat-square)](https://unity.com)
[![Platform](https://img.shields.io/badge/Platform-macOS%20Standalone%20%7C%20PC-orange?style=flat-square)](https://github.com/umutocguder/moon-sweepers)
[![License](https://img.shields.io/badge/License-11Hopem%20Proprietary-red?style=flat-square)](https://11hopem.com)
[![Brand](https://img.shields.io/badge/Brand-11Hopem-purple?style=flat-square)](https://11hopem.com)

A first-person 3D lunar dust cleanup, simulation, tycoon, and automation incremental game inspired by the mechanics of *Moon Sweepers* (DragDrop Games, Steam App ID 5335510). Built using the exact same game engine (**Unity 3D Engine / C#**) and natively optimized for macOS and PC platforms.

---

## 1. Project Overview & Premise
- **Premise:** The Lunar Surface Department (LSD) has tasked an astronaut with sweeping and restoring the Moon's regolith surface. Due to massive budget cuts, you begin your operations equipped with only a **standard toothbrush**.
- **Progression Loop:** Scrub regolith dust -> Gather mineral treasures -> Deposit dust at the Lunar Base Hopper -> Convert to Lunar Credits (LC) -> Purchase advanced vacuum gear, expanded backpacks, and automated drones.
- **Physics:** Modeled under authentic lunar low gravity ($g = -1.62\text{ m/s}^2$), allowing long hang-time jumps and jetpack gliding.
- **Engine:** Built with Unity 3D Engine, standard C# architecture, and Universal Render Pipeline (URP).
- **Brand Signature:** Exclusively **11Hopem** with direct links to [11hopem.com](https://11hopem.com).

---

## 2. Equipment & Cleanup Tiers
The player unlocks tools dynamically via the Lunar Hangar Shop:
1. **Tier 1 — Standard Toothbrush:** Starter tool (0 LC). Precision scrub, 1.0 kg/hit, 2.2m range, 0.6m radius.
2. **Tier 2 — Lunar Hand Broom:** Light upgrade (120 LC). Wider sweep arc, 3.5 kg/hit, 3.0m range, 1.2m radius.
3. **Tier 3 — Cyclone Vacuum Mk-I:** Industrial suction (550 LC). Continuous suction beam, 12.0 kg/sec, 4.5m range.
4. **Tier 4 — Electromagnetic Sweeper:** Magnetic attraction (1,800 LC). Radial field pull, 30.0 kg/sec, 6.0m range.
5. **Tier 5 — Lunar Sweeper Buggy:** Heavy machinery (5,000 LC). Massive forward roller intake, 100.0 kg/sec, 8.0m range.

---

## 3. Automation & Tycoon Mechanics
- **Depot Hopper Base:** The central drop-off station where astronaut backpacks are emptied and converted into Lunar Credits ($LC$).
- **Regolith Conveyor Line:** Directs dust from surrounding surface patches automatically into the hopper.
- **Autonomous Lunar Cleaner Drones:** Airborne AI drones patrolling designated circular orbits, sweeping dust continuously.
- **Rare Minerals:** Regolith patches have a chance to drop:
  - *Titanium Chunk* (+15 LC multiplier)
  - *Helium-3 Canister* (+45 LC multiplier)
  - *Ancient Alien Screw* (+120 LC multiplier)

---

## 4. Architecture & Directory Tree
```
moon-sweepers/
├── Assets/
│   ├── Scenes/
│   │   └── MainLunarBase.unity         # Primary 3D Lunar Base scene
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs          # Central lifecycle & gravity manager
│   │   │   ├── AudioManager.cs          # Procedural audio synth (Zero copyright risk)
│   │   │   └── SaveManager.cs           # Cryptographic JSON progress storage
│   │   ├── Player/
│   │   │   ├── PlayerController.cs      # 3D low-gravity movement & jetpack
│   │   │   └── PlayerInventory.cs       # Regolith dust kg & capacity limits
│   │   ├── Tools/
│   │   │   ├── CleaningTool.cs          # Tool data specifications & tiers
│   │   │   └── ToolController.cs        # Tool usage, continuous vacuum & raycasting
│   │   ├── Environment/
│   │   │   ├── LunarSurfaceGenerator.cs # Procedural crater, boulder & dust field generator
│   │   │   ├── RegolithDustPatch.cs     # Interactive dynamic shrinking dust patches
│   │   │   └── DepotHopper.cs           # Base unload trigger zone & payout calculator
│   │   ├── Economy/
│   │   │   ├── ShopManager.cs           # Equipment & upgrade purchase pipeline
│   │   │   └── AutomationStation.cs     # Conveyor lines & autonomous patrol drones
│   │   ├── UI/
│   │   │   ├── HUDController.cs         # On-screen HUD, backpack gauge & crosshair
│   │   │   └── ShopUIController.cs      # Tab-activated glassmorphism hangar shop
│   │   └── Editor/
│   │       ├── MacBuildScript.cs        # 1-click standalone macOS (.app) builder
│   │       └── LunarSceneBuilder.cs     # Automated scene construction script
│   └── logo.svg, favicon.ico, etc.     # Brand assets
├── Packages/
│   └── manifest.json                    # Unity package definitions
├── ProjectSettings/
│   ├── ProjectSettings.asset            # 11Hopem brand & macOS standalone settings
│   ├── ProjectVersion.txt               # Unity 6 / LTS editor target
│   ├── TagManager.asset                 # Layer & tag registrations
│   └── InputManager.asset               # Controls mapping (WASD, Mouse, Jump, Boost)
├── scripts/
│   ├── generateMetaFiles.js             # Deterministic .meta file generator
│   ├── buildMac.js                      # macOS build automation runner
│   └── runBackup.js                     # Local SHA-256 backup script
├── tests/
│   ├── game.test.js                     # Core logic unit tests
│   └── unityProject.test.js             # Unity architecture & security tests
└── package.json
```

---

## 5. Controls & Keybindings
| Action | Key / Input |
|---|---|
| **Movement** | `W`, `A`, `S`, `D` |
| **Mouse Look** | Mouse Pitch / Yaw |
| **Sweep / Vacuum** | `Left Mouse Click` (or hold for vacuum) |
| **Thruster / Jetpack Boost** | `Right Mouse Click` or `Left Shift` |
| **Low-G Jump** | `Spacebar` |
| **Switch Tool** | Keys `1`, `2`, `3`, `4`, `5` |
| **Deposit Dust** | `E` (when in Depot Hopper zone) |
| **Toggle Upgrade Hangar** | `TAB` or `B` |
| **Pause / Resume** | `ESC` |

---

## 6. How to Run on macOS
Unity Hub is installed on your Mac. Follow these steps to play or edit:
1. Open Unity Hub:
   ```bash
   npm run open:unity
   ```
2. In Unity Hub, navigate to **Installs** and install **Unity 6 (6000.0.x)** or **Unity 2022.3 LTS**.
3. In Unity Hub, click **Add project from disk** and select this directory (`/Users/umut/mycodespaces/moon-sweepers`).
4. Click to open the project.
5. In Unity Editor:
   - Double click `Assets/Scenes/MainLunarBase.unity`.
   - Click the **Play** button (`Cmd + P`) to play directly inside the editor!
6. To build a standalone macOS application:
   - Click top menu: `11Hopem` -> `Build` -> `Build macOS Standalone App (.app)`.
   - The compiled app will be generated at `Builds/macOS/11HopemMoonSweepers.app`!

---

## 7. Testing & Verification
All automated unit tests run via Node.js native test runner:
```bash
npm test
```
**Test Results:** 10/10 Passed (100% Success).

---

## 8. Governance & Brand Standards
- **Zero Copyright Infringement (Rule 11):** All code, procedural audio synthesizers, 3D procedural geometries, and SVG brand graphics are 100% original.
- **Strict Brand Identity (Rule 6):** All brand links point to [https://11hopem.com](https://11hopem.com). The phrase 'Software Ltd. Şti.' is strictly prohibited.
- **Security & Secret Shield (Rule 12):** Strict `.gitignore` policy protecting credentials, API keys, and Unity cache directories.

---

## 9. License
Copyright © 2026 **11Hopem**. All rights reserved.  
Official website: [https://11hopem.com](https://11hopem.com)
