namespace Opilo.HazardZoneMonitor.Core.Services;

public static class DomainEvents
{
    private static readonly List<Delegate> s_callBacks = [];

    public static void Register<T>(Action<T> callback) where T : IDomainEvent
    {
        s_callBacks.Add(callback);
    }

    public static void Raise<T>(T @event) where T : IDomainEvent
    {
        foreach (var callBack in s_callBacks)
        {
            if (callBack is Action<T> callBackForEventType)
            {
                callBackForEventType(@event);
            }
        }
    }
}
