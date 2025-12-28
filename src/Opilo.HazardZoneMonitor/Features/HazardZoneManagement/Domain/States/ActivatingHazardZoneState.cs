using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain.States;

internal sealed class ActivatingHazardZoneState : HazardZoneStateBase
{
    private readonly Opilo.HazardZoneMonitor.Shared.Abstractions.ITimer? _activationTimer;
    private readonly DateTime _enteredActivatingAtUtc;

    public ActivatingHazardZoneState(
        HazardZone hazardZone,
        HashSet<Guid> personsInZone,
        HashSet<string> registeredActivationSourceIds,
        int allowedNumberOfPersons)
        : base(hazardZone, personsInZone, registeredActivationSourceIds, allowedNumberOfPersons)
    {
        if (HazardZone.ActivationDuration == TimeSpan.Zero)
        {
            HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
                AllowedNumberOfPersons));
            return;
        }

        HazardZone.RaiseHazardZoneActivationStarted();

        _enteredActivatingAtUtc = HazardZone.Clock.UtcNow;

        _activationTimer = HazardZone.TimerFactory.Create(HazardZone.ActivationDuration);
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

    public override void DeactivateFromExternalSource(string sourceId)
    {
        if (!RegisteredActivationSourceIds.Remove(sourceId))
            return;

        HazardZone.TransitionTo(new InactiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    private void OnActivationTimerElapsed(object? _, EventArgs __)
    {
        if (HazardZone.Clock.UtcNow < _enteredActivatingAtUtc.Add(HazardZone.ActivationDuration))
            return;

        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    protected override void Dispose(bool disposing)
    {
        if (_activationTimer != null)
        {
            _activationTimer.Stop();
            _activationTimer.Elapsed -= OnActivationTimerElapsed;
            _activationTimer.Dispose();
        }

        base.Dispose(disposing);
    }
}
