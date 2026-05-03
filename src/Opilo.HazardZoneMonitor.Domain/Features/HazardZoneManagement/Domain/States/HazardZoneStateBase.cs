using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Opilo.HazardZoneMonitor.Domain.Shared.ValueObjects;

namespace Opilo.HazardZoneMonitor.Domain.Features.HazardZoneManagement.Domain.States;

internal abstract class HazardZoneStateBase(
    HazardZone hazardZone,
    HashSet<PersonId> personsInZone,
    HashSet<string> registeredActivationSourceIds,
    int allowedNumberOfPersons) : IDisposable
{
    public abstract ZoneState ZoneState { get; }
    public abstract AlarmState AlarmState { get; }
    public int AllowedNumberOfPersons { get; private set; } = allowedNumberOfPersons;

    protected HazardZone HazardZone => hazardZone;
    protected HashSet<PersonId> PersonsInZone => personsInZone;
    protected readonly HashSet<string> RegisteredActivationSourceIds = registeredActivationSourceIds;

    public void SetAllowedNumberOfPersons(int allowedNumberOfPersons)
    {
        AllowedNumberOfPersons = allowedNumberOfPersons;
        OnAllowedNumberOfPersonsChanged();
    }

    public void OnPersonAddedToHazardZone(PersonId personId)
    {
        if (PersonsInZone.Add(personId))
            HazardZone.RaisePersonAddedToHazardZone(personId);

        OnPersonAddedToHazardZone();
    }

    public void OnPersonRemovedFromHazardZone(PersonId personId)
    {
        if (PersonsInZone.Remove(personId))
            HazardZone.RaisePersonRemovedFromHazardZone(personId);

        OnPersonRemovedFromHazardZone();
    }

    public void OnPersonChangedLocation(PersonId personId, Location location)
    {
        if (PersonsInZone.Contains(personId))
        {
            if (HazardZone.Outline.IsLocationInside(location))
                return;

            OnPersonRemovedFromHazardZone(personId);
        }

        if (!HazardZone.Outline.IsLocationInside(location))
            return;

        OnPersonAddedToHazardZone(personId);
    }

    protected virtual void OnPersonAddedToHazardZone()
    {
    }

    protected virtual void OnPersonRemovedFromHazardZone()
    {
    }

    public virtual void ManuallyActivate()
    {
    }

    public virtual void ManuallyDeactivate()
    {
    }

    public virtual void ActivateFromExternalSource(string sourceId)
    {
    }

    public virtual void DeactivateFromExternalSource(string sourceId)
    {
    }

    public virtual void OnPreAlarmTimerElapsed()
    {
    }

    protected virtual void OnAllowedNumberOfPersonsChanged()
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}

