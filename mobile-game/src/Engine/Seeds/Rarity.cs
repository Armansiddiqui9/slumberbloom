namespace Slumberbloom.Engine.Seeds;

/// <summary>Rarity tiers — position in the chain makes rarity, unforgeable.</summary>
public enum Rarity
{
    Common,     // ⚪ Everything else
    Dawnling,   // 🟢 First seed of each Dawn
    Solstice,   // 🔵 First seed after solstice/equinox
    Eraborn,    // 🟣 First seed of each era
    Heirloom,   // 🟠 First seed of each Great Bloom cycle
    Genesis,    // 🌟 Seed #1, the Mother Tree
}

public static class RarityExt
{
    public static string Emoji(this Rarity r) => r switch
    {
        Rarity.Common => "⚪",
        Rarity.Dawnling => "🟢",
        Rarity.Solstice => "🔵",
        Rarity.Eraborn => "🟣",
        Rarity.Heirloom => "🟠",
        Rarity.Genesis => "🌟",
        _ => "⚪",
    };

    public static string Label(this Rarity r) => r switch
    {
        Rarity.Common => "Common",
        Rarity.Dawnling => "Dawnling · Uncommon",
        Rarity.Solstice => "Solstice · Rare",
        Rarity.Eraborn => "Eraborn · Epic",
        Rarity.Heirloom => "Heirloom · Legendary",
        Rarity.Genesis => "Genesis · Mythic",
        _ => "Common",
    };

    /// <summary>From chain position + era. Pure function of the chain, can't be farmed.</summary>
    public static Rarity FromPosition(int height, int era, bool isFirstOfDawn, bool isAfterSolstice, bool isEraStart)
    {
        if (height == 1 && era == 0) return Rarity.Genesis;
        if (isEraStart) return Rarity.Eraborn;
        if (isAfterSolstice) return Rarity.Solstice;
        if (isFirstOfDawn) return Rarity.Dawnling;
        return Rarity.Common;
    }
}
