using Slumberbloom.Contracts;

namespace Slumberbloom.UI;

/// <summary>
/// Morning Dew — "While you were away."
/// Mirrors apps/web/src/app/App.tsx tab === "dew".
/// </summary>
public static class MorningDewScreen
{
    public static void Render(AppModel app)
    {
        var dew = app.Blocks.FirstOrDefault(b => b.Stage == BlockStage.Dew);
        var gathered = app.Blocks.Any(b => b.Stage == BlockStage.Gathered);
        var last = app.Blocks.LastOrDefault();

        Console.WriteLine("  ╭──────────────────────────────────────────────╮");
        Console.WriteLine("  │   💧  Morning Dew — While You Were Away      │");
        Console.WriteLine("  ╰──────────────────────────────────────────────╯");
        Console.WriteLine();

        if (dew != null)
        {
            Console.WriteLine($"  {Format.Hm(dew.Ms)} of patience hashed into Night Block #{dew.Height}");
            Console.WriteLine($"  {dew.Hash}");
            Console.WriteLine();
            Console.WriteLine("  [1] 💧 Claim Dew");
        }
        else
        {
            Console.WriteLine("  Nothing yet. Dew forms after 30+ minutes away.");
            Console.WriteLine();
            Console.WriteLine("  [1] ⏱  Time Travel (+8h)   [2] 🌙 Tuck In");
        }

        if (gathered)
            Console.WriteLine("  [3] ☀️  Jump to Dawn: settle & mint Frost");

        if (last != null && last.Stage != BlockStage.Dew)
        {
            Console.WriteLine($"  ── Block #{last.Height} · {last.Stage.ToToken()} ──");
            Console.WriteLine("  [4] 🏮 Light the Lantern");
            if (app.Proof != null)
                Console.WriteLine($"  {(app.Proof.Ok ? "✓ Verified" : "✗ Failed")} · Merkle depth {app.Proof.Depth} · root {app.Proof.Root[..Math.Min(16, app.Proof.Root.Length)]}…");
        }

        Console.WriteLine();
        var key = Console.ReadKey(true).KeyChar;
        switch (key)
        {
            case '1':
                if (dew != null) { app.Api.ClaimBlock(dew.Hash, Guid.NewGuid().ToString()).Wait(); }
                else { app.Api.Dev.Advance(8).Wait(); }
                break;
            case '2': app.Api.TuckIn().Wait(); break;
            case '3': app.Api.SettleDawn().Wait(); break;
            case '4':
                if (last != null) app.Proof = app.Api.Lantern(last.Hash).Result;
                break;
        }
    }
}
