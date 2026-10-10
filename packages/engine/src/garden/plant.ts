export const stageOf = (g: number) => (g >= 100 ? "bloom" : g >= 60 ? "bud" : g >= 25 ? "sprout" : "seed");
/** Wilt ladder (no permanent death): healthy, droop 24h, brown 48h, dormant 120h */
export const wiltOf = (h: number) => (h >= 120 ? "dormant" : h >= 48 ? "brown" : h >= 24 ? "droop" : "healthy");
export const water = (g: number, eraMult = 1) => Math.min(100, g + 25 * eraMult);
