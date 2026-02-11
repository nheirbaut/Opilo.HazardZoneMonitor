# Development Items

An overview of tasks and features to be implemented.

## Build

- [ ] Add a GitHub build script.
- [ ] Add continuous integration pipeline
- [ ] Add code coverage reporting

## Domain

- [ ] Introduce ValueObjects to avoid the Primitive Obsession code smell.
- [ ] Add validation for domain invariants
- [ ] Implement domain event versioning for future compatibility

### Bugs

- [ ] Fix `Outline` constructor: `Vertices` is assigned before the null guard, and assigned twice.

## Architecture

- [ ] Wire the API layer to the domain model. Currently `RegisterPersonMovement` writes directly to SQLite via Dapper and never touches the domain (`Floor`, `Person`, `HazardZone`). The rich domain model is unreachable from the API. All feature work (hazard zone endpoints, notifications, floor management) is blocked by this gap.

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
- [ ] Add correlation/request ID tracking for traceability across logs.

### Root

- [ ] Determine available links when calling "/" dynamically so that they are not hardcoded.

### Endpoints

- [ ] Floor management endpoints (only `GET /api/v1/floors` exists, no CRUD).
- [ ] HazardZone management endpoints (none exist).
- [ ] Real-time WebSocket updates

## Tests

- [ ] Add missing API unit tests for `GetPersonMovements` and `GetFloors` handlers (only `RegisterPersonMovement` has a handler unit test).
- [ ] Add performance benchmarks
- [ ] Add load testing scenarios

## Documentation

- [ ] Create Architecture Decision Records
- [ ] Create Feature Catalog
- [ ] Add API documentation
- [ ] Create developer onboarding guide
- [ ] Add sequence diagrams for key workflows
