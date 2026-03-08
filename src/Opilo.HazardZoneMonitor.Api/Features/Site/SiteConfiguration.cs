using Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Domain;

namespace Opilo.HazardZoneMonitor.Api.Features.Site;

public sealed record SiteConfiguration(string Name, IReadOnlyCollection<Floor> Floors);
