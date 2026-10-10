using Slumberbloom.AI;

namespace Slumberbloom.Contracts;

/// <summary>Block lifecycle: Mist (away) → Dew → Gathered → Frost → Heartwood.</summary>
public enum BlockStage { Dew, Gathered, Frost, Heartwood }

public static class BlockStageExt
{
    public static string ToToken(this BlockStage s) => s switch
    {
        BlockStage.Dew => "dew",
        BlockStage.Gathered => "gathered",
        BlockStage.Frost => "frost",
        BlockStage.Heartwood => "heartwood",
        _ => "unknown",
    };

    public static BlockStage FromToken(string t) => t switch
    {
        "dew" => BlockStage.Dew,
        "gathered" => BlockStage.Gathered,
        "frost" => BlockStage.Frost,
        "heartwood" => BlockStage.Heartwood,
        _ => BlockStage.Dew,
    };
}

/// <summary>A NightBlock plus its current lifecycle stage. Mirrors BlockView in packages/contracts.</summary>
public sealed record BlockView
{
    public int Height { get; init; }
    public string Prev { get; init; } = "genesis";
    public long Start { get; init; }
    public long End { get; init; }
    public long Ms { get; init; }
    public string Hash { get; init; } = "";
    public BlockStage Stage { get; init; }
}

/// <summary>Garden state snapshot, as returned by getGarden(). Mirrors Garden in packages/contracts.</summary>
public sealed record Garden
{
    public double Growth { get; init; }
    public string Stage { get; init; } = "seed";
    public string Wilt { get; init; } = "healthy";
    public int Sun { get; init; }
    public int Moon { get; init; }
    public int Bloom { get; init; }
}

/// <summary>Lantern verification result. Mirrors Proof in packages/contracts.</summary>
public sealed record Proof
{
    public bool Ok { get; init; }
    public string Root { get; init; } = "";
    public string Hash { get; init; } = "";
    public int Depth { get; init; }
}

/// <summary>
/// The ONLY surface the UI may call. LocalApi now, HttpApi in v1.1.
/// Mirrors GameApi in packages/contracts/src/index.ts.
/// </summary>
public interface IGameApi
{
    long Now();
    Task TuckIn();
    Task<BlockView?> OpenBlock();
    Task<BlockView> ClaimBlock(string id, string idemKey);
    Task<int> SettleDawn();
    Task<Garden> GetGarden();
    Task<Garden> Water();
    Task<IReadOnlyList<BlockView>> GetBlocks();
    Task<Proof> Lantern(string blockHash);

    /// <summary>Optional AI layer — UI never touches this directly; wired through LocalApi.</summary>
    IAiAgent? Ai { get; }

    IDevApi Dev { get; }
}

public interface IDevApi
{
    Task Advance(double hours);
}
