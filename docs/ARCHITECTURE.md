# Slumberbloom v1 — Build Architecture & File Structure

> Production architecture, hackathon adapters. Last updated: 2026-10-08
>
> The hackathon version runs the **production server code inside the browser**, behind the **same API contract**. Production then only swaps the adapters. The engine, the use-case code and the UI stay untouched.

---

## 1. Guiding rules

1. **The engine is pure and shared.** It does no I/O, never reads `Date.now()`, uses no floats in the consensus path, and takes explicit inputs `(state, serverNow, charter)`.
2. **The UI never touches game state or time directly.** It only calls a `GameApi` interface. The hackathon implements it as `LocalApi` (an in-browser "server"). v1.1 implements it as `HttpApi` (`fetch`).
3. **Everything non-deterministic is a port.** Time, randomness, signing, storage, anchoring, world population, attestation and the LLM each get a local adapter now and a real one later.

---

## 2. Architecture

```
┌──────────────────────────── apps/web (React PWA) ────────────────────────────┐
│ Garden · Away reveal · Dawn/Sky Draw · Lantern · Census · Charter Lab · Fossil│
│            │ calls only                        │ verifies with                │
│            ▼                                   ▼                              │
│        GameApi (interface from packages/contracts)      engine/verify (Lantern)│
└────────────┬───────────────────────────────────────────────────────────────────┘
             │  composition root picks one (VITE_BACKEND=local|http)
     ┌───────┴────────┐
 LocalApi (NOW)    HttpApi (v1.1) ──► apps/server (Hono + Postgres)
     │                                      │
     └────────────► packages/server-core ◄──┘     ← SAME use-case code both places
                    (presence, blocks, claim, dawn, garden, census, proofs)
                              │ uses
                       packages/engine  (pure, deterministic, golden-vector tested)
                              │
   PORTS: Clock · Beacon · Signer · Repo/EventLog · Anchor · WorldSource · Attestor · KillSwitch · QuestAI
   NOW:   LocalClock(+time travel) · SimBeacon · LocalSigner · IndexedDB/memory · StubAnchor · WorldSim · StubAttestor · local flags · Ollama+templates
   LATER: ServerClock · drand · Ed25519 key manifest · Postgres(append-only) · OpenTimestamps · Play Integrity/App Attest · real claims queue · remote flags · on-device LLM
```

### Design decisions

**Two separate stores, like production.** The "server DB" (event log, blocks, claims, ledger, mints, almanac) and the "client cache" (your own blocks, Merkle paths, Almanac heads, UI prefs) live in different namespaces. The client cache is a light client: the Lantern re-verifies everything from it and never trusts a server read. Use IndexedDB for the server DB (the World Simulator will generate years of rows); localStorage is fine for the client cache.

**The event log is the source of truth.** Heartbeat, BlockMinted, Claimed, DawnSettled and similar events are append-only, and projections are derived from them. This makes invariant B2.7 (replay reproduces every Almanac hash) testable from day one.

**Dawn settlement runs through a `Scheduler` port.** In the hackathon, whenever the clock crosses a UTC Dawn boundary (including via time travel), `catchUp()` runs `settle(d)` for every unsettled Dawn in order. In production, a cron worker calls the same function. Settlement is idempotent, so both behave the same (edge case T8).

**The World Simulator is a `WorldSource`.** `settle(d)` asks for the day's claims. In the hackathon that is your real local claim plus a seeded virtual population. In production it is the DB claims queue. Φ, the halving, the Sky Draw odds and the Soil Forecast therefore behave realistically in the demo.

**Sync SHA-256 in the engine.** Use `@noble/hashes` (or a vendored implementation) so settlement stays synchronous and deterministic. Web Crypto is async and would leak `await` into the consensus path; keep it in adapters only.

---

## 3. File structure

