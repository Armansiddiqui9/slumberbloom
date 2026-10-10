using System.Security.Cryptography;
using System.Text.Json;
using Slumberbloom.Contracts;
using Slumberbloom.Engine;
using Slumberbloom.AI;

namespace Slumberbloom.Backend;

// ---- persistence ----------------------------------------------------------------

/// <summary>
/// Persisted state — the on-device "server DB". Mirrors packages/adapters-local/src/index.ts const S.
/// Kept as a separate record so blocks are stored in their canonical wire form (hash excluded).
/// </summary>
internal sealed record StoredBlock
{
    public int Height { get; init; }
    public string Prev { get; init; } = "genesis";
    public long Start { get; init; }
    public long End { get; init; }
    public long Ms { get; init; }
    public string Stage { get; init; } = "dew";

    public string HashOf() => Hash.H(CanonicalJson.CanonOf(this));
}

internal sealed record PersistedState
{
    public long Offset { get; set; }
    public long? LastSeen { get; set; }
    public long? LastWater { get; set; }
    public double Growth { get; set; } = 10;
    public int Sun { get; set; } = 3;
    public int Moon { get; set; } = 3;
    public int Bloom { get; set; }
    public List<StoredBlock> Blocks { get; set; } = new();

    public static PersistedState Fresh() => new() { Offset = 0, Growth = 10, Sun = 3, Moon = 3, Bloom = 0 };
}

/// <summary>Minimal key/value store port. Swap for Android Preferences / IndexedDB later.</summary>
internal interface IBlobStore
{
    string? Read(string key);
    void Write(string key, string value);
}

internal sealed class MemoryBlobStore : IBlobStore
{
    private readonly Dictionary<string, string> _map = new();
    public string? Read(string key) => _map.TryGetValue(key, out var v) ? v : null;
    public void Write(string key, string value) => _map[key] = value;
}

// ---- the in-device server --------------------------------------------------------

/// <summary>
/// LocalApi — the in-device "server". Mirrors packages/adapters-local/src/index.ts createLocalApi().
/// Deterministic: every rule (dust, caps, accrual, merkle) lives in @sb/engine and here.
/// The UI calls ONLY IGameApi; swapping to HttpApi later changes nothing else.
/// </summary>
public sealed class LocalApi : IGameApi
{
    private const string Key = "sb.v1";
    private const long HourMs = 3_600_000L;

    private readonly IBlobStore _store;
    private readonly PersistedState _s;

    public LocalApi(IBlobStore? store = null, IAiAgent? ai = null)
    {
        _store = store ?? new MemoryBlobStore();
        _s = Load();
        _s.LastWater ??= Now();
        Dev = new DevApi(this);
        Ai = ai; // optional — UI never touches directly; exposed behind IGameApi only
    }

    private PersistedState Load()
    {
        try
        {
            var raw = _store.Read(Key);
            if (raw != null)
            {
                var p = JsonSerializer.Deserialize<PersistedState>(raw);
                if (p != null) return p;
            }
        }
        catch { /* corrupted state → fresh, same as web's try/catch */ }
        return PersistedState.Fresh();
    }

    private Task SaveAsync()
    {
        _store.Write(Key, JsonSerializer.Serialize(_s));
        return Task.CompletedTask;
    }

    /// <summary>Mirrors: const now = () => Date.now() + s.offset;</summary>
    public long Now() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + _s.Offset;

    /// <summary>Record the tuck-in timestamp. Mirrors tuckIn().</summary>
    public async Task TuckIn()
    {
        _s.LastSeen = Now();
        await SaveAsync();
    }

    /// <summary>
    /// Mint a Dew block if the player was away ≥ 30 min (cap 10 h).
    /// At most one open Dew at a time. Mirrors openBlock().
    /// </summary>
    public async Task<BlockView?> OpenBlock()
    {
        var open = _s.Blocks.FirstOrDefault(b => b.Stage == "dew");
        if (open != null) return ToView(open);
        if (_s.LastSeen == null) return null;

        var prev = _s.Blocks.Count > 0 ? ToEngineBlock(_s.Blocks[^1]) : null;
        var minted = NightBlock.MintNightBlock(prev, _s.LastSeen!.Value, Now());
        if (minted == null) return null;

        var stored = new StoredBlock
        {
            Height = minted.Height,
            Prev = minted.Prev,
            Start = minted.Start,
            End = minted.End,
            Ms = minted.Ms,
        };
        // Sanity: the stored block must hash to exactly what the engine minted.
        if (stored.HashOf() != minted.Hash)
            throw new InvalidOperationException("Block hash mismatch — canonical JSON diverged from web.");

        _s.Blocks.Add(stored);
        _s.LastSeen = null;
        await SaveAsync();
        return ToView(stored);
    }

