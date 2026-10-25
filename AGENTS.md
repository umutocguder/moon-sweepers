# AGENTS.md - 11Hopem Moon Sweepers Game

## Project Overview
- **Name:** `moon-sweepers` (11Hopem Moon Sweepers)
- **Scope:** Lunar dust cleanup, simulation, tycoon, and automation 3D game matching the architecture of *Moon Sweepers* (DragDrop Games, Steam App ID 5335510). Starting with a single toothbrush and scaling into industrial vacuum sweepers, conveyor belts, and autonomous drones under real lunar gravity.
- **Engine:** Unity 3D Engine (C# / Universal Render Pipeline) — the exact engine used by the original *Moon Sweepers*.
- **Target Platforms:** macOS Standalone (.app) with Apple Silicon / Intel universal binary support, and cross-platform PC.
- **Brand Identity:** Exclusively `11Hopem` with redirect to `https://11hopem.com`.
- **GitHub Repository:** Private repo `umutocguder/moon-sweepers`
- **Antigravity Project ID:** `f0ecd30b-5512-4d20-a6b9-125b0d2c30eb`

---

## Architecture & Tech Stack
- **Game Engine:** Unity 3D Engine (C# / Universal Render Pipeline):
  - `Assets/Scripts/Core/GameManager.cs`: Singleton game lifecycle, lunar gravity (-1.62 m/s²), pause states, credits, and global loop.
  - `Assets/Scripts/Core/AudioManager.cs`: Procedural audio synthesizer (brush scratch, vacuum hum, coin chime, thrusters) ensuring 100% zero external audio file copyright risk (Rule 11).
  - `Assets/Scripts/Core/SaveManager.cs`: Cryptographic JSON game saves with SHA-256 integrity verification.
  - `Assets/Scripts/Player/PlayerController.cs`: 3D astronaut low-gravity movement, float-jumping, thruster boost with fuel gauge, and first-person camera look.
  - `Assets/Scripts/Player/PlayerInventory.cs`: Regolith dust weight management (kg), backpack capacity gates, and rare mineral pickups (Helium-3, Titanium Chunks).
  - `Assets/Scripts/Tools/ToolController.cs` & `CleaningTool.cs`: 5 tool progression tiers (Toothbrush -> Hand Broom -> Cyclone Vacuum Mk-I -> Electromagnetic Sweeper -> Lunar Buggy Sweeper).
  - `Assets/Scripts/Environment/LunarSurfaceGenerator.cs`: Procedural crater mesh generator, ambient lunar boulders, and interactive regolith dust disk patches.
  - `Assets/Scripts/Environment/RegolithDustPatch.cs`: Interactive shrinking dust nodes, dust gathering, and rare mineral roll drop table.
  - `Assets/Scripts/Environment/DepotHopper.cs`: Base hopper station with trigger zone for unloading dust and converting to Lunar Credits (LC).
  - `Assets/Scripts/Economy/ShopManager.cs`: Upgrades for tools, backpack capacities, auxiliary jetpack fuel, conveyors, and drones.
  - `Assets/Scripts/Economy/AutomationStation.cs`: Conveyor belts and autonomous patrol drones that clean dust automatically.
  - `Assets/Scripts/UI/HUDController.cs`: Dual OnGUI + Canvas HUD with backpack fill gauge, fuel meter, active tool display, crosshair, and depot prompts.
  - `Assets/Scripts/UI/ShopUIController.cs`: Tab-activated glassmorphism hangar upgrade shop.
  - `Assets/Scripts/Editor/MacBuildScript.cs`: Standalone macOS (.app) builder.
  - `Assets/Scripts/Editor/LunarSceneBuilder.cs`: 1-click procedural lunar scene generator.
- **Scenes:** `Assets/Scenes/MainLunarBase.unity`.
- **Configuration:** `ProjectSettings/`, `Packages/manifest.json`.
- **Testing:** Node.js native test runner (`node --test tests/*.test.js`) verifying 10/10 test specifications across C# scripts, Unity scene YAML, project settings, brand links, and security.

---

## Development & Verification Commands
- **Install Dependencies:** `npm install`
- **Run Unit Tests:** `npm test`
- **Sync Unity Meta Files:** `npm run meta`
- **Open Project in Unity Hub:** `npm run open:unity`
- **Build macOS Standalone (.app):** `npm run build:mac`
- **Create Local Backup:** `npm run backup`

---

## Engineering & Governance Rules
1. **100% English Code Standard:** All code, test definitions, scripts, commit messages, and configs must be in English. User-facing UI can be in Turkish.
2. **Strict Brand Compliance:** All links to brand must point directly to `https://11hopem.com`. Never use 'Software Ltd. Şti.'.
3. **1:1 Favicon & Brand Asset Rule:** All favicon and social graph assets (`favicon.svg`, `favicon.ico`, `logo.svg`, `apple-touch-icon.png`, `og-image.png`) must be present in both `assets/` and `Assets/`.
4. **Git Branching Policy:** `master` represents production deployment; `dev` represents staging/UAT.
5. **Zero Copyright Infringement (Rule 11):** 100% original code, procedural synth audio, and custom SVG game art.
6. **Strict Security (Rule 12):** Comprehensive `.gitignore` protecting against secrets, keys, and Unity cache churn.
