using Slumberbloom.Contracts;

namespace Slumberbloom.UI;

/// <summary>Formatting helpers (hm only). ScreenId lives in AppModel.cs.</summary>
public static class Format
{
    public static string Hm(long ms) =>
        $"{Math.Floor(ms / 3_600_000.0)}h {Math.Floor((ms % 3_600_000) / 60_000.0)}m";
}
