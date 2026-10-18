# ADR-0001: Hub topology (git-context builds)

- **Decision:** the hub builds services from **git contexts** (`https://github.com/TiagoR85/<repo>.git#main`) so a clean clone of `logpipe-ui` alone can `docker compose up`; Prometheus is the only other browser-exposed port.
- **Context:** the einvoice hub proved this pattern (fresh-clone smoke in `D:\opencode-temp\hub-smoke`); sibling-directory contexts would break the "clone one repo, run everything" promise.
- **Consequences:** compose builds pull from GitHub (slow first build, cached afterward); local uncommitted changes to ingest/search require a manual `docker build` + compose `image:` override for testing (documented in the README troubleshooting).
