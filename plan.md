# SlumberBloom v1 ASAP — Build Plan

> **Purpose:** Finish hackathon v1 as fast as possible. This is the only build guide for shipping the **60-second demo**.
>
> **Not this doc:** Package graphs, import boundaries, and v1.1 swap → [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md). Product pitch → [`README.md`](README.md). Visual tokens → [`design/DESIGN.md`](design/DESIGN.md).
>
> **Out of scope for v1:** Postgres / Hono server, real map & GPS wild seeds, Seed Capsules, Great Bloom, multi-device sync, push, passkeys. Landing gets a CTA → play app only — no redesign.

---

## 1. Goal & definition of done

Ship a laptop-local React PWA where judges can run the full Proof-of-Patience loop in ~60 seconds via time travel. All consensus math is pure engine code. Small open-source models (Ollama) power quests and photo judging — never Charter, Sky Draw, or Merkle.

### Demo checklist (acceptance bar)

| Beat | Time | Must see |
|------|------|----------|
| 1 | 0:00 | Garden + **Census**: ceiling falls out of the Charter (not a magic number) |
| 2 | 0:10 | Water → Tuck In → `+8h` → Dew “while you were away” → **Claim** |
| 3 | 0:25 | Jump to Dawn → **Sky Draw** anim → Frost + Bloom + checksummed serial + rarity badge |
| 4 | 0:35 | **Light the Lantern** → Merkle path verifies into the local Almanac |
| 5 | 0:42 | `+49h` wilt ladder → **Revive** (1 Sun + 1 Moon) |
| 6 | 0:48 | World Sim `+9mo` → sleep bar / Last Dawn / Era I; **Charter Lab** ceiling vs realized |
| 7 | 0:57 | “Four years of this ends in the Great Bloom. All of it runs on this laptop.” |

**P1 (high-leverage for judges):** one Sun photo quest judged by **moondream**, with accept-on-failure so a cold model never breaks the demo.

---

## 2. Baseline — what already works (do not rebuild)

| Layer | Where | Status |
|-------|--------|--------|
| Play shell | [`apps/web/src/app/App.tsx`](apps/web/src/app/App.tsx) | 4 tabs: Bed / Dew / Garden / Census + TimeTravel |
| Composition root | [`apps/web/src/api/game-api.ts`](apps/web/src/api/game-api.ts) | `createLocalApi()` only |
| Local “backend” | [`packages/adapters-local/src/index.ts`](packages/adapters-local/src/index.ts) | localStorage `sb.v1`; tuckIn → openBlock → claim → settleDawn → lantern |
| Engine (thin) | [`packages/engine`](packages/engine) | SHA-256, Merkle, `mintNightBlock` (dust/cap), stage/wilt/water |
| Contracts | [`packages/contracts/src/index.ts`](packages/contracts/src/index.ts) | `GameApi` surface (needs expansion) |
| Tests | [`packages/engine/test/golden.test.ts`](packages/engine/test/golden.test.ts) | Merkle, canon JSON, dust/10h cap |
| Landing | [`apps/landing`](apps/landing) | Static marketing; CTA not wired |
| Mockups | [`design/mockups/*.html`](design/mockups) | Target look — polish toward these, don’t restart design |

**Architecture rule (keep):**

```
UI  →  GameApi  →  LocalApi (in-browser server)  →  pure engine
         │
         └── Ollama (quests / vision only; optional with templates)
```

UI never reads game time or consensus state directly. No `apps/server` HTTP for v1.

---

## 3. Frontend — what to build

Target: split the monolith tab shell into screens under `apps/web/src/screens/`. Keep parchment tokens in [`apps/web/src/ui/theme.css`](apps/web/src/ui/theme.css). Match mockup *feel* (Newsreader / Plus Jakarta / moss / amber) — not pixel-perfect rebuild.

### Screen map (demo beat → file)

