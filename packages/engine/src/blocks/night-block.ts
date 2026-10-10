import { H } from "../crypto/hash"; import { canon } from "../encoding/canonical-json";
export const MIN_MS = 30 * 60e3, MAX_MS = 10 * 3600e3;
export type NightBlock = { height: number; prev: string; start: number; end: number; ms: number; hash: string };
/** Dust rule: <30 min mints nothing. Interval cap: 10 h. Pure: explicit inputs only. */
export function mintNightBlock(prev: NightBlock | null, lastSeen: number, now: number): NightBlock | null {
  const away = now - lastSeen; if (away < MIN_MS) return null;
  const b = { height: (prev?.height ?? 0) + 1, prev: prev?.hash ?? "genesis", start: lastSeen, end: now, ms: Math.min(away, MAX_MS) };
  return { ...b, hash: H(canon(b)) };
}
