using Slumberbloom.Contracts;

namespace Slumberbloom.UI;

/// <summary>
/// Bedtime screen — "Tuck in, and be patient."
/// Mirrors apps/web/src/app/App.tsx tab === "bed".
/// </summary>
public static class BedtimeScreen
{
    public static void Render(AppModel app)
    {
        Console.WriteLine("  ╭──────────────────────────────────────────────╮");
        Console.WriteLine("  │   🌙  Nocturnal Vigil                        │");
        Console.WriteLine("  ╰──────────────────────────────────────────────╯");
        Console.WriteLine();
        Console.WriteLine(app.Away
            ? "  Sleep well. Your garden is mining."
            : "  Tuck in, and be patient.");
        Console.WriteLine();
        Console.WriteLine("  Zero tracking · zero microphone · nothing runs");
        Console.WriteLine("  while you're away. Tuck in, slide +8h, then");
        Console.WriteLine("  open Morning Dew.");
        Console.WriteLine();
        Console.WriteLine("  [1] 🌙 Tuck In Phone & Begin Slumber");
        Console.WriteLine("  [2] ⏱  Time Travel (+8h)   [3] ⏱  (+24h)");
        Console.WriteLine();

        var key = Console.ReadKey(true).KeyChar;
        switch (key)
        {
            case '1':
                app.Api.TuckIn().Wait();
                app.Away = true;
                break;
            case '2': app.Api.Dev.Advance(8).Wait(); break;
            case '3': app.Api.Dev.Advance(24).Wait(); break;
        }
    }
}
