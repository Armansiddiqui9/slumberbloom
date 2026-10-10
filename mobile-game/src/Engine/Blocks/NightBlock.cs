using System.Security.Cryptography;
using System.Text;
using Slumberbloom.Engine.Crypto;
using Slumberbloom.Engine.Encoding;

namespace Slumberbloom.Engine;

/// <summary>
/// NightBlock = interval capped away time, hashed with SHA-256 + canonical JSON.
/// Mirrors packages/engine/src/blocks/night-block.ts.
/// </summary>
public sealed class NightBlock
{
    public int Height { get; init; }
    public string Prev { get; init; } = "genesis";
    public long Start { get; init; } // epoch ms
    public long End { get; init; }   // epoch ms
    public long Ms { get; init; }    // clamped away time
    public string Hash { get; private set; } = "";

    /// <summary>Mint a new NightBlock from lastSeen and now. Returns null if away &lt; 30 min.</summary>
    public static NightBlock? MintNightBlock(NightBlock? prev, long lastSeen, long now)
    {
        const long MinMs = 30 * 60_000L;    // dust rule: 30 min
        const long MaxMs = 10 * 3_600_000L; // interval cap: 10 h

        var away = now - lastSeen;
        if (away < MinMs) return null;

        var b = new NightBlock
        {
            Height = (prev?.Height ?? 0) + 1,
            Prev = prev?.Hash ?? "genesis",
            Start = lastSeen,
            End = now,
            Ms = Math.Min(away, MaxMs),
        };
        b.Hash = Hash.H(CanonicalJson.CanonOf(b));
        return b;
    }
}