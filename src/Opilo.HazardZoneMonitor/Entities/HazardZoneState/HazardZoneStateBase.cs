using Opilo.HazardZoneMonitor.Enums;
using Opilo.HazardZoneMonitor.Events.HazardZoneEvents;
using Opilo.HazardZoneMonitor.Services;
using Opilo.HazardZoneMonitor.ValueObjects;

namespace Opilo.HazardZoneMonitor.Entities.HazardZoneState;

internal abstract class HazardZoneStateBase(
    HazardZone hazardZone,
    HashSet<Guid> personsInZone,
    HashSet<string> registeredActivationSourceIds,
    int allowedNumberOfPersons) : IDisposable
{
    public abstract bool IsActive { get; }
    public abstract AlarmState AlarmState { get; }
    public int AllowedNumberOfPersons { get; private set; } = allowedNumberOfPersons;

    protected HazardZone HazardZone => hazardZone;
    protected HashSet<Guid> PersonsInZone => personsInZone;
    protected readonly HashSet<string> RegisteredActivationSourceIds = registeredActivationSourceIds;

    public void SetAllowedNumberOfPersons(int allowedNumberOfPersons)
    {
        AllowedNumberOfPersons = allowedNumberOfPersons;
        OnAllowedNumberOfPersonsChanged();
    }

    public void OnPersonAddedToHazardZone(Guid personId)
    {
        if (PersonsInZone.Add(personId))
            DomainEvents.Raise(new PersonAddedToHazardZoneEvent(personId, HazardZone.Name));

        OnPersonAddedToHazardZone();
    }

    public void OnPersonRemovedFromHazardZone(Guid personId)
    {
        if (PersonsInZone.Remove(personId))
            DomainEvents.Raise(new PersonRemovedFromHazardZoneEvent(personId, HazardZone.Name));

        OnPersonRemovedFromHazardZone();
    }

    public void OnPersonChangedLocation(Guid personId, Location location)
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
