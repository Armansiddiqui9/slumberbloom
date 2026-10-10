using Slumberbloom.Contracts;

namespace Slumberbloom.UI;

/// <summary>
/// Census — "The ceiling falls out of the Charter."
/// Mirrors apps/web/src/app/App.tsx tab === "census".
/// </summary>
public static class CensusScreen
{
    public static void Render(AppModel app)
    {
        var blocks = app.Blocks;
        var frost = blocks.Count(b => b.Stage == BlockStage.Frost);

        Console.WriteLine("  ╭──────────────────────────────────────────────╮");
        Console.WriteLine("  │   🧭  Census                                 │");
        Console.WriteLine("  ╰──────────────────────────────────────────────╯");
        Console.WriteLine();
        Console.WriteLine("  The ceiling falls out of the Charter.");
        Console.WriteLine();
        Console.WriteLine("  ╭─ The World ─────────────────────────────────╮");
        Console.WriteLine($"  │   Night blocks : {blocks.Count,-28}│");
        Console.WriteLine($"  │   Frost sealed : {frost,-28}│");
        Console.WriteLine("  ╰──────────────────────────────────────────────╯");
        Console.WriteLine();
        Console.WriteLine("  World Simulator, Charter Lab and era engine");
        Console.WriteLine("  come next (docs/ARCHITECTURE.md §7).");
        Console.WriteLine();
        Console.WriteLine("  Press any key…");
        Console.ReadKey(true);
    }
}
