using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Domain.States;

internal sealed class AlarmHazardZoneState : HazardZoneStateBase
{
    public AlarmHazardZoneState(
        HazardZone hazardZone,
        HashSet<PersonId> personsInZone,
        HashSet<SourceId> registeredActivationSourceIds,
        Capacity allowedNumberOfPersons)
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

    public override void DeactivateFromExternalSource(SourceId sourceId)
    {
        if (!RegisteredActivationSourceIds.Remove(sourceId))
            return;

        HazardZone.TransitionTo(new InactiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    protected override void OnPersonRemovedFromHazardZone()
    {
        if (PersonsInZone.Count > AllowedNumberOfPersons.Value)
            return;

        HazardZone.RaiseHazardZoneAlarmStateChanged(AlarmState.None);
        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    protected override void OnAllowedNumberOfPersonsChanged()
    {
        if (PersonsInZone.Count <= AllowedNumberOfPersons.Value)
        {
            HazardZone.RaiseHazardZoneAlarmStateChanged(AlarmState.None);
            HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
                AllowedNumberOfPersons));
        }
    }
}
