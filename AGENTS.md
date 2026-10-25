# AGENTS.md - 11Hopem Moon Sweepers Game

## Project Overview
- **Name:** `moon-sweepers` (11Hopem Moon Sweepers)
- **Scope:** Lunar dust cleanup, simulation, tycoon, and automation incremental game. Starting with a single toothbrush and scaling into industrial vacuum sweepers, conveyor belts, and autonomous drones.
- **Brand Identity:** Exclusively `11Hopem` with redirect to `https://11hopem.com`.
- **Live URL:** `https://moon-sweepers.vercel.app`
- **GitHub Repository:** Private repo `umutocguder/moon-sweepers`
- **Antigravity Project ID:** `f0ecd30b-5512-4d20-a6b9-125b0d2c30eb`

---

## Architecture & Tech Stack
- **Game Engine:** HTML5 Canvas, 60 FPS requestAnimationFrame loop, pure ES Modules in `src/game/`:
  - `toolSystem.js`: 5 tool tiers (Toothbrush -> Broom -> Cyclone Vacuum -> Electro Sweeper -> Lunar Buggy).
  - `economySystem.js`: Lunar Credits (LC) economy, mineral payouts, and automation stations (Conveyors, Drones, Solar Arrays).
  - `lunarWorld.js`: Procedural lunar craters, regolith dust particle fields, depot hopper zones.
  - `player.js`: Astronaut character, low-gravity gliding physics, backpack capacity management.
  - `audioSynth.js`: Pure Web Audio API procedural sound synthesizer (brush scrape, vacuum hum, coin chime).
- **Data Custody & Security:**
  - `src/services/saveService.js`: Rotating local SHA-256 cryptographic backups in `data/backups/`.
  - `src/telemetry/telemetryService.js`: Non-blocking Firebase telemetry for `hopem-software-cloud`.
- **Frontend Vitrine:** Vanilla HTML5, Tailwind CSS CDN, Lucide Icons, Glassmorphism UI in `public/index.html`.
- **Testing:** Node.js native test runner (`node --test tests/*.test.js`).
- **Build / Packaging:** `scripts/build.js` synchronizes assets into `public/`.
- **Hosting & CI/CD:** Vercel Production Deployment + GitHub Actions CI/CD.

---

## Development & Verification Commands
- **Install Dependencies:** `npm install`
- **Run Unit Tests:** `npm test`
- **Build Production Assets:** `npm run build`
- **Create Local Backup:** `npm run backup`
- **Start Local Dev Server:** `npm start`
- **Deploy to Vercel:** `npx -y vercel --prod --yes`

---

## Engineering & Governance Rules
1. **100% English Code Standard:** All code, test definitions, scripts, commit messages, and configs must be in English. User-facing UI can be in Turkish.
2. **Strict Brand Compliance:** All links to brand must point directly to `https://11hopem.com`. Never use 'Software Ltd. Şti.'.
3. **1:1 Favicon & Brand Asset Rule:** All favicon and social graph assets (`favicon.svg`, `favicon.ico`, `logo.svg`, `apple-touch-icon.png`, `og-image.png`) must be present in both `assets/` and `public/`.
4. **Git Branching Policy:** `master` represents production deployment; `dev` represents staging/UAT.
5. **Zero Copyright Infringement:** 100% original code, procedural synth audio, and custom SVG game art.
