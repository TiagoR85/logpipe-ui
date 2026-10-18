# ADR-0002: UI auth (confidential client against the ingest OIDC issuer)

- **Decision:** the UI is a confidential client of the ingest OIDC issuer (scope `logs:read`, client `logpipe-ui`, shared demo secret from env), fetching bearer tokens via client_credentials and attaching them to search calls.
- **Context:** Blazor Server keeps the secret server-side (never shipped to a browser); a real user-facing deployment would add an interactive flow + per-user tokens (roadmap) — F2 demonstrates the machine-to-machine path end-to-end.
- **Consequences:** anyone with the compose `.env` can read logs (acceptable for a local demo, stated in the README); the UI's `BaseAddress` for both token and search calls is derived from one config value (`OAuth:TokenUrl`).