| Priority | Screen | Demo beat | Create / change | Notes |
|----------|--------|-----------|-----------------|-------|
| **P0** | **Garden** | 0:00, 0:42 | `screens/Garden.tsx` (extract from App) | Plant emoji/stage bar, water, Sun/Moon/Bloom, wilt pills, revive CTA when dormant |
| **P0** | **Census** | 0:00, 0:48 | `screens/Census.tsx` | Ceiling · Realized · Living · Dormant · Fossil · Lost to frost; wire `getCensus()` |
| **P0** | **AwayReveal / Morning Dew** | 0:10 | `screens/AwayReveal.tsx` | “WHILE YOU WERE AWAY — Nh of patience…”, Claim Dew, Soil Forecast line |
| **P0** | **Dawn / Sky Draw** | 0:25 | `screens/Dawn.tsx` | Short animation → Frost reveal: Bloom paid, serial `SB-…`, rarity badge |
| **P0** | **Lantern** | 0:35 | `screens/Lantern.tsx` + `lantern/verify-my-block.ts` | Client re-hash + Merkle walk via **engine verify only**; show ok/fail + root |
| **P0** | **Charter Lab** | 0:48 | `screens/CharterLab.tsx` | Sliders for `S_0` / eras → live ceiling, era dates, Soil Forecast (pure engine sizing; no server) |
| **P0** | **World Sim panel** | 0:48 | `dev/WorldSimPanel.tsx` | Dev-only: `+9mo`, fill sleep bar, force Last Dawn / halving; only when `VITE_DEV_TOOLS=1` |
| **P0** | **Bedtime** | 0:10 setup | `screens/Bedtime.tsx` | Tuck In; keep copy from mockup `bedtime.html` |
| **P0** | **App shell / nav** | all | `app/App.tsx`, optional `app/routes.tsx` | Tabs or simple hash routes: bed · dew · garden · dawn · census · lab |
| **P1** | **Quest + camera** | morning bonus | `screens/Quest.tsx` | Show today’s quest → capture photo → call QuestAI → reward tiers |
| **P1** | **Gardener’s Journal** | after claim/dawn | snippet inside Dew or Garden | Two cozy sentences from llama / template |
| **P2** | Onboarding | — | skip or copy `onboarding.html` later | Cut unless spare time |
| **P2** | Fossil Record | — | skip or one line after `+2y` time travel | Cut unless spare time |
| **P2** | Field Guide | — | skip | Cut |

### Frontend file checklist

```
apps/web/src/
├── app/App.tsx              # shell + tab state (thin)
├── api/game-api.ts          # LocalApi only (unchanged role)
├── client-cache/store.ts    # own blocks, Almanac head, Merkle paths (light client)
├── lantern/verify-my-block.ts
├── screens/
│   ├── Bedtime.tsx
│   ├── AwayReveal.tsx
│   ├── Dawn.tsx
│   ├── Garden.tsx
│   ├── Lantern.tsx
│   ├── Census.tsx
│   ├── CharterLab.tsx
│   └── Quest.tsx            # P1
├── dev/TimeTravel.tsx       # label judge buttons: +8h, Dawn, +49h, +9mo
├── dev/WorldSimPanel.tsx
└── ui/theme.css
```

### Landing (minimal)

- Wire “Try Web Demo” in [`apps/landing/index.html`](apps/landing/index.html) (and mobile) → `/play/` or Vite `http://localhost:5173`.
- No new marketing sections. Brand in play app stays **SlumberBloom**.

### UI must call (extend `GameApi`)

| UI action | API |
|-----------|-----|
| Tuck in | `tuckIn()` / `heartbeat()` |
| Mint dew on wake | `openBlock()` |
| Claim | `claimBlock(id, idemKey)` |
| Dawn | `settleDawn()` → returns mint summary (Bloom, seeds) |
| Garden | `getGarden()`, `water()`, `revive()` |
| Census / forecast | `getCensus()`, `getForecast()` |
| Almanac / Lantern | `getAlmanac(day)`, `getProof(blockHash)` / `lantern()` |
| Dev | `dev.advance(hours)`, `dev.simWorld(months)` or World Sim panel helpers |
| Quest reward | `completeQuest({ tier })` after AI judge (or fold into `water` / Sun grant) |

---

## 4. Backend — what to build (in-browser)

**v1 backend = pure engine + expanded LocalApi.** Do **not** build Hono/Postgres. Prefer expanding [`packages/adapters-local`](packages/adapters-local) first; extract `packages/server-core` only if LocalApi becomes unmaintainable mid-hack.

### Engine modules to add (`packages/engine/src/`)

