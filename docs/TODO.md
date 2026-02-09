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

- [ ] Discover endpoints.

### Root

- [ ] Implement an API Root Resource response.

### Endpoints

- [ ] Person tracking endpoints
  - [ ] Create GET endpoint to fetch a person movement registration by ID (GET /api/v1/person-movements/{personId})
- [ ] Floor management endpoints
- [ ] HazardZone management endpoints
- [ ] Real-time WebSocket updates

## Tests

- [x] Move to xUnit v3.
- [ ] Add integration tests for feature workflows
- [ ] Add performance benchmarks
- [ ] Add load testing scenarios

## Documentation

- [ ] Create Architecture Decision Records
- [ ] Create Feature Catalog
- [x] Update README with architecture overview
- [ ] Add API documentation
- [ ] Create developer onboarding guide
- [ ] Add sequence diagrams for key workflows
