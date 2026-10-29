/*
 * Created by 11Hopem
 * Copyright (c) 2026 11Hopem. All rights reserved.
 */

using System;
using UnityEngine;

namespace ElevenHopem.MoonSweepers.Tools
{
    public enum ToolMode
    {
        ClickScrub,
        ContinuousHold
    }

    [Serializable]
    public class ToolData
    {
        public string toolName;
        public int tier;
        public ToolMode mode;
        public float sweepPowerKg;
        public float sweepRangeMeters;
        public float sweepRadiusMeters;
        public float useCooldownSeconds;
        public long unlockCostCredits;
        public Color toolVisualColor;

        public ToolData(string name, int tier, ToolMode mode, float power, float range, float radius, float cooldown, long cost, Color color)
        {
            this.toolName = name;
            this.tier = tier;
            this.mode = mode;
            this.sweepPowerKg = power;
            this.sweepRangeMeters = range;
            this.sweepRadiusMeters = radius;
            this.useCooldownSeconds = cooldown;
            this.unlockCostCredits = cost;
            this.toolVisualColor = color;
        }
    }
}
