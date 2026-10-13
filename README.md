# logpipe-ui

[![ci](https://github.com/TiagoR85/logpipe-ui/actions/workflows/ci.yml/badge.svg)](https://github.com/TiagoR85/logpipe-ui/actions/workflows/ci.yml)
[![license: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Web UI and one-command demo hub of the logpipe family: Blazor Server dashboard (fuzzy search, level/service facets, trace view) plus the `docker compose` stack that wires ingest, search, Elasticsearch, Redis and the sample producer together.

## Status

In development — part of the logpipe family ([logpipe-ingest](https://github.com/TiagoR85/logpipe-ingest), [logpipe-search](https://github.com/TiagoR85/logpipe-search), [logpipe-contracts](https://github.com/TiagoR85/logpipe-contracts)).

## Decisions

_None recorded yet — ADRs land with the hub compose wiring._

## Roadmap

- Blazor Server scaffold + OAuth token provider + search client
- Search dashboard, trace view, level badges
- Hub compose: one `docker compose up` demo of the whole family

## About this repository

This repository is part of a personal study project (the "github-portfolio-dotnet" portfolio). It was built with heavy use of AI-assisted development tools, but it is held to production standards on purpose: automated tests with a coverage gate, CI on every push, fail-fast configuration, structured logging, and honest documentation of trade-offs. It is not affiliated with any employer or client.

## License

MIT — see [LICENSE](LICENSE).
