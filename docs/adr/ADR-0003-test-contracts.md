# ADR-0003: Test contracts (data-testid as the DOM contract)

- **Decision:** every interactive/searchable DOM element that tests or the hub smoke depend on carries a stable `data-testid` attribute (`search-input`, `level-filter`, `service-filter`, `search-button`, `result-row`, `facet-level`, `facet-service`, `badge`, `trace-row`); bUnit tests and the Task 12 smoke select on these ids, never on CSS classes or markup structure.
- **Context:** Bootstrap class names and table markup change during design tweaks; F1's bUnit tests mixed structural and semantic selectors, which made restyles risky — the id contract makes visual refactors test-safe.
- **Consequences:** testids are part of the public component contract (removing one is a breaking change for the suite); the smoke asserts on rendered text (`smoke`, `smoke boom`) *and* testids, so both layers are covered.
