import { it, expect } from "vitest"; import { merkleRoot, merklePath, verifyPath, mintNightBlock, canon } from "../src";
it("merkle proofs verify", () => { const x = ["a", "b", "c", "d", "e"]; const r = merkleRoot(x); x.forEach((v, i) => expect(verifyPath(v, merklePath(x, i), r)).toBe(true)); });
it("canonical json is key-order independent", () => expect(canon({ b: 1, a: 2 })).toBe(canon({ a: 2, b: 1 })));
it("dust rule + 10h cap", () => { expect(mintNightBlock(null, 0, 20 * 60e3)).toBeNull(); expect(mintNightBlock(null, 0, 20 * 3600e3)!.ms).toBe(10 * 3600e3); });
