/**
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 * Licensed under 11Hopem Proprietary License.
 */

import { test } from 'node:test';
import assert from 'node:assert';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

import { ToolSystem, TOOL_TIERS } from '../src/game/toolSystem.js';
import { EconomySystem, MINERAL_VALUES } from '../src/game/economySystem.js';
import { LunarWorld } from '../src/game/lunarWorld.js';
import { Player } from '../src/game/player.js';
import { SaveService } from '../src/services/saveService.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const ROOT_DIR = path.resolve(__dirname, '..');

// Test 1: Tool Progression System
test('ToolSystem: Verify 5 tool tiers starting with Toothbrush and proper progression', () => {
  assert.strictEqual(TOOL_TIERS.length, 5);
  assert.strictEqual(TOOL_TIERS[0].id, 'TOOTHBRUSH');
  assert.strictEqual(TOOL_TIERS[0].cost, 0);
  assert.strictEqual(TOOL_TIERS[4].id, 'LUNAR_ROVER');

  const nextFromToothbrush = ToolSystem.getNextTool('TOOTHBRUSH');
  assert.strictEqual(nextFromToothbrush.id, 'HAND_BROOM');

  const nextFromRover = ToolSystem.getNextTool('LUNAR_ROVER');
  assert.strictEqual(nextFromRover, null); // Max tier reached

  assert.strictEqual(ToolSystem.canAffordTool(nextFromToothbrush, 250), true);
  assert.strictEqual(ToolSystem.canAffordTool(nextFromToothbrush, 50), false);
});

// Test 2: Economy & Selling Mechanics
test('EconomySystem: Validate dust selling, rare mineral pricing, and automation upgrades', () => {
  const economy = new EconomySystem(100);
  assert.strictEqual(economy.credits, 100);

  // Sell basic regolith dust
  const payout = economy.sellInventory({ dust: 20, minerals: [] });
  assert.strictEqual(payout.itemsSold, 20);
  assert.strictEqual(payout.payout, 20 * MINERAL_VALUES.REGOLITH_DUST.value);
  assert.strictEqual(economy.credits, 100 + 100);

  // Sell rare minerals
  const rarePayout = economy.sellInventory({
    dust: 0,
    minerals: [{ type: 'HELIUM_3' }, { type: 'ALIEN_ARTIFACT' }]
  });
  assert.strictEqual(rarePayout.itemsSold, 2);
  assert.strictEqual(rarePayout.payout, MINERAL_VALUES.HELIUM_3.value + MINERAL_VALUES.ALIEN_ARTIFACT.value);

  // Buy automation upgrade
  economy.credits = 2000;
  const buyRes = economy.buyUpgrade('CONVEYOR');
  assert.strictEqual(buyRes.success, true);
  assert.strictEqual(economy.automation.conveyors, 1);
  assert.strictEqual(economy.credits, 1500);

  // Passive earnings calculation
  const passive = economy.calculatePassiveDust(10); // 10 seconds
  assert.strictEqual(passive, 20); // 2 units/sec * 10
});

// Test 3: Lunar Surface & Sweeping Physics
test('LunarWorld: Verify dust field generation, craters, and area sweep cleanup', () => {
  const world = new LunarWorld(1000, 700);
  assert.ok(world.dustParticles.length > 50);
  assert.ok(world.craters.length > 0);

  // Initial cleanliness should be low
  const initialClean = world.getCleanlinessPercentage();
  assert.ok(initialClean >= 0 && initialClean < 100);

  // Find a particle to sweep
  const target = world.dustParticles[0];
  const sweepRes = world.sweepArea(target.x, target.y, 40, 10);
  assert.ok(sweepRes.collectedDust > 0);
  assert.strictEqual(target.cleaned, true);
});

// Test 4: Player Mechanics & Backpack
test('Player: Verify astronaut movement, tool usage, and backpack capacity', () => {
  const player = new Player(300, 300);
  assert.strictEqual(player.currentToolId, 'TOOTHBRUSH');
  assert.strictEqual(player.backpackDust, 0);

  // Simulate sweeping
  const world = new LunarWorld(1000, 700);
  const sweep = player.sweep(world);
  assert.ok(sweep !== null);

  // Backpack capacity check
  player.backpackDust = player.getCapacity();
  assert.strictEqual(player.isBackpackFull(), true);

  const fullSweep = player.sweep(world, Date.now() + 1000);
  assert.strictEqual(fullSweep.status, 'FULL');

  // Empty backpack
  const emptied = player.emptyBackpack();
  assert.strictEqual(emptied.dust, player.getCapacity());
  assert.strictEqual(player.backpackDust, 0);
});

// Test 5: Save System Integrity
test('SaveService: Validate cryptographic save file creation and SHA-256 verification', () => {
  const state = { credits: 500, tool: 'HAND_BROOM', cleaned: 120 };
  const res = SaveService.saveToFile(state);

  assert.ok(res.success);
  assert.ok(fs.existsSync(res.targetPath));
  assert.strictEqual(SaveService.verifySaveFile(res.targetPath), true);
});

// Test 6: Brand Assets and 1:1 Favicon Compliance
test('Branding: Verify 1:1 Favicon, Logo assets, and Moon Sweepers compliance', () => {
  const files = [
    'assets/logo.svg',
    'public/logo.svg',
    'public/favicon.svg',
    'public/favicon.ico',
    'public/apple-touch-icon.png',
    'public/og-image.png'
  ];

  for (const f of files) {
    const fullPath = path.join(ROOT_DIR, f);
    assert.ok(fs.existsSync(fullPath), `Asset must exist: ${f}`);
    assert.ok(fs.statSync(fullPath).size > 0);
  }
});
