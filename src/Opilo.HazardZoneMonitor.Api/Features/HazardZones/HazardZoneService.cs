using System.Collections.ObjectModel;
using Ardalis.Result;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Services;
using Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.HazardZones;

public sealed class HazardZoneService : IHazardZoneService, IDisposable
{
    private readonly Dictionary<HazardZoneName, HazardZone> _hazardZones = new();
    private bool _disposed;

    public HazardZoneService(IOptions<HazardZoneOptions> options, IClock clock, ITimerFactory timerFactory)
    {
        var configurations = options.Value.HazardZones;

        foreach (var config in configurations)
        {
            var outline = new Outline(new ReadOnlyCollection<Coordinate>(config.Outline.ToList()));
            var zone = new HazardZone(
                config.Name,
                outline,
                Duration.From(config.ActivationDuration),
                Duration.From(config.PreAlarmDuration),
                clock,
                timerFactory);

            zone.SetAllowedNumberOfPersons(Capacity.From(config.AllowedNumberOfPersons));

            _hazardZones[config.Name] = zone;
        }
    }

    public IReadOnlyList<HazardZoneInfo> GetHazardZones()
        => _hazardZones.Select(z => z.Value.ToHazardZoneInfo()).ToList();

    public Result ActivateHazardZone(HazardZoneName hazardZoneName)
    {
        if (!_hazardZones.TryGetValue(hazardZoneName, out var zone))
        {
            return Result.NotFound();
        }

        zone.ManuallyActivate();
        return Result.Success();
    }

    public Result DeactivateHazardZone(HazardZoneName hazardZoneName)
    {
        if (!_hazardZones.TryGetValue(hazardZoneName, out var zone))
        {
            return Result.NotFound();
        }

        zone.ManuallyDeactivate();
        return Result.Success();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var zone in _hazardZones.Values)
        {
            zone.Dispose();
        }

        _hazardZones.Clear();
    }
}
