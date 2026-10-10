# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

**SlumberBloom** — a "proof of patience" garden game (Bitcoin-inspired: Night Blocks via SHA-256 + Merkle, Dawn settlement, Charter, Sky Draw, Lantern verification). Hackathon v1 runs locally; v1.1 swaps adapters to a real server (Postgres/Hono). Pitch and protocol: [`README.md`](README.md); build plan: [`plan.md`](plan.md); architecture: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md).

Two working surfaces coexist:
- **Real server + site** (`server/`, `run.sh` / `run.bat`): FastAPI on port 8000 serving static site pages (`landing-desktop`, `garden-desktop`, `bedtime`, `morning-dew`, onboarding) plus the React play demo (`/play/`) and a JSON state file (`state.json`). This is what judges see.
- **In-browser PWA** (`apps/web/`): Vite React app intended to replace the static site over time; currently thin (`App.tsx`, `main.tsx`, `game-api.ts`).
- **C# mobile scaffold** (`mobile-game/`): mirrors `packages/engine` + contracts + `LocalApi`; runs with `dotnet run`. Not wired to the server.
- **TypeScript packages** (`packages/engine/`, `packages/contracts/`, etc.): mostly `.gitkeep` / skeleton; only `packages/engine/src/crypto/hash.ts`, `merkle.ts`, `encoding/canonical-json.ts`, `blocks/night-block.ts`, `garden/plant.ts` and `test/golden.test.ts` have content. Import boundaries are enforced per [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) §4.

The 60-second demo path (from [`plan.md`](plan.md)): open `/play/` → Tuck In (`/api/tuck-in`) → `+8h` (dev advance) → open block (`/api/block`) → Claim (`/api/blocks/{h}/claim`) → Dawn (`/api/dawn`) → Lantern (`/api/lantern/{h}`) → Garden (`/api/garden`) → Water (`/api/garden/water`); time travel via `/api/dev/advance?hours=N`.

## Build / run / test commands

**Site server (what runs today)**
```bash
# Mac/Linux
./run.sh
# Windows
run.bat
# Manual
python3 -m pip install -q -r server/requirements.txt && python3 scripts/build_site.py && python3 -m uvicorn server.main:app --port 8000 --reload
```
- Serves `http://localhost:8000` — landing (`/`), mobile (`/m`), `/app/*`, `/play/` (pre-built React demo from `dist/` mounted at `/play/`), `/static/` (site pages rebuilt by `scripts/build_site.py`), `/docs/`.
- State lives in `server/state.json`; reset via POST `/api/dev/reset`. `live.js` wires static mockup buttons to `/api/tuck-in`; the React `/play/` demo (`apps/web/src/app/App.tsx`) talks directly to `createLocalApi()` (localStorage `sb.v1`) unless the composition root (`api/game-api.ts`) is swapped.
- `scripts/build_site.py` rebuilds the static site from `design/mockups/*.html` into `server/static/site/`.

**Web app (partial / future)**
```bash
pnpm --filter @sb/web dev       # vite dev server (base /play/ per vite.config.ts)
pnpm --filter @sb/web build     # production build
pnpm --filter @sb/engine test   # engine unit tests (golden, merkle)
```
- Single test / filter: `pnpm --filter @sb/engine test -- --run golden` (vitest; see `packages/engine/package.json`: `"test": "vitest run"`)

