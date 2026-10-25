/**
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 * Licensed under 11Hopem Proprietary License.
 */

/**
 * Economy, Upgrades and Automation Station Manager.
 */
export const MINERAL_VALUES = {
  REGOLITH_DUST: { name: 'Ay Tozu (Regolit)', value: 5, icon: '🌫️' },
  HELIUM_3: { name: 'Helyum-3 Kristali', value: 75, icon: '💎' },
  TITANIUM_CHUNK: { name: 'Titanyum Külçesi', value: 160, icon: '🪨' },
  ALIEN_ARTIFACT: { name: 'Kadim Uzaylı Vidası', value: 600, icon: '🛸' }
};

export const AUTOMATION_UPGRADES = [
  {
    id: 'CONVEYOR',
    name: 'Conveyor Belt System',
    displayName: 'Konveyör Bant Hattı',
    desc: 'Toplanan tozları depoya otomatik aktarır.',
    cost: 500,
    ratePerSec: 2,
    icon: '🛤️'
  },
  {
    id: 'DRONE',
    name: 'Auto-Sweep Cleaning Drone',
    displayName: 'Otonom Temizlik Dronu',
    desc: 'Yüzeyde uçarak tozu otonom süpürür.',
    cost: 1500,
    ratePerSec: 6,
    icon: '🤖'
  },
  {
    id: 'FILTER',
    name: 'Dust Refinement Chamber',
    displayName: 'Toz Saflaştırma İstasyonu',
    desc: 'Tüm toz satış değerini kalıcı olarak +%50 artırır.',
    cost: 3000,
    multiplier: 1.5,
    icon: '🔬'
  },
  {
    id: 'SOLAR',
    name: 'Lunar Solar Array',
    displayName: 'Güneş Enerjisi Dizisi',
    desc: 'Otonom sistemlerin hızını 2 katına çıkarır.',
    cost: 7500,
    multiplier: 2.0,
    icon: '☀️'
  }
];

export class EconomySystem {
  constructor(initialCredits = 0) {
    this.credits = initialCredits;
    this.totalDustCleaned = 0;
    this.totalCreditsEarned = 0;
    this.automation = {
      conveyors: 0,
      drones: 0,
      hasFilter: false,
      hasSolar: false
    };
  }

  /**
   * Calculate sell payout for inventory batch
   * @param {Object} inventory - { dust: number, minerals: Array }
   * @returns {Object} { payout: number, itemsSold: number }
   */
  calculateSellPayout(inventory) {
    const filterMult = this.automation.hasFilter ? 1.5 : 1.0;
    let payout = 0;
    let itemsSold = 0;

    // Dust value
    const dustCount = Number(inventory.dust || 0);
    payout += dustCount * MINERAL_VALUES.REGOLITH_DUST.value * filterMult;
    itemsSold += dustCount;

    // Rare minerals
    if (Array.isArray(inventory.minerals)) {
      for (const mineral of inventory.minerals) {
        const itemDef = MINERAL_VALUES[mineral.type] || MINERAL_VALUES.REGOLITH_DUST;
        payout += itemDef.value * filterMult;
        itemsSold++;
      }
    }

    return {
      payout: Math.round(payout),
      itemsSold
    };
  }

  /**
   * Deposit collected dust and minerals at the depot hopper
   */
  sellInventory(inventory) {
    const result = this.calculateSellPayout(inventory);
    this.credits += result.payout;
    this.totalCreditsEarned += result.payout;
    this.totalDustCleaned += result.itemsSold;

    return result;
  }

  /**
   * Calculate passive automated collection per tick
   * @param {number} deltaSeconds
   * @returns {number} Passive dust collected
   */
  calculatePassiveDust(deltaSeconds) {
    let rate = 0;
    rate += this.automation.conveyors * 2;
    rate += this.automation.drones * 6;

    if (this.automation.hasSolar) {
      rate *= 2.0;
    }

    return rate * deltaSeconds;
  }

  /**
   * Purchase an automation upgrade
   */
  buyUpgrade(upgradeId) {
    const upgrade = AUTOMATION_UPGRADES.find(u => u.id === upgradeId);
    if (!upgrade || this.credits < upgrade.cost) {
      return { success: false, message: 'Yetersiz Lunar Kredisi (LC)!' };
    }

    this.credits -= upgrade.cost;

    if (upgradeId === 'CONVEYOR') {
      this.automation.conveyors++;
    } else if (upgradeId === 'DRONE') {
      this.automation.drones++;
    } else if (upgradeId === 'FILTER') {
      this.automation.hasFilter = true;
    } else if (upgradeId === 'SOLAR') {
      this.automation.hasSolar = true;
    }

    return { success: true, upgrade };
  }
}
