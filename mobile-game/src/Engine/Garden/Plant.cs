namespace Slumberbloom.Engine;

/// <summary>Plant growth, stages, and the wilt ladder. Mirrors packages/engine/src/garden/plant.ts.</summary>
public static class Plant
{
    public const int MaxGrowth = 100;

    /// <summary>0–24 seed · 25–59 sprout · 60–99 bud · 100 bloom</summary>
    public static string StageOf(double growth) =>
        growth >= 100 ? "bloom" : growth >= 60 ? "bud" : growth >= 25 ? "sprout" : "seed";

    /// <summary>Wilt ladder (no permanent death): healthy → droop 24h → brown 48h → dormant 120h.</summary>
    public static string WiltOf(double hoursUnwatered) =>
        hoursUnwatered >= 120 ? "dormant" : hoursUnwatered >= 48 ? "brown" : hoursUnwatered >= 24 ? "droop" : "healthy";

    /// <summary>Watering: growth +25 × era multiplier, capped at 100.</summary>
    public static double Water(double growth, double eraMult = 1.0) =>
        Math.Min(MaxGrowth, growth + 25 * eraMult);

    /// <summary>Passive offline growth: +1/hr, capped at 8h per watering.</summary>
    public static double PassiveGrowth(double growth, double awayHours) =>
        Math.Min(MaxGrowth, growth + Math.Min(awayHours, 8));
}