/**
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 * Licensed under 11Hopem Proprietary License.
 */

import { ToolSystem } from './toolSystem.js';

export class Player {
  constructor(x = 240, y = 400) {
    this.x = x;
    this.y = y;
    this.vx = 0;
    this.vy = 0;
    this.speed = 3.6;
    this.friction = 0.88; // Lunar low-friction gliding
    this.angle = 0;

    // Inventory
    this.currentToolId = 'TOOTHBRUSH';
    this.tool = ToolSystem.getToolById('TOOTHBRUSH');
    this.backpackDust = 0;
    this.backpackMinerals = [];
    this.lastSweepTime = 0;
  }

  setTool(toolId) {
    this.currentToolId = toolId;
    this.tool = ToolSystem.getToolById(toolId);
  }

  getCapacity() {
    return this.tool.backpackCapacity;
  }

  isBackpackFull() {
    return this.backpackDust >= this.getCapacity();
  }

  getRemainingCapacity() {
    return Math.max(0, this.getCapacity() - this.backpackDust);
  }

  /**
   * Handle low-gravity player movement
   */
  update(inputKeys, bounds) {
    let ax = 0;
    let ay = 0;

    if (inputKeys['ArrowUp'] || inputKeys['KeyW']) ay -= 1;
    if (inputKeys['ArrowDown'] || inputKeys['KeyS']) ay += 1;
    if (inputKeys['ArrowLeft'] || inputKeys['KeyA']) ax -= 1;
    if (inputKeys['ArrowRight'] || inputKeys['KeyD']) ax += 1;

    // Normalize diagonal movement
    if (ax !== 0 && ay !== 0) {
      ax *= 0.7071;
      ay *= 0.7071;
    }

    // Apply acceleration
    this.vx += ax * this.speed * 0.45;
    this.vy += ay * this.speed * 0.45;

    // Apply low friction gliding
    this.vx *= this.friction;
    this.vy *= this.friction;

    this.x += this.vx;
    this.y += this.vy;

    // Constrain inside world bounds
    this.x = Math.max(40, Math.min(bounds.width - 40, this.x));
    this.y = Math.max(40, Math.min(bounds.height - 40, this.y));

    // Update facing angle
    if (Math.abs(this.vx) > 0.1 || Math.abs(this.vy) > 0.1) {
      this.angle = Math.atan2(this.vy, this.vx);
    }
  }

  /**
   * Perform a sweep/vacuum action on the world
   */
  sweep(world, nowMs = Date.now()) {
    if (nowMs - this.lastSweepTime < this.tool.cooldownMs) {
      return null; // On cooldown
    }

    if (this.isBackpackFull()) {
      return { status: 'FULL', collectedDust: 0 };
    }

    this.lastSweepTime = nowMs;
    const remaining = this.getRemainingCapacity();
    const result = world.sweepArea(this.x, this.y, this.tool.sweepRadius, remaining);

    this.backpackDust += result.collectedDust;
    if (result.discoveredMinerals.length > 0) {
      this.backpackMinerals.push(...result.discoveredMinerals);
    }

    return {
      status: 'SWEPT',
      collectedDust: result.collectedDust,
      minerals: result.discoveredMinerals
    };
  }

  /**
   * Empty inventory at the depot
   */
  emptyBackpack() {
    const payload = {
      dust: this.backpackDust,
      minerals: [...this.backpackMinerals]
    };

    this.backpackDust = 0;
    this.backpackMinerals = [];
    return payload;
  }
}