    /// <summary>Dew → Gathered. Idempotent on stage, keyed by hash. Mirrors claimBlock().</summary>
    public async Task<BlockView> ClaimBlock(string id, string idemKey)
    {
        var b = _s.Blocks.FirstOrDefault(x => x.HashOf() == id)
            ?? throw new InvalidOperationException($"Block {id} not found");
        if (b.Stage == "dew")
        {
            b.Stage = "gathered";
            await SaveAsync();
        }
        return ToView(b);
    }

    /// <summary>
    /// The Dawn: every Gathered block is sealed to Frost and pays Bloom
    /// (10 per full hour of patience). Mirrors settleDawn().
    /// </summary>
    public async Task<int> SettleDawn()
    {
        var n = 0;
        foreach (var b in _s.Blocks.Where(b => b.Stage == "gathered").ToList())
        {
            b.Stage = "frost";
            _s.Bloom += (int)Math.Round(b.Ms / (double)HourMs * 10, MidpointRounding.AwayFromZero);
            n++;
        }
        await SaveAsync();
        return n;
    }

    /// <summary>Garden snapshot: stage, wilt ladder, ledger. Mirrors getGarden().</summary>
    public async Task<Garden> GetGarden()
    {
        var hoursSinceWater = _s.LastWater == null ? 0 : (Now() - _s.LastWater!.Value) / (double)HourMs;
        var garden = new Garden
        {
            Growth = _s.Growth,
            Stage = Plant.StageOf(_s.Growth),
            Wilt = Plant.WiltOf(hoursSinceWater),
            Sun = _s.Sun,
            Moon = _s.Moon,
            Bloom = _s.Bloom,
        };
        return await Task.FromResult(garden);
    }

    /// <summary>Water: spend 1 Sun, +25 growth (era multiplier ×1 in Era 0), refresh wilt clock.</summary>
    public async Task<Garden> Water()
    {
        if (_s.Sun > 0)
        {
            _s.Sun--;
            _s.Growth = Plant.Water(_s.Growth);
            _s.LastWater = Now();
            await SaveAsync();
        }
        return await GetGarden();
    }

    public async Task<IReadOnlyList<BlockView>> GetBlocks()
        => await Task.FromResult<IReadOnlyList<BlockView>>(_s.Blocks.Select(ToView).ToList());

    /// <summary>
    /// Light the Lantern: verify a block's inclusion in the day's Almanac (Merkle root
    /// over all sealed blocks). Mirrors lantern().
    /// </summary>
    public async Task<Proof> Lantern(string blockHash)
    {
        var sealedHashes = _s.Blocks.Where(b => b.Stage != "dew").Select(b => b.HashOf()).ToList();
        var root = Merkle.MerkleRoot(sealedHashes);
        var idx = sealedHashes.IndexOf(blockHash);
        if (idx < 0)
            return await Task.FromResult(new Proof { Ok = false, Root = root, Hash = blockHash, Depth = 0 });

        var path = Merkle.MerklePath(sealedHashes, idx);
        return await Task.FromResult(new Proof
        {
            Ok = Merkle.VerifyPath(blockHash, path, root),
            Root = root,
            Hash = blockHash,
            Depth = path.Count,
        });
    }

    // Optional AI layer — never reached by UI directly (only via IGameApi.Ai).
    public IAiAgent? Ai { get; }

    // Dev-only property to expose advancement from UI
    public IDevApi Dev { get; }

    private sealed class DevApi : IDevApi
    {
        private readonly LocalApi _api;
        public DevApi(LocalApi api) { _api = api; }
        public Task Advance(double hours)
        {
            _api._s.Offset += (long)(hours * HourMs);
            return _api.SaveAsync();
        }
    }

    // ---- mapping between storage and engine/contract types -------------------------

    private static NightBlock ToEngineBlock(StoredBlock b) => new()
    {
        Height = b.Height,
        Prev = b.Prev,
        Start = b.Start,
        End = b.End,
        Ms = b.Ms,
        Hash = b.HashOf(),
    };

    private static BlockView ToView(StoredBlock b) => new()
    {
        Height = b.Height,
        Prev = b.Prev,
        Start = b.Start,
        End = b.End,
        Ms = b.Ms,
        Hash = b.HashOf(),
        Stage = BlockStageExt.FromToken(b.Stage),
    };
}