| Module | Path (target) | Responsibility | Demo beat |
|--------|---------------|----------------|-----------|
| Charter | `charter/charter.ts`, `sizing.ts` | Sealed params, `S_k = S_0/2^k`, ceiling Σ, Soil Forecast inputs | Census, Charter Lab |
| Block state | `blocks/block-state.ts` | Dew → Gathered → Frost (+ 24h evaporation if cheap) | AwayReveal, Dawn |
| Dawn | `dawn/settle.ts`, `skydraw.ts`, `bloom.ts` | Settle day’s claims; sim beacon; Bloom payout; seed winners | Dawn / Sky Draw |
| Seeds | `seeds/serial.ts`, `rarity.ts` | `SB-…` Crockford + check char; Dawnling / Solstice / Eraborn / … | Dawn reveal |
| Almanac | `almanac/seal.ts`, `proofs.ts`, `verify.ts` | Daily seal; Merkle inclusion for Lantern | Lantern |
| Era | `era/era-engine.ts` | Sleep bar + calendar gate; Last Dawn; Era I transition | World Sim +9mo |
| Garden | extend `garden/plant.ts`; `wilt.ts` | Passive +1/hr (capped), era multiplier, revive → growth 60 | Garden wilt |
| Math / PRNG | `math/fixed.ts`, `prng.ts` | Deterministic draws from `H(beacon‖commit)` | Sky Draw |
| Encoding | `encoding/crockford32.ts` (if needed) | Serial alphabet | Seeds |

**Already done — reuse:** `crypto/hash.ts`, `crypto/merkle.ts`, `encoding/canonical-json.ts`, `blocks/night-block.ts` (dust &lt;30m, cap 10h).

### Golden tests (add to `packages/engine/test/`)

- Charter ceiling for known `S_0` / K eras
- Serial checksum round-trip
- Almanac Merkle inclusion (Lantern)
- Sky Draw identical given fixed beacon + claim set
- Dust / 10h cap (exists)
- Wilt thresholds + revive math

### LocalApi expansion (`packages/adapters-local`)

| Method | Behavior |
|--------|----------|
| `tuckIn` / heartbeat | Stamp `lastSeen` |
| `openBlock` | Mint Dew if away ≥ 30m (max 10h); link chain |
| `claimBlock` | Dew → Gathered; idempotent |
| `settleDawn` | Gathered → Frost; Bloom; Sky Draw mint; seal Almanac day |
| `getGarden` / `water` / `revive` | Growth, wilt, spend Sun/Moon |
| `getCensus` / `getForecast` | From Charter + realized mints + sim world |
| `getAlmanac` / `lantern` | Public day block + Merkle proof vs Almanac (not flat personal list) |
| `dev.advance` | Offset clock (existing) |
| World Sim | Seeded virtual gardeners feed dawn claims so Φ / halving / forecast look real |

**Storage:** keep versioned JSON in `localStorage` (`sb.v1` → `sb.v2` on schema bump). Switch to IndexedDB **only** if World Sim rows blow the quota during rehearsal.

**Patience rules the adapter must enforce** (engine helpers): interval cap 10h/block & 10 claimed hours/day; dust &lt;30m; Dew evaporates after 24h; sunlight rule (day quest → full night weight) — stub sunlight as “quest completed today” flag for demo.

---

## 5. AI — small open-source models in v1

Models make the **daily ritual** feel alive. They must **never** touch consensus (Charter, Sky Draw, rarity, Merkle, serials).

```
┌─────────────┐     optional      ┌──────────────────────┐
│ Quest.tsx / │ ───────────────►  │ Ollama :11434        │
│ Dew journal │                   │ llama3.2:3b (text)   │
└──────┬──────┘                   │ moondream (vision)   │
       │ timeout / down           └──────────────────────┘
       ▼
 templates/quests.json + journal lines  →  demo continues
```

### Model roles

| Model | Size role | What it does in-app | Output |
|-------|-----------|---------------------|--------|
| **`llama3.2:3b`** | Text | (1) Daily Sun/Moon quest copy ≤12 words, doable anywhere. (2) Gardener’s Journal — two cozy sentences after claim or dawn | Plain text / short JSON `{ quest }` / `{ journal: [s1,s2] }` |
| **`moondream`** | Vision | Judge quest photo on-device path: browser → Ollama with base64 image | Strict JSON `{ matches: boolean, isPlant: boolean, reason: string }` |
| **Templates** | Fallback | Curated quests + journal lines when Ollama is slow or missing | Same shapes as above |

### Package layout

```
packages/ai/
├── src/
│   ├── quest-ai.ts          # port: generateQuest, judgePhoto, writeJournal
│   ├── ollama.client.ts     # fetch http://localhost:11434/api/chat
│   ├── schema.ts            # validate + length limits; reject junk
│   ├── fallback.ts          # templates when fail/timeout (~3–5s)
│   └── templates/quests.json
└── package.json             # @sb/ai
```

Wire from web via a thin client helper (or LocalApi method that calls QuestAI). Photos **never leave the device** except to `localhost:11434`.

### Reward tiers (gameplay)

