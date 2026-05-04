using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Domain.States;

internal sealed class ActivatingHazardZoneState : HazardZoneStateBase
{
    private readonly Opilo.HazardZoneMonitor.Domain.Shared.Abstractions.ITimer _activationTimer;
    private readonly Timestamp _enteredActivatingAtUtc;

    public ActivatingHazardZoneState(
        HazardZone hazardZone,
        HashSet<PersonId> personsInZone,
        HashSet<SourceId> registeredActivationSourceIds,
        Capacity allowedNumberOfPersons)
        : base(hazardZone, personsInZone, registeredActivationSourceIds, allowedNumberOfPersons)
    {
        HazardZone.RaiseHazardZoneStateChanged(ZoneState.Activating);

        _enteredActivatingAtUtc = new Timestamp(HazardZone.Clock.UtcNow);

        _activationTimer = HazardZone.TimerFactory.Create(HazardZone.ActivationDuration.Value);
        _activationTimer.Elapsed += OnActivationTimerElapsed;
        _activationTimer.Start();
    }

    public override ZoneState ZoneState => ZoneState.Activating;
    public override AlarmState AlarmState => AlarmState.None;

    public override void ManuallyDeactivate()
    {
        HazardZone.TransitionTo(new InactiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    public override void DeactivateFromExternalSource(SourceId sourceId)
    {
        if (!RegisteredActivationSourceIds.Remove(sourceId))
            return;

        HazardZone.TransitionTo(new InactiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    private void OnActivationTimerElapsed(object? _, EventArgs __)
    {
        if (HazardZone.Clock.UtcNow < _enteredActivatingAtUtc.Value.Add(HazardZone.ActivationDuration.Value))
            return;

        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    protected override void Dispose(bool disposing)
    {
        _activationTimer.Stop();
        _activationTimer.Elapsed -= OnActivationTimerElapsed;
        _activationTimer.Dispose();

        base.Dispose(disposing);
    }
}
