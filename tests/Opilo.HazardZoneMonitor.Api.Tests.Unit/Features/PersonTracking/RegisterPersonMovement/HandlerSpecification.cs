using Ardalis.Result;
using NSubstitute;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;
using Opilo.HazardZoneMonitor.Domain.Shared.Abstractions;

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
        _sut = new Handler(_movementsRepository, _clock);
    }

    [Fact]
    public async Task Handle_ShouldReturnCreatedResult_WhenMovementIsRegisteredSuccessfully()
    {
        // Arrange
        Guid personId = Guid.NewGuid();
        double x = 1.0;
        double y = 2.0;
        Command command = new(personId, x, y);
        RegisteredPersonMovement expectedMovement = new()
        {
            PersonId = personId,
            X = x,
            Y = y,
            RegisteredAt = DateTime.UtcNow,
        };

        _movementsRepository
            .RegisterMovementAsync(personId, x, y, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
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

        var personId = Guid.NewGuid();
        Command command = new(personId, 1.0, 2.0);

        _movementsRepository
            .RegisterMovementAsync(Arg.Any<Guid>(), Arg.Any<double>(), Arg.Any<double>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created(new RegisteredPersonMovement
            {
                PersonId = personId,
                X = 1.0,
                Y = 2.0,
                RegisteredAt = fixedTime,
            }));

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await _movementsRepository.Received(1)
            .RegisterMovementAsync(personId, 1.0, 2.0, fixedTime, Arg.Any<CancellationToken>());
    }
}
