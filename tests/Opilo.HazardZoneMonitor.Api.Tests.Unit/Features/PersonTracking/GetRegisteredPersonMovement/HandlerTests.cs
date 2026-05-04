using Ardalis.Result;
using NSubstitute;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.Data;
using Opilo.HazardZoneMonitor.Api.Features.PersonTracking.GetRegisteredPersonMovement;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Tests.Unit.Features.PersonTracking.GetRegisteredPersonMovement;

public sealed class HandlerTests
{
    private readonly IMovementsRepository _movementsRepository;
    private readonly Handler _sut;

    public HandlerTests()
    {
        _movementsRepository = Substitute.For<IMovementsRepository>();
        _sut = new Handler(_movementsRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResult_WhenRepositoryReturnsSuccessResult()
    {
        // Arrange
        var movementId = Guid.NewGuid();
        var personId = PersonId.From(Guid.NewGuid());
        var coordinate = new Coordinate(1.0, 2.0);
        var registeredAt = DateTime.UtcNow;
        Query query = new(movementId);
        RegisteredPersonMovement expectedMovement = new()
        {
            Id = movementId,
            PersonId = personId,
            Coordinate = coordinate,
            RegisteredAt = registeredAt,
        };

        _movementsRepository
            .GetMovementByIdAsync(movementId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedMovement));

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.Ok);
        result.Value.Should().Be(expectedMovement);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFoundResult_WhenRepositoryReturnsNotFoundResult()
    {
        // Arrange
        var movementId = Guid.NewGuid();
        Query query = new(movementId);

        _movementsRepository
            .GetMovementByIdAsync(movementId, Arg.Any<CancellationToken>())
            .Returns(Result.NotFound());

        // Act
        var result = await _sut.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldPassQueryIdAndCancellationToken_ToRepository()
    {
        // Arrange
        var movementId = Guid.NewGuid();
        Query query = new(movementId);
        var cancellationToken = TestContext.Current.CancellationToken;

        _movementsRepository
            .GetMovementByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.NotFound());

        // Act
        await _sut.Handle(query, cancellationToken);

        // Assert
        await _movementsRepository.Received(1)
            .GetMovementByIdAsync(movementId, cancellationToken);
    }
}
