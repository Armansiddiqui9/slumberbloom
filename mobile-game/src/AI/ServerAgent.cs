using System.Diagnostics;
using System.Net.Http.Json;

namespace Slumberbloom.AI;

/// <summary>
/// ServerAgent — graceful remote fallback when on-device is unavailable.
/// Targets packages/ai FastAPI endpoint (http://localhost:8000/complete by convention).
/// </summary>
public sealed class ServerAgent : IAiAgent
{
    private static readonly HttpClient _client = new()
    {
        Timeout = TimeSpan.FromSeconds(15),
    };

    private readonly string _url;

    public ServerAgent(string url = "http://localhost:8000/complete")
    {
        _url = url;
    }

    public async Task<string> Complete(string prompt)
    {
        // Deterministic-friendly: fixed temperature passed explicitly
        try
        {
            var payload = new { prompt, temperature = 0.7, seed = 42 };
            var response = await _client.PostAsJsonAsync(_url, payload);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return result.Length > 2 && result[0] == '"' ? result.Trim('"') : result;
        }
        catch
        {
            return "[server fallback: unavailable]";
        }
    }

    public Task<bool> IsAvailable()
    {
        // Conservative: assume server is up unless we have evidence otherwise.
        return Task.FromResult(true);
    }

    public void Unload() { /* nothing to unload — remote */ }
}
