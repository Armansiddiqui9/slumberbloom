using Slumberbloom.Engine;
using Slumberbloom.Contracts;
using System.Text.Json;

namespace Slumberbloom.UI;

/// <summary>
# .NET Console Demo — runs the full SlumberBloom loop (tuck-in, open, claim, dawn, lantern) on any .NET SDK.
# To open in Android Studio later: copy mobile-game/ → Android Studio → import as C# .NET project.
# For Unity: move src/Engine/ and src/Contracts/ into Assets/Scripts/; LocalApi stays as backend.
# To build Android from here: add a MAUI head (MobileGame.csproj) pointing to this lib.
# </summary>
public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  SlumberBloom — Mobile Game Demo (v1 scaffold)            ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("Architecture mapping (web → mobile-game):");
        Console.WriteLine("  packages/engine/          →  src/Engine/   (pure C#)");
        Console.WriteLine("  packages/contracts/       →  src/Contracts/ (GameApi interface)");
        Console.WriteLine("  packages/adapters-local/  →  src/Backend/   (LocalApi, in-device server)");
        Console.WriteLine("  apps/web/src/app/         →  src/UI/       (screens — MAUI / Unity)");
        Console.WriteLine();

        // Create a working backend
        var api = new Backend.LocalApi();

        // 1. Tuck in
        await api.TuckIn();
        Console.WriteLine("[ONBOARDING] Tuck in — clock stamped.");

        // 2. Simulate 8h away (time travel, like web dev button +8h)
        await api.Dev.Advance(8);
        Console.WriteLine("[DEMO] +8h time travel (like site / +8h button).");

        // 3. Morning dew — open block
        var dew = await api.OpenBlock();
        if (dew == null)
        {
            Console.WriteLine("[DEW] Nothing yet — need 30+ min away.");
            return;
        }
        Console.WriteLine($"[DEW] Night Block #{dew.Height} | {dew.Ms / 3600000.0:F1}h patience | stage={dew.Stage}");
        Console.WriteLine($"     Hash: {dew.Hash.Substring(0, 16)}…");

        // 4. Claim
        await api.ClaimBlock(dew.Hash, Guid.NewGuid().ToString());
        Console.WriteLine("[CLAIM] Dew → Gathered.");

        // 5. Dawn — settle
        int frostCount = await api.SettleDawn();
        Console.WriteLine($"[DAWN] {frostCount} block(s) sealed → Frost. Bloom updated.");

        // 6. Lantern
        var proof = await api.Lantern(dew.Hash);
        Console.WriteLine($"[LANTERN] Verified={proof.Ok} | depth={proof.Depth} | root={proof.Root.Substring(0, 10)}…");

        // 7. Garden
        var g = await api.GetGarden();
        Console.WriteLine($"[GARDEN] Growth={g.Growth} | Stage={g.Stage} | Wilt={g.Wilt} | Sun={g.Sun} Moon={g.Moon} Bloom={g.Bloom}");

        // 8. Water
        var watered = await api.Water();
        Console.WriteLine($"[WATER] Growth={watered.Growth} (spent 1 Sun).");

        Console.WriteLine();
        Console.WriteLine("Next: open in Android Studio → File → New → Import Project → mobile-game/");
        Console.WriteLine("      Unity: copy src/Engine/ + src/Contracts/ into Assets/Scripts/");
    }
}
