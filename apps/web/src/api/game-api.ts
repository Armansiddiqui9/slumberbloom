// Composition root: the ONLY file allowed to import an adapter. v1.1 swaps this for HttpApi.
import { createLocalApi } from "@sb/adapters-local"; import type { GameApi } from "@sb/contracts";
export const api: GameApi = createLocalApi();