**Mobile (C# scaffold)**
```bash
cd mobile-game
dotnet build slumberbloom.mobile.csproj
dotnet run --project slumberbloom.mobile.csproj
```

**Ollama (AI / photo judging — optional for demo)**
```bash
ollama pull llama3.2:3b
ollama pull moondream
$env:OLLAMA_ORIGINS = "*"   # PowerShell; Mac/Linux: OLLAMA_ORIGINS=* ollama serve
ollama serve
```
Photos never leave device; template fallback (`packages/ai/src/templates/quests.json`) keeps demo alive if Ollama is down (required by [`plan.md`](plan.md) P1).

## High-level architecture (the part that takes multiple files to understand)

The consensus path is pure and shared across web, server, and mobile: `UI / GameApi → LocalApi / HttpApi → pure engine`. The engine (`packages/engine/`) is the only layer that knows SHA-256, canonical JSON, Merkle trees, night-block rules (dust <30m, interval cap 10h, 10h/day max, sunlight rule), and growth stages / wilt ladder (`stageOf`, `wiltOf`, `water`). It never reads `Date.now()`; it takes explicit inputs and is fully unit-testable.

The server (`server/main.py`) is not the "real" production server — it's the hackathon adapter. It replicates the engine's canonical JSON + RFC 6962 Merkle exactly (`H`, `canon`, `leaf`, `node`, `levels`, `path`) and exposes the same API surface the web app eventually needs (`/api/tuck-in`, `/api/block`, `/api/blocks/{h}/claim`, `/api/dawn`, `/api/garden`, `/api/garden/water`, `/api/lantern/{h}`, `/api/dev/advance`, `/api/dev/reset`). The `state.json` file is a rudimentary event log / DB; `packages/server-core/` and `packages/ai/` describe the intended adapter structure but are mostly skeleton — while `packages/adapters-local/src/index.ts` (`createLocalApi`, localStorage `sb.v1`) is fully implemented and is what the React demo uses.

**Key design decisions from [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md):**
- Two stores: server DB (`IndexedDB` / `state.json`) vs client cache (light client, verifies via Lantern / `engine/verify`).
- Event log is source of truth (heartbeat, block mint, claim, dawn settled); projections derived from it; replay must reproduce every Almanac hash (invariant B2.7).
- Dawn settlement runs through a `Scheduler` port; in the hackathon `catchUp()` runs `settle()` when clock crosses UTC dawn (including via time travel); idempotent.
- World Simulator (`packages/world-sim/`) feeds `settle()` with a seeded virtual population so the global sleep bar / halving / Soil Forecast look realistic in the demo.
- Hashing must stay sync: use `@noble/hashes` (or vendored) in engine; Web Crypto is async and must stay in adapters only.
- `GameApi` interface (`packages/contracts/src/index.ts`) is the only surface UI can touch; `apps/web/src/api/game-api.ts` wires to `LocalApi` (in-browser) now and will swap to `HttpApi` for v1.1 via a one-file change (the composition root).
- Import boundaries (`engine` nothing; `server-core` engine+contracts; `adapters-local` + `world-sim` server-core ports + engine; `apps/web` contracts + engine/verify + ai + adapters-local only inside `api/game-api.ts`) keep the architecture production-like.

**Where to add code:**
- Engine consensus (charter, dawn, seeds, almanac proofs, era gate): `packages/engine/src/` (mostly skeleton; see `plan.md` §4 for module list).
- Web screens (Garden, Census, AwayReveal, Dawn/Sky Draw, Lantern, Charter Lab, World Sim): `apps/web/src/screens/` (skeleton / `.gitkeep` today; extract from `app/App.tsx`).
- Mobile engine port: `mobile-game/src/Engine/` (already mirrors web engine; uses `MemoryBlobStore` / `AppModel`).
- AI / photo judging: `packages/ai/src/` or `mobile-game/src/AI/`; never touches Charter / Sky Draw / rarity / Merkle.

## Important rules / cut decisions

- **Time travel is a dev feature, compiled out of production.** `?t=+49h`, `dev.advance()`, `WorldSimPanel` are behind `VITE_DEV_TOOLS=1`. The server's `/api/dev/advance` is for demo only; production will reject time params (H8).
- **No real server for v1:** do not build Hono/Postgres/cron. `packages/adapters-local/src/index.ts` (`createLocalApi`, `localStorage sb.v1`) is the working backend; `packages/server-core/` and `packages/ai/` are skeleton.
- **No GPS / outdoor wild seeds for v1:** vouchers only in simulation.
- **No Seed Capsules, postcards, Great Bloom, Fossil museum polish, multi-device sync, push** — explicitly cut in [`plan.md`](plan.md) §8.
- **AI must never touch consensus.** `llama3.2:3b` does quest text + journal; `moondream` judges photos with strict JSON; templates (`quests.json`) are the fallback.
- `.gitignore` already excludes `Private-readme.md`, `charter_sim.py`, `/private/` (secret spawn secrets / trust weights) — do not commit those.

## Quick reference pointers

- Pitch / protocol: [`README.md`](README.md) (Sections 1–14; includes Charter math formulas)`
- Build plan / demo script / cuts: [`plan.md`](plan.md) (Sections 1–9)
- File layout / import boundaries / v1.1 swap: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) (Sections 1–8)
- Web app screens / API calls / AI setup: [`plan.md`](plan.md) §3–5
- Mobile mapping / build: [`mobile-game/README.md`](mobile-game/README.md) (Sections 1–7)
- Visual mockups / design tokens: [`design/DESIGN.md`](design/DESIGN.md), [`design/mockups/*.html`](design/mockups/)
- Landing site build script: [`scripts/build_site.py`](scripts/build_site.py) (rewrites mockup links → `/app/` routes)