```
slumberbloom/                         # public repo (Part 1 as README.md)
├─ pnpm-workspace.yaml · tsconfig.base.json · .eslintrc (import boundaries)
├─ .gitignore                         # Private-readme.md, charter_sim.py, /private/  ← before first commit
├─ fixtures/golden/                   # shared vectors: canonical-json, merkle, fixed-ln, serials, skydraw
│
├─ packages/
│  ├─ engine/                         # PURE. depends on nothing (sync sha256 injected)
│  │  ├─ src/
│  │  │  ├─ encoding/   canonical-json.ts · bytes.ts · crockford32.ts
│  │  │  ├─ crypto/     hash.ts · merkle.ts (RFC 6962 0x00/0x01) · domain-tags.ts
│  │  │  ├─ math/       fixed.ts (Q64.64, ln) · prng.ts (H(beacon‖commit))
│  │  │  ├─ charter/    charter.ts (sealed params + hash) · ops.ts (tunable types) · sizing.ts (ceiling, S₀ worksheet)
│  │  │  ├─ blocks/     night-block.ts (interval, dust, caps, sunlight) · block-state.ts (Dew→Gathered→Frost→Heartwood/Held)
│  │  │  ├─ dawn/       settle.ts · skydraw.ts · bloom.ts · seed-accrual.ts (acc // D_MAX)
│  │  │  ├─ era/        era-engine.ts (gate, date, retarget, pre-announce, Last Dawns)
│  │  │  ├─ seeds/      serial.ts (SB-… mod-37 check) · rarity.ts · species.ts
│  │  │  ├─ garden/     plant.ts (growth, era multiplier) · wilt.ts · lifecycle.ts (Resting→Fossil)
│  │  │  ├─ ledger/     ledger.ts (double-entry, petals)
│  │  │  ├─ almanac/    seal.ts · proofs.ts · verify.ts   ← the Lantern lives here
│  │  │  └─ index.ts
│  │  └─ test/          invariants.property.test.ts (B2) · golden.test.ts · replay-5y.test.ts · fuzz-encoder.test.ts
│  │
│  ├─ contracts/                      # types only: shared by client + server
│  │  └─ src/  game-api.ts (the interface) · dto.ts (zod) · events.ts · errors.ts · idempotency.ts
│  │
│  ├─ server-core/                    # use cases. Runs in-browser now, in Node later
│  │  └─ src/
│  │     ├─ ports/      clock · beacon · signer · repo · anchor · world-source · attestor · scheduler · kill-switch
│  │     ├─ services/   presence.ts (heartbeat, single lease) · blocks.ts (open, idempotent claim)
│  │     │              dawn.ts (settle orchestration) · garden.ts (water, revive, quests)
│  │     │              census.ts · forecast.ts · proofs.ts
│  │     ├─ scheduler/  catch-up.ts
│  │     └─ api-handler.ts          # implements GameApi, transport-agnostic
│  │
│  ├─ adapters-local/                 # hackathon implementations of the ports
│  │  └─ src/  clock.local.ts (monotonic max(prev,now) + offset) · beacon.sim.ts (flagged "sim" in Almanac)
│  │           signer.local.ts (Ed25519 + key_id manifest) · repo.indexeddb.ts · repo.memory.ts (tests)
│  │           anchor.stub.ts (pending → Heartwood after sim delay) · attestor.stub.ts · kill-switch.local.ts
│  │           create-local-api.ts   # wires everything → GameApi
│  │
│  ├─ world-sim/                      # WorldSource implementation
│  │  └─ src/  population.ts (seeded cohorts) · world-source.ts · chaos.ts (outage, 10% sybil, DST week, flash crowd)
│  │           scenarios/ steady-100k.ts · slow-burn.ts · viral.ts · ghost-town.ts · closed-test.ts
│  │
│  └─ ai/                             # QuestAI port + adapters
│     └─ src/  quest-ai.ts (port) · ollama.client.ts · schema.ts (JSON-schema validate, length limit)
│              fallback.ts · templates/quests.json (never sees user-authored text)
│
├─ apps/
│  ├─ web/
│  │  └─ src/
│  │     ├─ main.tsx · app/routes.tsx
│  │     ├─ api/           game-api.ts          # composition root: LocalApi vs HttpApi
│  │     ├─ client-cache/  store.ts (own blocks, Merkle paths, Almanac heads)
│  │     ├─ lantern/       verify-my-block.ts   # imports engine/verify only
│  │     ├─ screens/       Garden · AwayReveal · Dawn (Sky Draw anim) · Lantern · Census · CharterLab · FossilRecord · FieldGuide · Settings
│  │     ├─ dev/           TimeTravel.tsx · WorldSimPanel.tsx   # only loaded when VITE_DEV_TOOLS=1
│  │     ├─ ui/ · assets/ · sw/ (PWA)
│  │
│  └─ server/                         # v1.1: create the folder + README now, build later
│     └─ src/  http.ts (Hono wrapper around api-handler) · cron/dawn.ts
│              adapters/ postgres · drand · opentimestamps · attestation · server-clock
│
├─ tools/
│  ├─ gen-golden.ts · replay.ts (event log → compare Almanac hashes) · sim-cli.ts (prints the Part 2 world tables)
│
├─ docs/  ARCHITECTURE.md · API.md · PROTOCOL-public.md
└─ private/                           # gitignored, or a separate slumberbloom-private repo
   ├─ charter_sim.py · ops.private.ts (real trust weights, cluster caps, spawn secrets)
```

