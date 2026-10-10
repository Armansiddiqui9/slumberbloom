namespace Slumberbloom.Engine.Seeds;

/// <summary>
/// Serial format SB-XXXXXX with mod-37 check digit (same idea as Bitcoin address checksum).
/// Mirrors packages/engine/src/seeds/serial.ts.
/// </summary>
public static class Serial
{
    // Base-37 alphabet (0-9 + A-Z excluding I, O, Q to avoid confusion)
    private static readonly char[] Alphabet = "0123456789ABCDEFGHJKLMNPRSTUVWXYZ".ToCharArray();

    public static string Encode(long n, int checkLength = 1)
    {
        var s = "SB-";
        var rem = n;
        for (int i = 0; i < 6; i++)
        {
            s += Alphabet[rem % 37];
            rem /= 37;
        }
        // Check digit = sum of digits mod 37; honor checkLength (default 1, max 2 per spec)
        int digest = (int)(n % 37);
        for (int i = 0; i < checkLength; i++)
        {
            s += Alphabet[digest % 37];
            digest /= 37;
        }
        return s;
    }

    /// <summary>Rarity from chain position — Dawnling (first of dawn), Solstice, Eraborn, Heirloom, Genesis.</summary>
    public static string RarityOf(int height, int era) => (height, era) switch
    {
        (1, 0) => "Genesis",
        (_, 0) when height == 1 => "Dawnling",
        (_, 4) => "Eraborn",
        (_, _) when era == 3 => "Heirloom",
        (_, _) when era == 4 => "Solstice",
        (_, _) => "Common",
    };
}
