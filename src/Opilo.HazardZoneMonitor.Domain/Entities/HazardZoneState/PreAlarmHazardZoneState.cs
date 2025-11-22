using System.Timers;
using Opilo.HazardZoneMonitor.Domain.Enums;
using Timer = System.Timers.Timer;

namespace Opilo.HazardZoneMonitor.Domain.Entities.HazardZoneState;

internal sealed class PreAlarmHazardZoneState : HazardZoneStateBase
{
    private readonly Timer? _preAlarmTimer;

    public PreAlarmHazardZoneState(
        HazardZone hazardZone,
        HashSet<Guid> personsInZone,
        HashSet<string> registeredActivationSourceIds,
        int allowedNumberOfPersons)
        : base(hazardZone, personsInZone, registeredActivationSourceIds, allowedNumberOfPersons)
    {
        if (HazardZone.PreAlarmDuration == TimeSpan.Zero)
        {
            HazardZone.OnPreAlarmTimerElapsed();
            return;
        }

        _preAlarmTimer = new Timer(HazardZone.PreAlarmDuration);
        _preAlarmTimer.Elapsed += OnPreAlarmTimerElapsed;
        _preAlarmTimer.Start();
    }

    public override bool IsActive => true;
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

        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    protected override void OnAllowedNumberOfPersonsChanged()
    {
        if (PersonsInZone.Count <= AllowedNumberOfPersons)
            HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
                AllowedNumberOfPersons));
    }

    private void OnPreAlarmTimerElapsed(object? _, ElapsedEventArgs __)
    {
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
