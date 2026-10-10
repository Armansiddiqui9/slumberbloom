using Slumberbloom.Contracts;

namespace Slumberbloom.UI;

/// <summary>
/// The Sowing of Quietude — the full onboarding ritual from design/mockups/onboarding.html.
/// Step 1: the Botanical Covenant (3 principles)
/// Step 2: choose your Genesis Seed (3 seeds)
/// Step 3: circadian cadence (tuck-in / wake windows)
/// Step 4: "Sow First Seed & Enter Sanctuary" → tuck-in + garden
/// Mirrors apps/web onboarding.html.
/// </summary>
public sealed class OnboardingScreen
{
    public const string Moonflower = "moonflower";
    public const string Fernlet = "fernlet";
    public const string MossPebble = "moss";

    public static IReadOnlyList<SeedCard> Seeds { get; } = new[]
    {
        new SeedCard(Moonflower, "Baby Moonflower", "Ipomoea Somnifera",
            "Tender nocturnal bloom. Thrives during undisturbed night hours. Radiates faint bioluminescent silver upon maturity.",
            "🌙 Deep Sleeper", "Archaic Mythic", "Folio #01"),
        new SeedCard(Fernlet, "Dewy Fernlet", "Polypodium Aurum",
            "Morning dawn companion. Collects the first golden amber rays of sunrise while filtering wakeful morning mindfulness.",
            "🌅 Dawn Riser", "Heirloom", "Folio #02"),
        new SeedCard(MossPebble, "Mossy Pebble", "Bryophyta Dormiens",
            "The gentle, patient snooze companion. Requires zero strict horological cadence; blossoms purely through spontaneous restorative rest.",
            "🛏 Free Sleeper", "Ancient Field", "Folio #03"),
    };

    /// <summary>The 3 covenant principles — zero surveillance, no guilt, crystallized dew.</summary>
    public static IReadOnlyList<CovenantPrinciple> Covenant { get; } = new[]
    {
        new CovenantPrinciple("Zero Surveillance",
            "No acoustic monitors, biometric telemetry, or algorithmic intrusion. Your private night remains pristine."),
        new CovenantPrinciple("No Guilt or Wilting",
            "Time spent away simply invites peaceful dormancy. You will never encounter punitive decay or lost streaks."),
        new CovenantPrinciple("Crystallized Dew",
            "Your unperturbed nocturnal rest transmutes directly into cryptobotanical seeds and archival collector flora."),
    };

    /// <summary>Runs the onboarding flow in the console (placeholder for MAUI/Unity UI).
    /// Returns the chosen seed id and performs the tuck-in.</summary>
    public static async Task<string> RunAsync(IGameApi api)
    {
        Console.WriteLine();
        Console.WriteLine("  ╭──────────────────────────────────────────────╮");
        Console.WriteLine("  │   🌱  The Sowing of Quietude                 │");
        Console.WriteLine("  │      Ritual 01 · Genesis Herbarium           │");
        Console.WriteLine("  ╰──────────────────────────────────────────────╯");
        Console.WriteLine();
        Console.WriteLine("  Welcome to SlumberBloom: an archival botanical");
        Console.WriteLine("  sanctuary shielded from the frantic velocity of");
        Console.WriteLine("  the attention economy.");
        Console.WriteLine();
        Console.WriteLine("  ── The Botanical Covenant ──────────────────────");
        foreach (var p in Covenant)
            Console.WriteLine($"   • {p.Name}: {p.Description}");
        Console.WriteLine();

        Console.WriteLine("  ── Choose Your Genesis Seed ────────────────────");
        for (var i = 0; i < Seeds.Count; i++)
        {
            var s = Seeds[i];
            Console.WriteLine($"   [{i + 1}] {s.Name} · {s.Latin}");
            Console.WriteLine($"       {s.Badge} · Rarity: {s.Rarity}");
        }
        Console.WriteLine();

        var choice = Moonflower;
        Console.Write("  Select a seed (1-3, Enter = Moonflower): ");
        var line = Console.ReadLine()?.Trim();
        if (int.TryParse(line, out var n) && n >= 1 && n <= 3)
            choice = Seeds[n - 1].Id;

        Console.WriteLine();
        Console.WriteLine("  ── Circadian Cadence Haven ─────────────────────");
        Console.WriteLine("   Tuck-in window : 10:30 PM · Candlelight dusk");
        Console.WriteLine("   Awaken call    : 07:00 AM · Morning dew collect");
        Console.WriteLine("   Sanctuary      : 8h 30m of Undisturbed Rest");
        Console.WriteLine();

        Console.WriteLine("  ⟡ Sow First Seed & Enter Sanctuary…");
        await api.TuckIn();
        Console.WriteLine("  ✓ Sanctuary Sealed · Rest Gently");
        Console.WriteLine("    100% Private · Stored on device · Genesis Block #0001");
        Console.WriteLine();

        return choice;
    }
}

public sealed record SeedCard(
    string Id, string Name, string Latin, string Description,
    string Badge, string Rarity, string Folio);

public sealed record CovenantPrinciple(string Name, string Description);
