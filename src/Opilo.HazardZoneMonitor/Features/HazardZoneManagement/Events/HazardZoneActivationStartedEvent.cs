using Opilo.HazardZoneMonitor.Shared.Abstractions;

namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Events;

public record HazardZoneActivationStartedEvent(string HazardZoneName) : IDomainEvent;
