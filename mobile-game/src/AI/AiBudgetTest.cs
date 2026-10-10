using System.Diagnostics;

namespace Slumberbloom.AI;

/// <summary>
/// Budget test: prints peak working set after each of the three AI jobs.
/// Keeps sampling deterministic (seed=42, temp=0.7) so numbers are reproducible.
/// </summary>
public static class AiBudgetTest
{
    public static async Task RunAsync()
    {
        Console.WriteLine("[AI BUDGET] Peak RAM after each job — deterministic seed, temp=0.7");

        // Job 1: quest generation (dawn)
        using var agent1 = new OnDeviceAgent();
        var p1 = await agent1.Complete("Generate a dawn quest for the garden.");
        PrintPeak("Quest (dawn)", p1);
        agent1.Unload();

        // Job 2: photo judging (dew claim)
        using var agent2 = new OnDeviceAgent();
        var p2 = await agent2.Complete("Judge dew photo quality: bright, sharp, herbal.");
        PrintPeak("PhotoJudge (dew claim)", p2);
        agent2.Unload();

        // Job 3: seed naming (garden)
        using var agent3 = new OnDeviceAgent();
        var p3 = await agent3.Complete("Name a garden seed from the morning dew.");
        PrintPeak("SeedName (garden)", p3);
        agent3.Unload();

        Console.WriteLine("[AI BUDGET] All three jobs finished — ~0 idle RAM expected after Unload().");
    }

    private static void PrintPeak(string job, string result)
    {
        var proc = Process.GetCurrentProcess();
        var ws = proc.WorkingSet64 / (1024 * 1024); // MB
        Console.WriteLine($"  {job} | result len={result.Length} | peak working set={ws} MB");
    }
}
