using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain.States;

internal sealed class AlarmHazardZoneState : HazardZoneStateBase
{
    public AlarmHazardZoneState(
        HazardZone hazardZone,
        HashSet<Guid> personsInZone,
        HashSet<string> registeredActivationSourceIds,
        int allowedNumberOfPersons)
        : base(hazardZone, personsInZone, registeredActivationSourceIds, allowedNumberOfPersons)
    {
        HazardZone.RaiseHazardZoneAlarmStateChanged(AlarmState.Alarm);
    }

    public override ZoneState ZoneState => ZoneState.Active;
    public override AlarmState AlarmState => AlarmState.Alarm;

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
}
