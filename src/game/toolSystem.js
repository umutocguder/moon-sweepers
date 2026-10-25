/**
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 * Licensed under 11Hopem Proprietary License.
 */

/**
 * Tool definitions and upgrade tiers for Moon Sweepers.
 * Starting with a single toothbrush and escalating to industrial rovers.
 */
export const TOOL_TIERS = [
  {
    id: 'TOOTHBRUSH',
    tier: 1,
    name: 'Standard Toothbrush',
    displayName: 'Standart Diş Fırçası',
    description: 'LSD tarafından verilen tek bütçe: Bembeyaz bir diş fırçası.',
    icon: '🪥',
    cost: 0,
    sweepRadius: 36,
    dustPerSweep: 1,
    cooldownMs: 220,
    backpackCapacity: 20,
    isContinuousSuction: false
  },
  {
    id: 'HAND_BROOM',
    tier: 2,
    name: 'Lunar Hand Broom',
    displayName: 'Ay El Süpürgesi',
    description: 'Geniş süpürme açısı ve toz partiküllerini hızla toplama.',
    icon: '🧹',
    cost: 200,
    sweepRadius: 58,
    dustPerSweep: 3,
    cooldownMs: 160,
    backpackCapacity: 60,
    isContinuousSuction: false
  },
  {
    id: 'CYCLONE_VACUUM',
    tier: 3,
    name: 'Cyclone Vacuum Mk-I',
    displayName: 'Siklon Vakum Mk-I',
    description: 'Sürekli çekim gücü; tozu hortumuyla çeker.',
    icon: '🌀',
    cost: 850,
    sweepRadius: 85,
    dustPerSweep: 6,
    cooldownMs: 90,
    backpackCapacity: 160,
    isContinuousSuction: true
  },
  {
    id: 'ELECTRO_MAGNET',
    tier: 4,
    name: 'Electromagnetic Sweeper',
    displayName: 'Elektromanyetik Süpürge',
    description: 'Manyetik regolit aurası; uzaktaki tozları kendine çeker.',
    icon: '⚡',
    cost: 3200,
    sweepRadius: 130,
    dustPerSweep: 12,
    cooldownMs: 60,
    backpackCapacity: 450,
    isContinuousSuction: true
  },
  {
    id: 'LUNAR_ROVER',
    tier: 5,
    name: 'Lunar Sweeper Buggy',
    displayName: 'Ay Süpürme Aracı (Rover)',
    description: 'Çift ön silindirli devasa temizlik canavarı.',
    icon: '🚜',
    cost: 12000,
    sweepRadius: 210,
    dustPerSweep: 25,
    cooldownMs: 40,
    backpackCapacity: 1500,
    isContinuousSuction: true
  }
];

export class ToolSystem {
  static getToolById(id) {
    return TOOL_TIERS.find(t => t.id === id) || TOOL_TIERS[0];
  }

  static getNextTool(currentToolId) {
    const currentIndex = TOOL_TIERS.findIndex(t => t.id === currentToolId);
    if (currentIndex >= 0 && currentIndex < TOOL_TIERS.length - 1) {
      return TOOL_TIERS[currentIndex + 1];
    }
    return null; // Max tier reached
  }

  static canAffordTool(tool, credits) {
    return credits >= tool.cost;
  }
}
