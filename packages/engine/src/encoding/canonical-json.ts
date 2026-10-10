export function canon(v: unknown): string {
  if (Array.isArray(v)) return "[" + v.map(canon).join(",") + "]";
  if (v && typeof v === "object") return "{" + Object.keys(v as object).sort().map(k => JSON.stringify(k) + ":" + canon((v as any)[k])).join(",") + "}";
  return JSON.stringify(v);
}
