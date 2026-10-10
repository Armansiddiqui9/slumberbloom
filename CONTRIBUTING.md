# Contributing

Thanks for wanting to help! SlumberBloom is a hackathon prototype with a real
architecture — please read `CLAUDE.md` and `docs/ARCHITECTURE.md` first. The
short version:

## Setup

```bash
# Site server (FastAPI + static site, the demo judges see)
./run.sh            # Mac/Linux          → http://localhost:8000
run.bat             # Windows

# Web app (Vite React, the future site)
pnpm install
pnpm --filter @sb/web dev

# Engine unit tests (golden vectors, Merkle)
pnpm --filter @sb/engine test

# Mobile (C# console demo)
cd mobile-game && dotnet run --project slumberbloom.mobile.csproj
```

## Where things live

- **Engine consensus** (charter, dawn, seeds, Merkle, night-block rules) → `packages/engine/src/`
- **Web screens** → `apps/web/src/screens/` (extract from `apps/web/src/app/App.tsx`)
- **Server API** → `server/main.py` (mirrors the engine; same canonical JSON + Merkle)
- **AI / photo judging** → `packages/ai/src/` — **never** touches Charter, Sky Draw, rarity, or Merkle

## Rules

1. **The engine is pure.** It never reads `Date.now()`; take explicit inputs and keep it unit-testable.
2. **Hashing must stay sync.** Use `@noble/hashes` (or vendored) in the engine; Web Crypto stays in adapters only.
3. **Import boundaries** are documented in `docs/ARCHITECTURE.md` §4 — don't cross them.
4. **Time travel is a dev feature**, compiled out of production (`VITE_DEV_TOOLS=1`).
5. **No secrets, ever.** `private/`, `charter_sim.py`, `Private-readme.md` are gitignored and must stay local.

## Pull requests

- One concern per PR; keep diffs reviewable.
- Run `pnpm --filter @sb/engine test` (and `python scripts/build_site.py` if you touch `design/mockups/`) before pushing.
- Update the relevant doc (`README.md`, `docs/ARCHITECTURE.md`, `plan.md`) when you change behavior.
- No commits to `main` without review; open a PR against `main`.

## Commit style

Lowercase, imperative, present tense: `add lantern verification to garden screen`.
