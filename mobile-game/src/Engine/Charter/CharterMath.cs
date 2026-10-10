namespace Slumberbloom.Engine.Charter;

/// <summary>
/// The Charter: supply falls out of published rates, not a magic number.
/// s_k = S_k / D_max  where  S_k = S_0 / 2^k
/// Ceiling = S_0 + S_0/2 + S_0/4 + … (K eras)
/// Mirrors packages/engine/src/charter/sizing.ts.
/// </summary>
public static class CharterMath
{
    /// <summary>Seed supply per Dawn in era k: s_k = S_0 / 2^k / D_max.</summary>
    public static double SeedsPerDawn(double s0, int era, double dMax)
        => (s0 / Math.Pow(2, era)) / dMax;

    /// <summary>Maximum that can ever exist across K eras (geometric series).</summary>
    public static double Ceiling(double s0, int eras)
    {
        // S_0 + S_0/2 + S_0/4 + … + S_0/2^(K-1) = S_0 · (2 - 2^(1-K))
        return s0 * (2 - Math.Pow(2, 1 - eras));
    }

    /// <summary>Era k → era multiplier for plant growth (gen0 ×1.0, genI ×0.5, …).</summary>
    public static double EraMultiplier(int era) => 1.0 / Math.Pow(2, era);

    /// <summary>Era label — Era 0 = Genesis, I, II, III, IV · Great Bloom.</summary>
    public static string EraName(int era) => era switch
    {
        0 => "Genesis",
        1 => "I",
        2 => "II",
        3 => "III",
        4 => "IV · Great Bloom",
        _ => $"Era {era}",
    };
}
