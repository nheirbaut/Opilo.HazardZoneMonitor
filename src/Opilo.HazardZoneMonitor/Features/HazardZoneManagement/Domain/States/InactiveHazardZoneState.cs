using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain.States;

internal sealed class InactiveHazardZoneState(
    HazardZone hazardZone,
    HashSet<Guid> personsInZone,
    HashSet<string> registeredActivationSourceIds,
    int allowedNumberOfPersons)
    : HazardZoneStateBase(hazardZone, personsInZone, registeredActivationSourceIds, allowedNumberOfPersons)
{
    public override ZoneState ZoneState => ZoneState.Inactive;
    public override AlarmState AlarmState => AlarmState.None;

    public override void ManuallyActivate()
    {
        if (HazardZone.ActivationDuration > TimeSpan.Zero)
        {
            HazardZone.TransitionTo(new ActivatingHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
                AllowedNumberOfPersons));
            return;
        }

        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    public override void ActivateFromExternalSource(string sourceId)
    {
        if (!RegisteredActivationSourceIds.Add(sourceId))
            return;

        if (HazardZone.ActivationDuration > TimeSpan.Zero)
        {
            HazardZone.TransitionTo(new ActivatingHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
                AllowedNumberOfPersons));
            return;
        }

        HazardZone.TransitionTo(new ActiveHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }
}
