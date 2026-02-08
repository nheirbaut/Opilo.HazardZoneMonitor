// ReSharper disable UnusedTypeParameter
#pragma warning disable S2326 // Phantom type parameter used for handler type constraints
namespace Opilo.HazardZoneMonitor.Api.Shared.Cqrs;

public interface ICommand<TResponse> where TResponse : IResponse;
