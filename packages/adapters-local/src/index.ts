import { mintNightBlock, merkleRoot, merklePath, verifyPath, stageOf, wiltOf, water as grow, type NightBlock } from "@sb/engine";
import type { GameApi, BlockView, Garden } from "@sb/contracts";
const KEY = "sb.v1";
type S = { offset: number; lastSeen: number | null; lastWater: number; growth: number; sun: number; moon: number; bloom: number; blocks: BlockView[] };
const fresh = (): S => ({ offset: 0, lastSeen: null, lastWater: 0, growth: 10, sun: 3, moon: 3, bloom: 0, blocks: [] });
export function createLocalApi(): GameApi {
  let s: S; try { s = { ...fresh(), ...JSON.parse(localStorage.getItem(KEY) || "{}") }; } catch { s = fresh(); }
  const save = () => localStorage.setItem(KEY, JSON.stringify(s));
  const now = () => Date.now() + s.offset;
  if (!s.lastWater) s.lastWater = now();
  const garden = (): Garden => ({ growth: s.growth, stage: stageOf(s.growth), wilt: wiltOf((now() - s.lastWater) / 3.6e6), sun: s.sun, moon: s.moon, bloom: s.bloom });
  const sealed = () => s.blocks.filter(b => b.stage !== "dew").map(b => b.hash);
  return {
    now,
    async tuckIn() { s.lastSeen = now(); save(); },
    async openBlock() {
      const open = s.blocks.find(b => b.stage === "dew"); if (open || s.lastSeen == null) return open ?? null;
      const b = mintNightBlock((s.blocks.at(-1) as NightBlock) ?? null, s.lastSeen, now()); if (!b) return null;
      const v: BlockView = { ...b, stage: "dew" }; s.blocks.push(v); s.lastSeen = null; save(); return v;
    },
    async claimBlock(id) { const b = s.blocks.find(x => x.hash === id)!; if (b.stage === "dew") b.stage = "gathered"; save(); return b; },
    async settleDawn() { let n = 0; for (const b of s.blocks) if (b.stage === "gathered") { b.stage = "frost"; s.bloom += Math.round(b.ms / 3.6e6 * 10); n++; } save(); return n; },
    async getGarden() { return garden(); },
    async water() { if (s.sun > 0) { s.sun--; s.growth = grow(s.growth); s.lastWater = now(); save(); } return garden(); },
    async getBlocks() { return [...s.blocks]; },
    async lantern(h) {
      const l = sealed(), i = l.indexOf(h), r = merkleRoot(l);
      if (i < 0) return { ok: false, root: r, hash: h, depth: 0 };
      const p = merklePath(l, i); return { ok: verifyPath(h, p, r), root: r, hash: h, depth: p.length };
    },
    dev: { async advance(hours) { s.offset += hours * 3.6e6; save(); } },
  };
}
