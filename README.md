# Opilo HazardZone Monitor

**Opilo HazardZone Monitor** is a safety application designed to monitor and alert when individuals are present in restricted or hazardous areas. This project serves dual purposes:
- **Primary Goal:** Enhance safety by providing real-time monitoring and alerts for unauthorized or dangerous zone entries.
- **Learning Objective:** To explore and learn modern web technologies including Blazor, React, and Microsoft Aspire, Kubernetes and KubeEdge.

## Architecture

This project implements a **Vertical Slice Architecture** with **DDD** and **CQRS**, organizing code by features rather than technical layers. Each feature is self-contained with its own domain models, events, and business logic.

**Stack:** .NET 10, Minimal API, Dapper + Sqlite, Serilog

### Structure

```
src/
├── Opilo.HazardZoneMonitor.Domain/          # Rich domain models, events, state machine
│   ├── Features/
│   │   ├── PersonTracking/                  # Person lifecycle and location tracking
│   │   │   ├── Domain/
│   │   │   │   └── Person.cs
│   │   │   └── Events/
│   │   ├── FloorManagement/                 # Floor occupancy management
│   │   │   ├── Domain/
│   │   │   │   └── Floor.cs
│   │   │   └── Events/
│   │   └── HazardZoneManagement/            # Hazard zone monitoring with alarm states
│   │       ├── Domain/
│   │       │   ├── HazardZone.cs
│   │       │   └── States/                  # State pattern for alarm management
│   │       └── Events/
│   └── Shared/                              # Shared primitives and infrastructure
│       ├── Abstractions/                    # IClock, ITimer, ITimerFactory
│       ├── Guards/
│       ├── Primitives/                      # Location, Outline, ZoneState, AlarmState
│       └── Time/                            # System implementations
├── Opilo.HazardZoneMonitor.Api/             # Minimal API, vertical slices, CQRS handlers
│   ├── Features/
│   │   ├── Floors/                          # Floor configuration and queries
│   │   └── PersonTracking/                  # Person movement endpoints
│   │       ├── RegisterPersonMovement/      # Feature.cs, Handler.cs, Command.cs, Response.cs
│   │       └── GetPersonMovements/          # Feature.cs, Handler.cs, Query.cs, Response.cs
│   └── Shared/
│       ├── Cqrs/                            # ICommandHandler, IQueryHandler, ICommand, IQuery
│       └── Features/                        # IFeature, auto-discovery
tests/
├── Opilo.HazardZoneMonitor.Domain.Tests.Unit/
├── Opilo.HazardZoneMonitor.Api.Tests.Unit/
└── Opilo.HazardZoneMonitor.Tests.Integration/
```

### Key Architectural Principles

- **Feature Cohesion:** All code for a feature lives together
- **Independent Evolution:** Features can evolve independently
- **Clear Boundaries:** Features communicate through well-defined events
- **Domain-Driven Design:** Rich domain models with behavior — no anemic models
- **Event-Driven:** Features coordinate through standard .NET domain events
- **CQRS:** Separate command and query handlers — no MediatR

### Feature Interactions

- **PersonTracking** → Raises person lifecycle events (created, expired, location changed)
- **FloorManagement** → Listens to PersonTracking, manages floor occupancy
- **HazardZoneManagement** → Listens to PersonTracking, manages hazard zones with a state machine for zone activation (Inactive → Activating → Active) and alarm escalation (None → PreAlarm → Alarm)

## Getting Started

### Prerequisites

- .NET 10 SDK

### Installation

1. **Clone the repository:**
   ```sh
   git clone https://github.com/nheirbaut/Opilo.HazardZoneMonitor.git
   ```
2. **Navigate to the project folder:**
   ```sh
   cd Opilo.HazardZoneMonitor
   ```
3. **Restore dependencies:**
   ```sh
   dotnet restore
   ```
4. **Build:**
   ```sh
   dotnet build
   ```
5. **Run the API:**
   ```sh
   dotnet run --project src/Opilo.HazardZoneMonitor.Api
   ```
6. **Run tests:**
   ```sh
   dotnet test
   ```

## Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

Distributed under the GNU Affero General Public License. See `LICENSE` for more information.

## Contact

Niels Heirbaut - [hazardzone@opilo.nl](mailto:hazardzone@opilo.nl)

Project Link: https://github.com/nheirbaut/Opilo.HazardZoneMonitor
