using System.Collections.Generic;
using System.Linq;
using Slumberbloom.Engine.Crypto;
using Slumberbloom.Engine.Encoding;

namespace Slumberbloom.Engine;

/// <summary>
/// RFC 6962 Merkle tree: leaf=0x00, node=0x01 domain separation.
/// Mirrors packages/engine/src/crypto/merkle.ts.
/// </summary>
public static class Merkle
{
    // Leaf: \x00 + data
    private static string Leaf(string data) => Hash.H("\x00" + data);
    // Node: \x01 + left + right
    private static string Node(string left, string right) => Hash.H("\x01" + left + right);

    /// <summary>Merkle root of items list.</summary>
    public static string MerkleRoot(IReadOnlyList<string> items)
    {
        if (items.Count == 0) return Hash.H("");
        var lv = items.Select(Leaf).ToList();
        while (lv.Count > 1)
        {
            var n = new List<string>();
            for (int i = 0; i < lv.Count; i += 2)
            {
                if (i + 1 < lv.Count)
                    n.Add(Node(lv[i], lv[i + 1]));
                else
                    n.Add(lv[i]);
            }
            lv = n;
        }
        return lv[0];
    }

    /// <summary>Merkle path for item at index idx.</summary>
    public static List<MerkleStep> MerklePath(IReadOnlyList<string> items, int idx)
    {
        var path = new List<MerkleStep>();
        var levels = Levels(items);
        for (int l = 0; l < levels.Count - 1; l++)
        {
            var lv = levels[l];
            var s = idx ^ 1;
            if (s < lv.Count)
                path.Add(new MerkleStep(lv[s], s < idx));
            idx >>= 1;
        }
        return path;
    }

    /// <summary>Verify a Merkle path.</summary>
    public static bool VerifyPath(string item, List<MerkleStep> path, string root)
    {
        var h = Leaf(item);
        foreach (var step in path)
        {
            h = step.Left ? Node(step.Sib, h) : Node(h, step.Sib);
        }
        return h == root;
    }

    private static List<List<string>> Levels(IReadOnlyList<string> items)
    {
        var lv = items.Select(Leaf).ToList();
        var all = new List<List<string>> { new(lv) };
        while (lv.Count > 1)
        {
            var n = new List<string>();
            for (int i = 0; i < lv.Count; i += 2)
            {
                if (i + 1 < lv.Count)
                    n.Add(Node(lv[i], lv[i + 1]));
                else
                    n.Add(lv[i]);
            }
            lv = n;
            all.Add(new(lv));
        }
        return all;
    }
}

public sealed record MerkleStep(string Sib, bool Left);
