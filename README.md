# logpipe-ui

Blazor Server dashboard and the **one-command demo hub** for the `logpipe` family: a sample app ships NLog entries over HTTP, ingest rate-limits/dedups them and forwards over gRPC, search indexes into Elasticsearch and serves a REST API, and this UI renders search + trace views.

[![CI](https://github.com/TiagoR85/logpipe-ui/actions/workflows/ci.yml/badge.svg)](https://github.com/TiagoR85/logpipe-ui/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

## About this repository

This repository is part of a personal study project (the `github-portfolio-dotnet` portfolio). It was built with heavy use of AI-assisted development tools, but it is held to production standards on purpose: automated tests with a coverage gate, CI on every push, fail-fast configuration, structured logging, and honest documentation of trade-offs. It is not affiliated with any employer or client.

## How it fits the family

```mermaid
flowchart LR
    producer[logpipe-sample-producer] -->|POST /v1/logs (OIDC + Redis)| ingest[logpipe-ingest]
    ingest -->|gRPC StreamLogs| search[logpipe-search]
    search -->|bulk index| es[(Elasticsearch)]
    ui[logpipe-ui (this repo)] -->|GET /v1/logs| search
    prom[Prometheus] -->|scrape /metrics| ingest
    prom -->|scrape /metrics| search
```

## Quickstart - one command

```powershell
# from a clean clone of this repo
Copy-Item .env.example .env
# edit .env and set OIDC_CLIENT_SECRET to any non-empty value
docker compose up --build
```

- UI: http://localhost:8092
- Prometheus: http://localhost:9090
- Ingest (OIDC + REST): http://localhost:8090
- Search (REST): http://localhost:8091 — gRPC (HTTP/2 only): 8093

The compose stack builds `ingest`, `search` and `producer` from their GitHub `main` branches (git-context builds), so this is the only repo you need to clone.

## Ports

| Service | Host port | Notes |
| --- | --- | --- |
| logpipe-ui | 8092 | Blazor Server dashboard |
| logpipe-ingest | 8090 | OIDC issuer + `POST /v1/logs` |
| logpipe-search | 8091 | `GET /v1/logs`, `GET /v1/logs/trace/{traceId}` (HTTP/1.1) |
| logpipe-search gRPC | 8093 | `StreamLogs` from ingest (HTTP/2 only — cleartext dual-version mode is rejected by Kestrel, see Troubleshooting) |
| Elasticsearch | (not published) | in-network only; `curl` from inside for debugging |
| Redis | (not published) | in-network only |
| Prometheus | 9090 | scrapes both services' OTel `/metrics` |

## Configuration

| Variable | Compose value | Host-local alternative |
| --- | --- | --- |
| `OIDC_CLIENT_SECRET` | any non-empty value (`.env`) | same secret used for `logpipe-writer`, `logpipe-reader`, `logpipe-ui` clients |
| `Redis__Configuration` | `redis:6379` | `localhost:6379` |
| `Search__GrpcUrl` | `http://search:8093` | `http://localhost:8093` |
| `Search__BaseUrl` (ui) | `http://search:8091` | `http://localhost:8091` |
| `Oidc__Issuer` | `http://ingest:8090` | `http://localhost:8090` |
| `Oidc__DbPath` | `/tmp/openiddict.db` | `openiddict.db` (SQLite file) |
| `Elasticsearch__Url` | `http://elasticsearch:9200` | `http://localhost:9200` |
| `Jwt__Authority` | `http://ingest:8090` | `http://localhost:8090` |
| `Jwt__RequireHttpsMetadata` | `false` (plain-HTTP demo issuer) | `false` in Development via `appsettings.Development.json` |
| `Retention__Days` | `7` | `7` |
| `OAuth__TokenUrl` | `http://ingest:8090/connect/token` | `http://localhost:8090/connect/token` |
| `OAuth__ClientId` / `OAuth__ClientSecret` / `OAuth__Scope` | `logpipe-ui` / `OIDC_CLIENT_SECRET` / `logs:read` | same |

## Tests

Per repo, after `scripts/fetch-contracts.ps1` (fetches the pinned `LogPipe.Contracts` NuGet package):

```powershell
dotnet test -c Release
```

This repo's tests (bUnit + WireMock) gate at 80% line coverage.

## Decisions

- [ADR-0001 — Hub topology (git-context builds)](docs/adr/ADR-0001-hub-topology.md)
- [ADR-0002 — UI auth (confidential client against the ingest OIDC issuer)](docs/adr/ADR-0002-ui-auth.md)
- [ADR-0003 — Test contracts (data-testid as the DOM contract)](docs/adr/ADR-0003-test-contracts.md)

## Troubleshooting

- **`docker compose up --build` fails on the bake step:** run `COMPOSE_BAKE=false docker compose up --build` (known environment issue; see einvoice-ui `07be5f7`).
- **Ingest drops batches with `HTTP_1_1_REQUIRED (0xd)`:** Kestrel only negotiates HTTP versions via TLS/ALPN; a cleartext `Http1AndHttp2` endpoint accepts the h2c preface but answers GOAWAY `HTTP_1_1_REQUIRED` (dotnet/aspnetcore#56984). Search therefore serves gRPC on a dedicated HTTP/2-only port (`8093`) and the query API on `8091`.
- **Search shows no results for ~2 s after the first log:** Elasticsearch near-real-time refresh + the forwarder's batch delay.
- **Compose refuses to start:** `OIDC_CLIENT_SECRET` must be set in `.env` — the compose file guards it with `${OIDC_CLIENT_SECRET:?set in .env}`.
- **Local uncommitted changes to ingest/search:** git-context builds pull from GitHub `main`; test local changes with a manual `docker build` + compose `image:` override.

## Roadmap

- Tags/releases for ingest, search and UI after the family smoke passes.
- Index Lifecycle Management (ILM) instead of the code-driven retention loop.
- OTLP collector as a scrape target.
- Interactive user auth (per-user tokens) for the UI.

## License

MIT — see [LICENSE](LICENSE).
