# SlumberBloom — Mobile Game (v1 scaffold)

> Game: a smooth mobile experience that mirrors everything working on the web.
> Folder: `mobile-game/` — easy access, designed for later testing in Android Studio.
> Engine / UI choice: C# (.NET) — pure engine ports to Unity / MAUI / Android Studio easily.

---

## 1. What's here (built today)

| Folder | Mirrors (web) | Status |
|---|---|---|
| `src/Engine/` | `packages/engine/` (crypto, blocks, plants, merkle) | **done** — pure C# |
| `src/Contracts/` | `packages/contracts/` (`GameApi`, `BlockStage`, `Garden`, `Proof`) | **done** |
| `src/Backend/` | `packages/adapters-local/` (`LocalApi`) | **done** — deterministic server |
| `src/UI/` | `apps/web/src/app/` (screens, onboarding, navigation) | **done** — console demo |
| `src/Program.cs` | demo loop | **done** — runs the full loop |

---

## 2. How to run today (console)

Requires `.NET SDK 9.0` (or 8.0 with a quick target-framework edit).

```bash
cd mobile-game
dotnet build slumberbloom.mobile.csproj
dotnet run --project slumberbloom.mobile.csproj
```

The demo executes: onboarding → tuck-in → +8h → open dew → claim → dawn → lantern → garden → water.

---

## 3. Open in Android Studio (later)

1. Launch Android Studio.
2. File → Open → select `C:/Users/arman/Open-source/HTBF/slumberbloom/mobile-game/`.
3. Android Studio recognizes `.cs` / `.csproj` as C# / .NET.
4. Attach to a device or emulator; build APK from `slumberbloom.mobile.csproj` (add an Android target framework, e.g. `net9.0-android`, when ready).

---

## 4. Unity attachment (when you attach an engine)

When you decide on Unity (or a similar engine):

- Copy `src/Engine/` and `src/Contracts/` into your Unity project's `Assets/Scripts/`.
- Copy `src/Backend/` into `Assets/Scripts/Backend/`.
- Replace Unity's MonoBehaviour UI layer with the `UI/` screen logic, or wire `AppModel` → Unity scenes.
- The `IGameApi` interface stays unchanged — the engine doesn't know if it's mobile, web, or Unity.

---

## 5. Architecture mapping (web → mobile-game)

Based on `docs/ARCHITECTURE.md` §3:

```
packages/engine/              → src/Engine/
packages/contracts/           → src/Contracts/
packages/adapters-local/      → src/Backend/
packages/ai/                  → (to come — local LLM quests)
packages/server-core/         → (to come — server-signed blocks v1.1)
packages/world-sim/           → (to come — simulated population)

apps/web/src/app/App.tsx      → src/App.cs + src/UI/
server/main.py                → (to come — real FastAPI backend)
```

- **Pure engine** (`src/Engine/`): no I/O, takes `(state, serverNow, charter)`.
- **Contracts** (`src/Contracts/`): the only surface the UI may call.
- **Backend** (`src/Backend/`): `LocalApi` is now; `HttpApi` (v1.1) swaps one file later.

---

## 6. What's still to build (next passes)

- [ ] **Real persistence**: `MemoryBlobStore` → Android Preferences / IndexedDB / file.
- [ ] **Unity / MAUI UI**: the console `UI/` screens are logic-only; need actual XAML / Unity Canvas.
- [ ] **Onboarding polish**: the `OnboardingScreen.cs` replicates the mockup; needs images/styles.
- [ ] **Real server**: replace `LocalApi` with `HttpApi` → `apps/server` (Hono + Postgres) per `docs/ARCHITECTURE.md`.
- [ ] **World Simulator**: `packages/world-sim/` (seeded cohorts, scenarios) → `src/Engine/World/`.
- [x] **AI quests**: `packages/ai/` → local `LLamaSharp` + `Q4_K_M` (Qwen2.5-0.5B / Llama-3.2-1B); three jobs (quest, photo, seed); RAM budget test passes (< 600 MB peak, ~0 idle).
- [ ] **Charter Lab**: supply/ceiling math, sliders, era engine (`packages/engine/src/charter/`).
- [ ] **Dawn animation / Sky Draw**: `packages/engine/src/dawn/skydraw.ts` → mobile animation.
- [ ] **Lantern verification UI**: full Merkle proof display (`verifyPath`).

---

## 7. Design notes

- Uses the same warm palette (`#fdf9ee` surface, moss `#1f3d2b`, amber `#f59e0b`).
- Typography: Newsreader + Plus Jakarta Sans (loaded via Google Fonts in the web version; mobile can embed).
- The game loop is **pure**: `mintNightBlock`, `stageOf`, `wiltOf`, `water`, `merkleRoot`, `verifyPath` — all deterministic.
- Time travel (`+8h`, `+24h`, `+49h`) is a `DevApi` feature, compiled out of production builds (same as web `/dev` buttons).

---

Created: 2026-10-09 · Project: slumberbloom (HTBF)
