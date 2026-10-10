using System.Text.Json;
using System.Text.Json.Nodes;

namespace Slumberbloom.Engine;

/// <summary>
/// Canonical JSON: sorted keys, no whitespace — identical output to
/// packages/engine/src/encoding/canonical-json.ts (and the web's canon()).
/// Numbers serialize as the shortest form that round-trips (JS Number semantics),
/// so hashes match the web byte-for-byte.
/// </summary>
public static class CanonicalJson
{
    public static string Canon(JsonNode v) => v switch
    {
        JsonArray a => "[" + string.Join(",", a.Select(Canon)) + "]",
        JsonObject o => "{" + string.Join(",", o
            .OrderBy(p => p.Key, StringComparer.Ordinal)
            .Select(p => JsonEncodedText.Encode(p.Key) + ":" + Canon(p.Value))) + "}",
        JsonValue jv => CanonValue(jv),
        null => "null",
    };

    private static string CanonValue(JsonValue jv)
    {
        // Try to recover the exact JS number rendering: integers as plain ints,
        // fractions in shortest round-trippable form.
        if (jv.TryGetValue<long>(out var l)) return l.ToString();
        if (jv.TryGetValue<double>(out var d))
        {
            // "R" round-trips; trim the exponent form JS never emits.
            var s = d.ToString("R");
            // JS JSON.stringify never emits exponent notation; normalize C# forms
            // (e.g. "1E-06", "1.5E+10") back to plain decimal so hashes match.
            var e = s.IndexOfAny(new[] { 'e', 'E' });
            if (e > 0)
            {
                var exp = int.Parse(s[(e + 1)..]);
                var mantissa = s[..e].Replace(".", "");
                var neg = mantissa.StartsWith('-');
                if (neg) mantissa = mantissa[1..];
                var dot = mantissa.Length > 1 ? 1 : 0;
                if (exp >= 0)
                {
                    s = mantissa.PadRight(dot + exp + 1, '0');
                }
                else
                {
                    s = "0." + new string('0', -exp - 1) + mantissa;
                }
                if (neg) s = "-" + s;
                // strip trailing zeros / trailing dot, keep at least one digit
                if (s.Contains('.'))
                {
                    s = s.TrimEnd('0').TrimEnd('.');
                    if (s.Length == 0 || s == "-") s = "0";
                }
            }
            return s;
        }
        if (jv.TryGetValue<bool>(out var b)) return b ? "true" : "false";
        if (jv.TryGetValue<string>(out var s)) return JsonEncodedText.Encode(s);
        return jv.ToJsonString();
    }

    /// <summary>Canon a plain object (round-trips through System.Text.Json).</summary>
    public static string CanonOf(object v) => Canon(JsonNode.Parse(JsonSerializer.Serialize(v))!);
}
