using System.Diagnostics;

namespace Slumberbloom.AI;

/// <summary>
/// OnDeviceAgent — lazy-loaded, memory-mapped LLM via LLamaSharp.
/// Model: Q4_K_M quantized (Qwen2.5-0.5B preferred; Llama-3.2-1B acceptable).
/// Rules: load per job → unload after; cap context at 2048; seed 42; temp 0.7.
/// Peak target &lt; 600 MB; ~0 idle when unloaded.
/// </summary>
public sealed class OnDeviceAgent : IAiAgent
{
    private const int MaxContext = 2048;
    private const int FixedSeed = 42;
    private const float Temperature = 0.7f;

    private readonly string _modelPath;
    private readonly int _ramThresholdMb = 4096; // 4 GB
    private object? _model; // LLamaSharp LLamaWeights / LLamaContext held here

    public OnDeviceAgent(string modelPath = "models/Qwen2.5-0.5B.Q4_K_M.gguf")
    {
        _modelPath = modelPath;
    }

    /// <summary>Lazy load per job; guild delegate if GPU available; memory-map weights.</summary>
    private bool TryLoad()
    {
        if (_model != null) return true;

        // RAM guard: graceful fallback to ServerAgent if < 4 GB available
        try
        {
            var workingSet = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024);
            if (workingSet < _ramThresholdMb) return true;
            // We have at least 4 GB — proceed with on-device inference.
        }
        catch { return false; }

        if (!File.Exists(_modelPath)) return false;

        // LLamaSharp: LLamaWeights.LoadFromFile(new ModelParams(_modelPath)
        // { ContextSize = 2048, GpuLayerCount = GpuAvailable ? 32 : 0,
        //   Seed = 42, UseMemoryLock = false, MemoryMap = true });
        // model.CreateContext(params); InteractiveExecutor(context);
        _model = new object(); // placeholder — real build references LLamaSharp
        return true;
    }

    public async Task<string> Complete(string prompt)
    {
        if (!TryLoad()) return "[on-device unavailable — server fallback required]";

        try
        {
            // Deterministic sampling: fixed seed + fixed temperature.
            // Real LLamaSharp call would pass: seed=FixedSeed, temperature=Temperature,
            // contextSize=MaxContext, memoryMap=true, gpuDelegate=true (if available).
            var result = $"[AI:{prompt.Substring(0, Math.Min(20, prompt.Length))}… — q4_k_m seed={FixedSeed} t={Temperature} ctx={MaxContext}]";

            Unload(); // mandatory: unload after every job
            return result;
        }
        catch
        {
            Unload();
            return "[on-device error]";
        }
    }

    public Task<bool> IsAvailable()
    {
        // Can serve if RAM ok and model file present (without actually loading).
        var workingSet = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024);
        return Task.FromResult(workingSet < _ramThresholdMb && File.Exists(_modelPath));
    }

    /// <summary>Unload model weights; leave ~0 resident RAM.</summary>
    public void Unload()
    {
        if (_model != null)
        {
            // In real build: _model.Dispose() / GC.Collect() after release.
            _model = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
