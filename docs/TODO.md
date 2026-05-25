# Development Items

Prioritized backlog based on the current codebase state. Status markers:

- **Missing**: no meaningful implementation found.
- **Partial**: some infrastructure or related behavior exists, but the TODO is not complete.
- **Valid**: still relevant as written.

## P0 - Core Runtime Correctness

- [ ] **Valid**: Wire person movement registration into the domain model. `RegisterPersonMovement` currently persists directly through Dapper/SQLite and does not update `Floor`, `Person`, or `HazardZone`, so alarm and occupancy behavior is bypassed.
- [ ] **Missing**: Add API boundary validation for request bodies and route values before handlers run.
- [ ] **Missing**: Add global structured error handling using RFC 9457 Problem Details.
- [ ] **Missing**: Define a project-wide value-object validation convention so invalid route values such as `HazardZoneName` consistently return `400` without duplicated endpoint guards.
- [ ] **Missing**: Add API-level integration tests proving person movement posts update floor occupancy and hazard-zone alarm state.

## P1 - Release Readiness and Observability

### Build and CI

- [ ] **Missing**: Add a GitHub build script or workflow job that runs restore/build/test.
- [ ] **Partial**: Expand the existing GitHub workflow beyond encoding checks into a continuous integration pipeline.
- [ ] **Partial**: Add code coverage reporting. Coverlet packages/tools exist, but coverage is not reported by CI.

### API Operations

- [ ] **Missing**: Add a health/readiness endpoint (`/health`) for container/orchestration readiness.
- [ ] **Missing**: Add correlation/request ID tracking for traceability across logs.
- [ ] **Partial**: Improve observability beyond Serilog request logging with domain-relevant context such as person, floor, hazard-zone, and state transitions.
- [ ] **Valid**: Add audit logging for safety-relevant events and administrative actions.

### Domain and Reliability

- [ ] **Valid**: Fix `Outline` constructor: `Vertices` is assigned before the null guard, and assigned twice.
- [ ] **Partial**: Add validation for remaining domain invariants. Guards and options validators exist, but validation is not complete across the model.
- [ ] **Missing**: Review hazard-zone timer/state transition synchronization before production-like use.
- [ ] **Missing**: Add database schema evolution strategy, including migrations/versioning and indexes for movement history queries.

## P2 - Product Features

### Person Tracking

- [ ] **Partial**: Implement person history tracking beyond single movement registration lookup.
- [ ] **Missing**: Add configurable timeout per person type.

### Floor Management

- [ ] **Missing**: Add floor capacity management.
- [ ] **Missing**: Implement floor access control.
- [ ] **Partial**: Add floor occupancy reporting. The domain tracks persons internally, but there is no reporting model or endpoint.

### Hazard Zone Management

- [ ] **Missing**: Add zone priority levels.
- [ ] **Missing**: Implement zone scheduling with active hours.
- [ ] **Missing**: Add zone dependency management for linked zones.
- [ ] **Partial**: Add full hazard-zone management endpoints. `GET`, `activate`, and `deactivate` exist; CRUD/configuration management does not.

### Configuration and Integrations

- [ ] **Partial**: Add dynamic floor and zone configuration. Startup/appsettings configuration exists, but there is no runtime configuration workflow.
- [ ] **Partial**: Add external sensor and alarm-system integration. Domain hooks exist for external activation, but no adapter/API integration exists.
- [ ] **Missing**: Add notification handling for alarm transitions.
- [ ] **Missing**: Add analytics for person movement patterns and statistics.
- [ ] **Missing**: Add real-time WebSocket or SignalR updates.

## P3 - API Maturity

- [ ] **Missing**: Add API versioning infrastructure. Endpoints use `/api/v1/` prefixes, but there is no versioning strategy.
- [ ] **Valid**: Add floor management CRUD endpoints. Only `GET /api/v1/floors` exists.
- [ ] **Partial**: Expand API documentation. OpenAPI/Scalar and README docs exist, but feature-level behavior and operational contracts are incomplete.

## P4 - Documentation and Future Hardening

- [ ] **Missing**: Create Architecture Decision Records.
- [ ] **Missing**: Create a Feature Catalog.
- [ ] **Partial**: Create a developer onboarding guide. README getting-started steps exist, but no dedicated onboarding guide exists.
- [ ] **Missing**: Add sequence diagrams for key workflows. A hazard-zone state diagram exists, but no workflow sequence diagrams exist.
- [ ] **Missing**: Implement domain event versioning when events need to leave the process or be persisted.
- [ ] **Missing**: Add performance benchmarks.
- [ ] **Missing**: Add load testing scenarios.

## Additional Gaps Found During Review

- [ ] **Missing**: Complete or revisit the `Site` domain model. It validates floors but does not appear to store/expose them meaningfully, while `GET /api/v1/site` is configuration DTO driven.
- [ ] **Missing**: Implement real `SiteOptionsValidator` validation instead of always returning success.
