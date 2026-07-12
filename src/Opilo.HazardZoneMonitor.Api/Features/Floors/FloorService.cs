using System.Collections.ObjectModel;
using Microsoft.Extensions.Options;
using Opilo.HazardZoneMonitor.Api.Features.Floors.Configuration;
using Opilo.HazardZoneMonitor.Api.Features.Floors.Services;
using Opilo.HazardZoneMonitor.Api.Features.HazardZones.Services;
using Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Domain;
using Opilo.HazardZoneMonitor.Domain.Features.FloorManagement.Events;
using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.Floors;

public sealed class FloorService : IFloorService, IDisposable
{
    private readonly Dictionary<FloorName, Floor> _floors = new();
    private readonly IHazardZoneService _hazardZoneService;
    private volatile bool _disposed;

    public FloorService(IOptions<FloorOptions> options, IHazardZoneService hazardZoneService, ITimerFactory timerFactory)
    {
        _hazardZoneService = hazardZoneService;

        foreach (var config in options.Value.Floors)
        {
            var outline = new Outline(new ReadOnlyCollection<Coordinate>(config.Outline.ToList()));

            var floor = new Floor(config.Name, outline, timerFactory: timerFactory);
            floor.PersonAddedToFloor += OnPersonAddedToFloor;
            floor.PersonRemovedFromFloor += OnPersonRemovedFromFloor;
            floor.PersonLocationChanged += OnPersonLocationChanged;
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

    private void OnPersonLocationChanged(object? sender, PersonLocationChangedOnFloorEventArgs e)
    {
        _hazardZoneService.ApplyPersonLocationUpdate(
            new PersonLocationUpdate(e.PersonId, e.Location));
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var floor in _floors.Values)
        {
            floor.PersonAddedToFloor -= OnPersonAddedToFloor;
            floor.PersonRemovedFromFloor -= OnPersonRemovedFromFloor;
            floor.PersonLocationChanged -= OnPersonLocationChanged;
            floor.Dispose();
        }

        _floors.Clear();
    }
}
