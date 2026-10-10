namespace Slumberbloom.AI;

/// <summary>
/// The ONLY surface callers may use. OnDeviceAgent now; ServerAgent when RAM &lt; 4 GB
/// or the model is missing. UI code never implements or holds this — it goes through
/// IGameApi (LocalApi) only.
/// </summary>
public interface IAiAgent
{
    /// <summary>Run a completion job (quest text, photo verdict, seed name).</summary>
    Task<string> Complete(string prompt);

    /// <summary>True when the on-device path can serve a job right now.</summary>
    Task<bool> IsAvailable();

    /// <summary>Release model weights. Idempotent; leaves ~0 resident RAM.</summary>
    void Unload();
}

/// <summary>The three — and only three — jobs the game needs from an agent.</summary>
public enum AiJob { Quest, PhotoJudge, SeedName }
