using System.Globalization;
using Ardalis.Result;
using Dapper;
using Opilo.HazardZoneMonitor.Api.Shared.Database;
using Opilo.HazardZoneMonitor.Domain.Shared.Primitives;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking;

internal sealed class MovementsRepository(IDbConnectionFactory connectionFactory) : IMovementsRepository
{
    public async Task<Result<RegisteredPersonMovement>> RegisterMovementAsync(
        Guid personId,
        Coordinate coordinate,
        DateTime registeredAt,
        CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.Create();
        connection.Open();

        var id = Guid.CreateVersion7();

        const string sql = """
            INSERT INTO PersonMovements (Id, PersonId, X, Y, RegisteredAt)
            VALUES (@Id, @PersonId, @X, @Y, @RegisteredAt)
            """;

        var parameters = new
        {
            Id = id,
            PersonId = personId,
            X = coordinate.X,
            Y = coordinate.Y,
            RegisteredAt = registeredAt,
        };

        CommandDefinition command = new(sql, parameters, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command).ConfigureAwait(false);

        RegisteredPersonMovement movement = new()
        {
            Id = id,
            PersonId = personId,
            Coordinate = coordinate,
            RegisteredAt = registeredAt,
        };

        return Result.Created(movement);
    }

    public async Task<Result<RegisteredPersonMovement>> GetMovementByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.Create();
        connection.Open();

        const string sql = """
            SELECT Id, PersonId, X, Y, RegisteredAt
            FROM PersonMovements
            WHERE Id = @Id
            """;

        CommandDefinition command = new(sql, new { Id = id }, cancellationToken: cancellationToken);
        var raw = await connection.QuerySingleOrDefaultAsync<dynamic>(command)
            .ConfigureAwait(false);

        if (raw is null)
        {
            return Result<RegisteredPersonMovement>.NotFound();
        }

        RegisteredPersonMovement movement = new()
        {
            Id = Guid.Parse((string)raw.Id),
            PersonId = Guid.Parse((string)raw.PersonId),
            Coordinate = new Coordinate((double)raw.X, (double)raw.Y),
            RegisteredAt = DateTime.Parse((string)raw.RegisteredAt, CultureInfo.InvariantCulture),
        };

        return Result.Success(movement);
    }
}
