using Slumberbloom.Contracts;

namespace Slumberbloom.UI;

/// <summary>
/// Garden — the plant, growth bar, ledger.
/// Mirrors apps/web/src/app/App.tsx tab === "garden".
/// </summary>
public static class GardenScreen
{
    private static readonly Dictionary<string, string> Emoji = new()
    {
        ["seed"] = "🌰", ["sprout"] = "🌱", ["bud"] = "🌿", ["bloom"] = "🌻",
    };

    public static void Render(AppModel app)
    {
        var g = app.Garden;
        if (g == null) return;

        Console.WriteLine("  ╭──────────────────────────────────────────────╮");
        Console.WriteLine("  │   🪴  Garden                                 │");
        Console.WriteLine("  ╰──────────────────────────────────────────────╯");
        Console.WriteLine();
        Console.WriteLine("  Ipomoea noctiflora — Baby Moonflower");
        Console.WriteLine();
        Console.WriteLine($"        {Emoji.GetValueOrDefault(g.Stage, "🌰")}");
        Console.WriteLine();
        Console.WriteLine($"  [{new string('█', (int)(g.Growth / 5))}{new string('░', 20 - (int)(g.Growth / 5))}]");
        Console.WriteLine($"  {g.Stage} · {g.wilt} · {g.Growth}/100");
        Console.WriteLine();
        Console.WriteLine(g.Sun > 0
            ? "  [1] 💧 Water (1 ☀️)"
            : "  💧 Water (1 ☀️) — no Sun left today");
        Console.WriteLine();
        Console.WriteLine("  ╭─ Ledger ────────────────────────────────────╮");
        Console.WriteLine($"  │   {g.Bloom} Bloom                                │");
        Console.WriteLine($"  │   ☀️ {g.Sun} Sun · 🌙 {g.Moon} Moon                          │");
        Console.WriteLine("  ╰──────────────────────────────────────────────╯");
        Console.WriteLine();
        Console.WriteLine("  Plants never die. After 120h unwatered they rest in dormancy.");

        var key = Console.ReadKey(true).KeyChar;
        if (key == '1') app.Api.Water().Wait();
    }
}
