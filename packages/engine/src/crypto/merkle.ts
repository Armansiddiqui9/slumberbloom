import { H } from "./hash";
// RFC 6962 domain separation: 0x00 leaf, 0x01 node
const leaf = (d: string) => H("\x00" + d), node = (a: string, b: string) => H("\x01" + a + b);
export type Step = { sib: string; left: boolean };
function levels(items: string[]) {
  let lv = items.map(leaf); const all = [lv];
  while (lv.length > 1) { const n: string[] = []; for (let i = 0; i < lv.length; i += 2) n.push(i + 1 < lv.length ? node(lv[i], lv[i + 1]) : lv[i]); lv = n; all.push(lv); }
  return all;
}
export const merkleRoot = (items: string[]) => (items.length ? levels(items).at(-1)![0] : H(""));
export function merklePath(items: string[], idx: number): Step[] {
  const path: Step[] = []; let i = idx;
  for (const lv of levels(items).slice(0, -1)) { const s = i ^ 1; if (s < lv.length) path.push({ sib: lv[s], left: s < i }); i >>= 1; }
  return path;
}
export const verifyPath = (item: string, path: Step[], root: string) =>
  path.reduce((h, s) => (s.left ? node(s.sib, h) : node(h, s.sib)), leaf(item)) === root;
