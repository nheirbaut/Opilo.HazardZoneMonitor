# Health Endpoint & Startup Validation Analysis

## Context

Investigation into how the application should handle two concerns:

1. **Runtime health monitoring** — is the app healthy right now?
2. **Startup validation** — should the app even start with this configuration?

## Key Finding: Two Different Mechanisms

These are distinct concerns that require different solutions.

### Startup Validation (fail fast)

**Mechanism**: `IValidateOptions<T>` + `ValidateOnStart()`

Configuration is static and known at startup. If floor/zone definitions are invalid, the app should refuse to start — not start and then report "unhealthy". Invalid config will never self-heal at runtime.

The domain already enforces all the relevant business rules:

| Rule | Enforced by | Location |
|------|-------------|----------|
| Outline must have >= 3 vertices | `Outline` constructor | `Shared/Primitives/Outline.cs` |
| Floor name cannot be empty | `Floor` constructor (`Guard.Against.NullOrWhiteSpace`) | `FloorManagement/Domain/Floor.cs` |
| Zone name cannot be empty | `HazardZone` constructor (`Guard.Against.NullOrWhiteSpace`) | `HazardZoneManagement/Domain/HazardZone.cs` |
| No duplicate zone names within a floor | `Floor` constructor (`Guard.Against.DuplicateHazardZones`) | `Shared/Guards/HazardZoneGuards.cs` |
| No overlapping zones within a floor | `Floor` constructor (`Guard.Against.OverlappingHazardZones`) | `Shared/Guards/HazardZoneGuards.cs` |
| All zones must be within floor outline | `Floor` constructor (`Guard.Against.HazardZonesOutsideFloor`) | `Shared/Guards/HazardZoneGuards.cs` |
| Activation/PreAlarm durations non-negative | `HazardZone` constructor (`Guard.Against.Negative`) | `HazardZoneManagement/Domain/HazardZone.cs` |

**Implementation approach**: The `IValidateOptions<T>` validator should attempt to construct the domain objects from configuration. If the domain constructors throw, the validator catches the exceptions and translates them into validation failure messages. This keeps the domain as the single source of truth for business rules — no duplication.

```
appsettings.json → FloorOptions / HazardZoneOptions
    → IValidateOptions<T> tries to build domain objects
        → Domain constructors enforce all rules
            → Success: app starts
            → Failure: app refuses to start with clear error messages
```

### Runtime Health Checks (monitor ongoing health)

**Mechanism**: ASP.NET Core built-in health checks (`AddHealthChecks()` / `MapHealthChecks()`)

For checking things that can change at runtime: database connectivity, disk space, external service availability. Not relevant for configuration validation.

## Gap: Duplicate Floor Names

The domain enforces uniqueness of zone names **within a floor** (via `Floor` constructor + `Guard.Against.DuplicateHazardZones`). However, nothing enforces uniqueness of **floor names across the system**.

This matters because floor names are used as identifiers in events (`PersonAddedToFloorEventArgs(Name, ...)`) and API responses. Duplicates would cause ambiguity.

### Resolution

A `Site` aggregate root should own the collection of floors and enforce system-wide invariants — mirroring how `Floor` already owns `HazardZone`s.

**Tracked in**: [GitHub Issue #35](https://github.com/nheirbaut/Opilo.HazardZoneMonitor/issues/35)

## Domain Model Summary (Current State)

```
Site (does not exist yet — proposed)
 └── Floor (aggregate root for zones)
      ├── Name (string, non-empty)
      ├── Outline (>= 3 vertices)
      ├── HazardZone[] (validated by Floor constructor)
      │    ├── Name (string, non-empty, unique within floor)
      │    ├── Outline (>= 3 vertices, within floor, non-overlapping)
      │    ├── ActivationDuration (>= 0)
      │    ├── PreAlarmDuration (>= 0)
      │    └── State machine (Inactive → Activating → Active, alarm escalation)
      └── Person[] (runtime, managed via events)
```

## Aggregate Boundaries

| Entity | Role | Owns | Validates |
|--------|------|------|-----------|
| `Site` (proposed) | Aggregate root | `Floor[]` | Unique floor names |
| `Floor` | Aggregate root for zones | `HazardZone[]`, `Person[]` | Duplicate zone names, overlapping zones, zones within floor |
| `HazardZone` | Entity within Floor | State machine, person tracking | Own construction invariants |
| `Person` | Entity within Floor | Expiry timer | Own construction invariants |
| `Outline` | Value object | `Location[]` vertices | >= 3 vertices |

## Configuration Pipeline (Current vs Proposed)

### Current

```
appsettings.json
    → FloorOptions (AddOptions + BindConfiguration)
    → HazardZoneOptions (AddOptions + BindConfiguration)
    → No startup validation
    → Domain objects constructed lazily (on first request?)
    → Invalid config discovered at runtime
```

### Proposed

```
appsettings.json
    → SiteOptions (or FloorOptions + HazardZoneOptions)
    → ValidateOnStart() triggers IValidateOptions<T>
    → Validator constructs Site → Floor[] → HazardZone[] (domain objects)
    → Domain constructors enforce ALL business rules
    → Invalid config → app refuses to start with clear error
    → Valid config → domain objects available for injection
```
