import type { NightBlock } from "@sb/engine";
export type BlockStage = "dew" | "gathered" | "frost" | "heartwood";
export type BlockView = NightBlock & { stage: BlockStage };
export type Garden = { growth: number; stage: string; wilt: string; sun: number; moon: number; bloom: number };
export type Proof = { ok: boolean; root: string; hash: string; depth: number };
/** The ONLY surface the UI may call. LocalApi now, HttpApi in v1.1. */
export interface GameApi {
  now(): number;
  tuckIn(): Promise<void>;
  openBlock(): Promise<BlockView | null>;
  claimBlock(id: string, idemKey: string): Promise<BlockView>;
  settleDawn(): Promise<number>;
  getGarden(): Promise<Garden>;
  water(): Promise<Garden>;
  getBlocks(): Promise<BlockView[]>;
  lantern(blockHash: string): Promise<Proof>;
  dev: { advance(hours: number): Promise<void> };
}
