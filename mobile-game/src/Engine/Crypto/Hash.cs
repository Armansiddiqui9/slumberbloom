using System.Security.Cryptography;
using System.Text;

namespace Slumberbloom.Engine;

/// <summary>SHA-256 over UTF-8 bytes, hex-encoded. Same as packages/engine/src/crypto/hash.ts.</summary>
public static class Hash
{
    public static string H(string s)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(s));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}