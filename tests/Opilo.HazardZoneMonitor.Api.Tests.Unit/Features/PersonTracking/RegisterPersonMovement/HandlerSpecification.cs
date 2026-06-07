using Ardalis.Result;
using NSubstitute;
using Opilo.HazardZoneMonitor.Api.Features.Floors.Services;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.Data;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.GetRegisteredPersonMovement;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;
using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;
using Handler = Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement.Handler;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.PersonTracking.RegisterPersonMovement;

public sealed class HandlerSpecification
{
    private readonly IMovementsRepository _movementsRepository;
    private readonly IClock _clock;
    private readonly Handler _sut;

    public HandlerSpecification()
    {
        _movementsRepository = Substitute.For<IMovementsRepository>();
        _clock = Substitute.For<IClock>();
        var floorService = Substitute.For<IFloorService>();
        _sut = new Handler(_movementsRepository, _clock, floorService);
    }

    [Fact]
    public async Task Handle_ShouldReturnCreatedResult_WhenMovementIsRegisteredSuccessfully()
    {
        // Arrange
        var personId = PersonId.From(Guid.NewGuid());
        var coordinate = new Coordinate(1.0, 2.0);
        Command command = new(personId, coordinate);
        RegisteredPersonMovement expectedMovement = new()
        {
            PersonId = personId,
            Coordinate = coordinate,
            RegisteredAt = DateTime.UtcNow,
        };

        _movementsRepository
            .RegisterMovementAsync(personId, coordinate, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created(expectedMovement));

        // Act
        Result<RegisteredPersonMovement> result = await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.Created);
        result.Value.Should().Be(expectedMovement);
    }

    [Fact]
    public async Task Handle_ShouldPassClockUtcNow_AsRegisteredAtToRepository()
    {
        // Arrange
        DateTime fixedTime = new(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        _clock.UtcNow.Returns(fixedTime);

        var personId = PersonId.From(Guid.NewGuid());
        var coordinate = new Coordinate(1.0, 2.0);
        Command command = new(personId, coordinate);

        _movementsRepository
            .RegisterMovementAsync(personId, Arg.Any<Coordinate>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created(new RegisteredPersonMovement
            {
                PersonId = personId,
                Coordinate = coordinate,
                RegisteredAt = fixedTime,
            }));

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await _movementsRepository.Received(1)
            .RegisterMovementAsync(personId, coordinate, fixedTime, Arg.Any<CancellationToken>());
    }
}
