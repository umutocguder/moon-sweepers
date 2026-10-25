using System;
using UnityEngine;

namespace ElevenHopem.MoonSweepers.Tools
{
    public enum ToolMode
    {
        ClickScrub,     // Toothbrush, Broom (tap to scrub)
        ContinuousHold  // Cyclone Vacuum, Electro Sweeper, Buggy (hold left click)
    }

    [Serializable]
    public class ToolData
    {
        public string toolName;
        public int tier;
        public ToolMode mode;
        public float sweepPowerKg;      // kg cleaned per hit or per second
        public float sweepRangeMeters;  // distance reach
        public float sweepRadiusMeters; // radius around hit point
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
