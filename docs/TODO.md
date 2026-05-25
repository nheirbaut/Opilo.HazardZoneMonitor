# Development Items

An overview of tasks and features to be implemented.

## Build

- [ ] Add a GitHub build script.
- [ ] Add continuous integration pipeline
- [ ] Add code coverage reporting

## Domain

- [ ] Add validation for domain invariants
- [ ] Implement domain event versioning for future compatibility

### Bugs

- [ ] Fix `Outline` constructor: `Vertices` is assigned before the null guard, and assigned twice.

## Critical Blockers

- [ ] Wire the API layer to the domain model. `RegisterPersonMovement` writes directly to SQLite via Dapper and never touches `Floor`, `Person`, or `HazardZone`. All feature work is blocked until this gap is closed.

## Features

### PersonTracking

- [ ] Implement person history tracking
- [ ] Add configurable timeout per person type

### FloorManagement

- [ ] Add floor capacity management
- [ ] Implement floor access control
- [ ] Add floor occupancy reporting

### HazardZoneManagement

- [ ] Add zone priority levels
- [ ] Implement zone scheduling (active hours)
- [ ] Add zone dependency management (linked zones)

### New Features

- [ ] NotificationManagement: Handle alarm notifications
- [ ] AuditLogging: Track all system events
- [ ] Analytics: Person movement patterns and statistics
- [ ] Configuration: Dynamic zone and floor configuration
- [ ] Integration: External sensor and alarm system integration

## API

- [ ] Add health check endpoint (`/health`) for container/orchestration readiness (Kubernetes/KubeEdge).
- [ ] Add API versioning infrastructure (endpoints use `/api/v1/` prefix but no actual versioning strategy exists).
- [ ] Add structured error responses (RFC 9457 Problem Details) via global error handling middleware.
- [ ] Add request validation at the API boundary (commands accept arbitrary values with no validation before hitting the handler).
- [ ] Decide project-wide value-object validation convention so invalid `HazardZoneName` route values map consistently to 400 without duplicating endpoint guards.
- [ ] Add correlation/request ID tracking for traceability across logs.

### Endpoints

- [ ] Floor management endpoints (only `GET /api/v1/floors` exists, no CRUD).
- [ ] HazardZone management endpoints (`GET /api/v1/hazard-zones` exists, no CRUD).
- [ ] Real-time WebSocket updates

## Tests

- [ ] Add performance benchmarks
- [ ] Add load testing scenarios

## Documentation

- [ ] Create Architecture Decision Records
- [ ] Create Feature Catalog
- [ ] Add API documentation
- [ ] Create developer onboarding guide
- [ ] Add sequence diagrams for key workflows
