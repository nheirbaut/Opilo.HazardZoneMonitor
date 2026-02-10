using Ardalis.Result;
using NSubstitute;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.RegisterPersonMovement;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.PersonTracking.RegisterPersonMovement;

public sealed class HandlerSpecification
{
    private readonly IMovementsRepository _movementsRepository;
    private readonly Handler _sut;

    public HandlerSpecification()
    {
        _movementsRepository = Substitute.For<IMovementsRepository>();
        _sut = new Handler(_movementsRepository);
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
    public async Task Handle_ShouldCallRepositoryWithCorrectParameters_WhenHandled()
    {
        // Arrange
        Guid personId = Guid.NewGuid();
        double x = 3.5;
        double y = 7.2;
        Command command = new(personId, x, y);

        _movementsRepository
            .RegisterMovementAsync(personId, x, y, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(Result.Created(new RegisteredPersonMovement
            {
                PersonId = personId,
                X = x,
                Y = y,
                RegisteredAt = DateTime.UtcNow,
            }));

        // Act
        await _sut.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await _movementsRepository
            .Received(1)
            .RegisterMovementAsync(personId, x, y, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }
}
