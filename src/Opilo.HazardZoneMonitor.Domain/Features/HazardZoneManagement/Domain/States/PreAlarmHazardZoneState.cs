using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Domain.States;

internal sealed class PreAlarmHazardZoneState : HazardZoneStateBase
{
    private readonly Opilo.HazardZoneMonitor.Domain.Shared.Abstractions.ITimer? _preAlarmTimer;
    private readonly DateTime _enteredPreAlarmAtUtc;

    public PreAlarmHazardZoneState(
        HazardZone hazardZone,
        HashSet<Guid> personsInZone,
        HashSet<string> registeredActivationSourceIds,
        int allowedNumberOfPersons)
        : base(hazardZone, personsInZone, registeredActivationSourceIds, allowedNumberOfPersons)
    {
        HazardZone.RaiseHazardZoneAlarmStateChanged(AlarmState.PreAlarm);

        if (HazardZone.PreAlarmDuration == TimeSpan.Zero)
        {
            HazardZone.OnPreAlarmTimerElapsed();
            return;
        }

        _enteredPreAlarmAtUtc = HazardZone.Clock.UtcNow;

        _preAlarmTimer = HazardZone.TimerFactory.Create(HazardZone.PreAlarmDuration);
        _preAlarmTimer.Elapsed += OnPreAlarmTimerElapsed;
        _preAlarmTimer.Start();
    }

    public override ZoneState ZoneState => ZoneState.Active;
    public override AlarmState AlarmState => AlarmState.PreAlarm;

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

    public override void OnPreAlarmTimerElapsed()
    {
        HazardZone.TransitionTo(new AlarmHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    protected override void OnPersonRemovedFromHazardZone()
    {
        if (PersonsInZone.Count > AllowedNumberOfPersons)
            return;

        HazardZone.RaiseHazardZoneAlarmStateChanged(AlarmState.None);
        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    protected override void OnAllowedNumberOfPersonsChanged()
    {
        if (PersonsInZone.Count <= AllowedNumberOfPersons)
        {
            HazardZone.RaiseHazardZoneAlarmStateChanged(AlarmState.None);
            HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
                AllowedNumberOfPersons));
        }
    }

    private void OnPreAlarmTimerElapsed(object? _, EventArgs __)
    {
        if (HazardZone.Clock.UtcNow < _enteredPreAlarmAtUtc.Add(HazardZone.PreAlarmDuration))
            return;

        HazardZone.OnPreAlarmTimerElapsed();
    }

    protected override void Dispose(bool disposing)
    {
        if (_preAlarmTimer != null)
        {
            _preAlarmTimer.Stop();
            _preAlarmTimer.Elapsed -= OnPreAlarmTimerElapsed;
            _preAlarmTimer.Dispose();
        }

        base.Dispose(disposing);
    }
}
