# Security Policy

## Supported versions

SlumberBloom v1 is a hackathon prototype that runs locally. There is no production
deployment, no user accounts, and no network exposure beyond `localhost:8000`.
The React demo's data lives in your browser's `localStorage` (`sb.v1`); the
FastAPI server's data lives in `server/state.json`. Neither leaves your machine.

## Reporting a vulnerability

Please do **not** open a public issue for a security concern.

1. Write to the maintainer privately (see the repo's `README.md` / GitHub
   **Security** tab for contact once the repository is public).
2. Include a short description, the affected file/endpoint, and how to reproduce.
3. Expect an initial response within 72 hours.

## Scope notes

- **AI never touches consensus.** Photo judging (`moondream`) and quest text
  (`llama3.2:3b`) are demo conveniences; photos never leave your device.
- **No real money, tokens, or wallets.** "Sealed capsules" and "wax-sealed gifts"
  are game mechanics, not cryptographic assets.
- **Time travel** (`/api/dev/advance`, `?t=+49h`, `VITE_DEV_TOOLS=1`) is a dev
  feature and must stay compiled out of any public build.

## Hardening checklist for v1.1 (server)

- Reject `?t=` / `/api/dev/*` on any non-local deployment (H8).
- Sign Night Blocks server-side; verify signatures in the light client.
- Keep the server's signing key out of the repo (`private/ops.private.ts`).