| Judge result | Reward |
|--------------|--------|
| `isPlant && matches` | Full Sun (+ small rare-seed chance if Dawn path allows) |
| `isPlant && !matches` | Half Sun |
| neither | Soft “try again” — no punish |
| timeout / Ollama down | **Accept photo + half Sun** (graceful failure) — sunlight rule still satisfiable for demo |

### Setup (document in README Demo section)

```bash
# pull once
ollama pull llama3.2:3b
ollama pull moondream

# Windows PowerShell — allow browser origin, then restart Ollama
$env:OLLAMA_ORIGINS = "*"
ollama serve

# Mac/Linux
OLLAMA_ORIGINS=* ollama serve
```

Core crypto loop (tuck → dew → dawn → lantern) must work **with Ollama off**. AI is the bonus act, not the show.

### Prompt constraints (keep tiny)

- Quest: “Under 12 words. Doable indoors or on a short walk. No brand names. One concrete thing to find or notice.”
- Judge: “Reply ONLY with JSON keys matches, isPlant, reason. reason ≤ 12 words.”
- Journal: “Two short cozy sentences about a garden that grew while the gardener slept. No hashtags.”

---

## 6. Build phases (ASAP order)

Engine → API → screen. Never fake consensus in the UI.

| Phase | Time box | Work | Unlocks demo beat |
|-------|----------|------|-------------------|
| **0 Hygiene** | 0.5d | Confirm `pnpm dev` / `pnpm test`; keep import boundaries; TimeTravel behind `VITE_DEV_TOOLS` | — |
| **1 Engine** | 1–2d | Charter, block-state, dawn/skydraw, seeds, almanac/verify, era, wilt revive + golden tests | Math for 0:00–0:48 |
| **2 LocalApi** | 1d | Expand adapter methods; World Sim seed population; schema bump storage | Live state for UI |
| **3 Frontend P0** | 1–2d | Extract screens; Dawn anim; Census + Charter Lab; Lantern client verify; WorldSimPanel; judge-labeled TimeTravel | Full 60s path |
| **4 AI P1** | 0.5–1d | `@sb/ai` + Quest screen + journal; templates; CORS docs | Photo quest showcase |
| **5 Rehearse** | 0.5d | Run 60s script cold; freeze cuts; README Demo blurb | Ship |

**Parallelization tip:** After Phase 1 Charter + Almanac APIs exist, one person can build Census/Charter Lab UI while another finishes Sky Draw + Dawn screen.

---

## 7. 60-second demo script (judge path)

1. Open `/play/` — Garden + Census: “The ceiling falls out of the Charter.”
2. Water once. Bedtime → **Tuck In**. Dev **`+8h`**. Morning Dew → claim.
3. **Jump to Dawn** — Sky Draw plays → Frost, Bloom, serial, rarity.
4. **Light the Lantern** — verified into Almanac.
5. Dev **`+49h`** — wilt → Revive.
6. World Sim **`+9mo`** — bar fills, Last Dawn, Era I; Charter Lab ceiling vs realized.
7. Close: “All of it runs on this laptop.” *(Optional: 10s photo quest if Ollama is warm.)*

Dev button labels (suggested): `+8h` · `Dawn` · `+49h` · `+9mo`.

---

## 8. Explicit cuts (do not gold-plate)

- No `HttpApi`, Hono, Postgres, or real cron
- No GPS map / outdoor wild-seed claiming (sim vouchers only if needed for mint)
- No Seed Capsules, postcards, Great Bloom fork ritual
- No Fossil museum polish / Field Guide / full onboarding
- No PWA install / push polish beyond Vite defaults
- No model use for money-looking scarcity math
- No brand unification project (play app = **SlumberBloom**)

---

## 9. Pointers

| Need | Doc |
|------|-----|
| Package graph, ports, v1.1 swap | [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) |
| Pitch & protocol story | [`README.md`](README.md) |
| Run site + mockups | [`QUICKSTART.md`](QUICKSTART.md) |
| App visual system | [`design/DESIGN.md`](design/DESIGN.md) |
| HTML comps to chase | [`design/mockups/`](design/mockups) |

---

## 10. Done means

- [ ] All seven demo beats work without Ollama
- [ ] Census numbers come from Charter engine
- [ ] Dawn mints checksummed serial + rarity from Sky Draw
- [ ] Lantern verifies against sealed Almanac Merkle root
- [ ] Wilt + revive works via `+49h`
- [ ] World Sim reaches Era I; Charter Lab shows ceiling vs realized
- [ ] (P1) Photo quest works with moondream **or** falls back cleanly
- [ ] Landing CTA opens the play app
