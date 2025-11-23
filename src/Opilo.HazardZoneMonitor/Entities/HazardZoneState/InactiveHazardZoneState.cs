using Opilo.HazardZoneMonitor.Enums;

namespace Opilo.HazardZoneMonitor.Entities.HazardZoneState;

internal sealed class InactiveHazardZoneState(
    HazardZone hazardZone,
    HashSet<Guid> personsInZone,
    HashSet<string> registeredActivationSourceIds,
    int allowedNumberOfPersons)
    : HazardZoneStateBase(hazardZone, personsInZone, registeredActivationSourceIds, allowedNumberOfPersons)
{
    public override bool IsActive => false;
    public override AlarmState AlarmState => AlarmState.None;

    public override void ManuallyActivate()
    {
        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    public override void ActivateFromExternalSource(string sourceId)
    {
        if (!RegisteredActivationSourceIds.Add(sourceId))
            return;

        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }
}