---

## 4. Import boundaries

Enforce with `eslint-plugin-boundaries` or `dependency-cruiser`. These boundaries are what keep the architecture production-like.

| Package | May import |
|---|---|
| `engine` | nothing |
| `contracts` | nothing |
| `server-core` | `engine`, `contracts` |
| `adapters-local`, `world-sim` | `server-core` ports, `engine` |
| `apps/web` | `contracts`, `engine/verify` only, `ai`, and `adapters-local` only inside `api/game-api.ts` |

If `apps/web` cannot import `server-core` directly, the UI cannot cheat by reading server state. That makes the v1.1 swap a one-file change.

---

## 5. Planned `GameApi` surface (mirrors the future REST API)

| Operation | Future route | Notes |
|---|---|---|
| `heartbeat()` | `POST /heartbeat` | Foreground every 60 s; updates `last_seen` |
| `openBlock()` | `POST /blocks/open` | Mints a Dew if away ≥ 30 min (max 10 h) |
| `claimBlock(id, idemKey)` | `POST /blocks/:id/claim` | Idempotent; Dew → Gathered |
| `getGarden()` | `GET /plants` | Plants, Sun/Moon, Bloom balance |
| `water(plantId)` | `POST /plants/:id/water` | Spends a Sun/Moon |
| `getAlmanac(day)` | `GET /almanac/:day` | Public Almanac block |
| `getProof(blockHash)` | `GET /proof/:blockHash` | Merkle path for the Lantern |
| `getCensus()` / `getForecast()` | `GET /census`, `GET /forecast` | Ceiling, realized, Soil Forecast |
| `dev.setTime()` | none (dev only) | Compiled out of production builds; server rejects time params |

---

## 6. What changes at v1.1

| Concern | Hackathon | Production |
|---|---|---|
| Time | `LocalClock` plus the `?t=+49h` slider | Server clock; slider compiled out and rejected by the server (H8) |
| Randomness | `SimBeacon` (labeled `sim` in the Almanac) | drand, with the commit-before-beacon rule |
| Signing | Local Ed25519 key | Server key manifest, with rotation |
| Storage | IndexedDB | Postgres with append-only tables and revoked UPDATE/DELETE |
| Anchoring | Stub "pending" proof | OpenTimestamps |
| World | `WorldSim` | Real claims queue |
| Trust tiers | Stub (T1) | Play Integrity / App Attest |
| Transport | `LocalApi` | `HttpApi` → `apps/server` |

The engine, `server-core`, `contracts`, the Lantern and all golden fixtures are identical in both columns.

---

## 7. Build order (mapped to the 60-second demo)

1. **`engine`**: `canonical-json`, `merkle`, `fixed`, `charter`, plus golden vectors and invariant tests.
2. **`server-core` ports and `adapters-local`**: memory repo first so tests run without a browser.
3. **Night Block → Dew → Gathered**: presence, blocks, claim, then the AwayReveal screen *(demo 0:10)*.
4. **Dawn settle + Sky Draw + serials/rarity** *(0:25)*, then the **Almanac and Lantern** *(0:35)*.
5. **Plants, wilt ladder and time travel** *(0:42)*.
6. **`world-sim` + era engine + Census and Charter Lab** *(0:48)*.
7. **`ai`**: template fallback first, then Ollama on top.

---

## 8. Early decisions

- **Hashing:** use `@noble/hashes` for synchronous SHA-256 in `engine` (recommended).
- **Config split:** decide which real values live in `private/ops.private.ts` (trust weights, cluster caps, spawn secrets) versus public placeholders in the demo build.
- **Repo hygiene:** add `Private-readme.md`, `charter_sim.py` and `/private/` to `.gitignore` before the first commit; enable secret scanning.
