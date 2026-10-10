using Slumberbloom.Contracts;

namespace Slumberbloom.UI;

/// <summary>
/// Screen identifiers — mirrors the web app's tabs:
/// bed (tuck-in), dew (morning reveal), garden, census — plus onboarding.
/// </summary>
public enum ScreenId
{
    Onboarding,
    Bedtime,
    MorningDew,
    Garden,
    Census,
}

/// <summary>
/// Screen model. Mirrors the React App.tsx state bundle (tab, garden, blocks, away, proof).
/// Screens render from this and call back through IGameApi — never touching state directly.
/// </summary>
public sealed class AppModel
{
    public required IGameApi Api { get; init; }
    public ScreenId Screen { get; set; } = ScreenId.Onboarding;
    public Garden? Garden { get; set; }
    public IReadOnlyList<BlockView> Blocks { get; set; } = Array.Empty<BlockView>();
    public bool Away { get; set; }
    public Proof? Proof { get; set; }
    public string SelectedSeedId { get; set; } = "moonflower";

    /// <summary>Mirrors the web's refresh(): openBlock → getGarden → getBlocks.</summary>
    public async Task RefreshAsync()
    {
        await Api.OpenBlock();
        Garden = await Api.GetGarden();
        Blocks = await Api.GetBlocks();
    }
}

/// <summary>"7h 42m" — mirrors the web's hm() helper.</summary>
public static class Format
{
    public static string Hm(long ms) =>
        $"{Math.Floor(ms / 3_600_000.0)}h {Math.Floor((ms % 3_600_000) / 60_000.0)}m";
}
