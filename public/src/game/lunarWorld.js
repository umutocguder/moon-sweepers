/**
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 * Licensed under 11Hopem Proprietary License.
 */

/**
 * Lunar World Simulation: Surface terrain, dust particles, craters, and the Depot Hopper.
 */
export class LunarWorld {
  constructor(width = 1200, height = 800) {
    this.width = width;
    this.height = height;
    this.depot = {
      x: 140,
      y: 400,
      radius: 90,
      name: 'LSD Central Depot (Hopper)'
    };
    this.dustParticles = [];
    this.craters = [];
    this.rareMinerals = [];
    this.initTerrain();
  }

  /**
   * Initialize craters and dust field
   */
  initTerrain() {
    this.dustParticles = [];
    this.craters = [];
    this.rareMinerals = [];

    // 1. Procedural Craters
    const craterCount = 8;
    for (let i = 0; i < craterCount; i++) {
      this.craters.push({
        x: 250 + Math.random() * (this.width - 320),
        y: 80 + Math.random() * (this.height - 160),
        radius: 25 + Math.random() * 45
      });
    }

    // 2. Generate Dense Dust Field across the lunar surface
    // Grid-based sampling for optimal performance
    const step = 28;
    for (let x = 60; x < this.width - 60; x += step) {
      for (let y = 60; y < this.height - 60; y += step) {
        // Skip inside the Depot zone
        const distToDepot = Math.hypot(x - this.depot.x, y - this.depot.y);
        if (distToDepot < this.depot.radius + 40) continue;

        // Slight jitter for natural look
        const jx = x + (Math.random() - 0.5) * 14;
        const jy = y + (Math.random() - 0.5) * 14;

        this.dustParticles.push({
          x: jx,
          y: jy,
          density: 1 + Math.floor(Math.random() * 3), // 1-3 layers of dust
          cleaned: false
        });

        // 3. Chance of rare mineral hidden under dust (3% chance)
        if (Math.random() < 0.035) {
          const types = ['HELIUM_3', 'TITANIUM_CHUNK', 'ALIEN_ARTIFACT'];
          const picked = types[Math.floor(Math.random() * types.length)];
          this.rareMinerals.push({
            x: jx,
            y: jy,
            type: picked,
            discovered: false
          });
        }
      }
    }
  }

  /**
   * Sweeps dust particles within radius of (x, y)
   * @param {number} x - Tool position
   * @param {number} y - Tool position
   * @param {number} radius - Sweep reach
   * @param {number} maxCollect - Space available in backpack
   * @returns {Object} { collectedDust: number, discoveredMinerals: Array }
   */
  sweepArea(x, y, radius, maxCollect) {
    let collectedDust = 0;
    const discoveredMinerals = [];

    if (maxCollect <= 0) {
      return { collectedDust: 0, discoveredMinerals: [] };
    }

    for (const p of this.dustParticles) {
      if (p.cleaned) continue;

      const dist = Math.hypot(p.x - x, p.y - y);
      if (dist <= radius) {
        const canTake = Math.min(p.density, maxCollect - collectedDust);
        if (canTake > 0) {
          p.density -= canTake;
          collectedDust += canTake;

          if (p.density <= 0) {
            p.cleaned = true;

            // Check if a rare mineral was uncovered here
            for (const m of this.rareMinerals) {
              if (!m.discovered && Math.hypot(m.x - p.x, m.y - p.y) < 20) {
                m.discovered = true;
                discoveredMinerals.push(m);
              }
            }
          }
        }
      }

      if (collectedDust >= maxCollect) break;
    }

    return {
      collectedDust,
      discoveredMinerals
    };
  }

  /**
   * Check if player is standing within the Depot sell zone
   */
  isPlayerInDepot(px, py) {
    return Math.hypot(px - this.depot.x, py - this.depot.y) <= this.depot.radius;
  }

  /**
   * Calculate percentage of the moon surface that has been swept clean
   */
  getCleanlinessPercentage() {
    if (this.dustParticles.length === 0) return 100;
    const cleaned = this.dustParticles.filter(p => p.cleaned).length;
    return Math.round((cleaned / this.dustParticles.length) * 100);
  }
}
