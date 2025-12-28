using Opilo.HazardZoneMonitor.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Features.HazardZoneManagement.Domain.States;

internal sealed class ActiveHazardZoneState : HazardZoneStateBase
{
    public ActiveHazardZoneState(
        HazardZone hazardZone,
        HashSet<Guid> personsInZone,
        HashSet<string> registeredActivationSourceIds,
        int allowedNumberOfPersons)
        : base(hazardZone, personsInZone, registeredActivationSourceIds, allowedNumberOfPersons)
    {
        HazardZone.RaiseHazardZoneStateChanged(ZoneState.Active);
        HazardZone.RaiseHazardZoneAlarmStateChanged(AlarmState.None);
    }

    public override ZoneState ZoneState => ZoneState.Active;
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

    protected override void OnPersonAddedToHazardZone()
    {
        if (PersonsInZone.Count <= AllowedNumberOfPersons)
            return;

        if (HazardZone.PreAlarmDuration == TimeSpan.Zero)
        {
            HazardZone.TransitionTo(new AlarmHazardZoneState(HazardZone, PersonsInZone,
                RegisteredActivationSourceIds,
                AllowedNumberOfPersons));

            return;
        }

        HazardZone.TransitionTo(new PreAlarmHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
            AllowedNumberOfPersons));
    }

    protected override void OnAllowedNumberOfPersonsChanged()
    {
        if (PersonsInZone.Count > AllowedNumberOfPersons)
        {
            if (HazardZone.PreAlarmDuration == TimeSpan.Zero)
            {
                HazardZone.TransitionTo(new AlarmHazardZoneState(HazardZone, PersonsInZone,
                    RegisteredActivationSourceIds,
                    AllowedNumberOfPersons));

                return;
            }

            HazardZone.TransitionTo(new PreAlarmHazardZoneState(HazardZone, PersonsInZone, RegisteredActivationSourceIds,
                AllowedNumberOfPersons));
        }
    }
}
