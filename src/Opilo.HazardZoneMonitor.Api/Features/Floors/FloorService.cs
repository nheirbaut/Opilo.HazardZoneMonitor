using System.Collections.ObjectModel;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.Floors.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.Floors.Services;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Services;
using Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Events;
using Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors;

public sealed class FloorService : IFloorService, IDisposable
{
    private readonly Dictionary<FloorName, Floor> _floors = new();
    private readonly IHazardZoneService _hazardZoneService;

    public FloorService(IOptions<FloorOptions> options, IHazardZoneService hazardZoneService, IClock clock, ITimerFactory timerFactory)
    {
        _hazardZoneService = hazardZoneService;

        foreach (var config in options.Value.Floors)
        {
            var outline = new Outline(new ReadOnlyCollection<Coordinate>(config.Outline.ToList()));

            var hazardZones = config.HazardZones.Select(hz =>
            {
                var hzOutline = new Outline(new ReadOnlyCollection<Coordinate>(hz.Outline.ToList()));
                var zone = new HazardZone(
                    hz.Name,
                    hzOutline,
                    Duration.From(hz.ActivationDuration),
                    Duration.From(hz.PreAlarmDuration),
                    clock,
                    timerFactory);
                zone.SetAllowedNumberOfPersons(Capacity.From(hz.AllowedNumberOfPersons));
                return zone;
            }).ToList();

            var floor = new Floor(config.Name, outline, hazardZones, timerFactory: timerFactory);
            floor.PersonAddedToFloor += OnPersonAddedToFloor;
            floor.PersonRemovedFromFloor += OnPersonRemovedFromFloor;
            _floors[config.Name] = floor;
        }
    }

    public void ApplyPersonLocationUpdate(PersonLocationUpdate personLocationUpdate)
    {
        foreach (var floor in _floors.Values)
        {
            floor.TryAddPersonLocationUpdate(personLocationUpdate);
        }
    }

    private void OnPersonAddedToFloor(object? sender, PersonAddedToFloorEventArgs e)
    {
        _hazardZoneService.ApplyPersonLocationUpdate(
            new PersonLocationUpdate(e.PersonId, e.Location));
    }

    private void OnPersonRemovedFromFloor(object? sender, PersonRemovedFromFloorEventArgs e)
    {
        _hazardZoneService.RemovePerson(e.PersonId);
    }

    public void Dispose()
    {
        foreach (var floor in _floors.Values)
        {
            floor.PersonAddedToFloor -= OnPersonAddedToFloor;
            floor.PersonRemovedFromFloor -= OnPersonRemovedFromFloor;
            floor.Dispose();
        }

        _floors.Clear();
    }
}
