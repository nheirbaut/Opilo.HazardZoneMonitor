using Ardalis.Result;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Opilo.HazardZoneMonitor.Api.Features.PersonTracking;

internal sealed class MovementsRepository(SqliteConnection connection) : IMovementsRepository
{
    public async Task<Result<RegisteredPersonMovement>> RegisterMovementAsync(
        Guid personId,
        double x,
        double y,
        DateTime registeredAt,
        CancellationToken cancellationToken)
    {
        RegisteredPersonMovement movement = new()
        {
            PersonId = personId,
            X = x,
            Y = y,
            RegisteredAt = registeredAt,
        };

        const string sql = """
            INSERT INTO PersonMovements (Id, PersonId, X, Y, RegisteredAt)
            VALUES (@Id, @PersonId, @X, @Y, @RegisteredAt)
            """;

        CommandDefinition command = new(sql, movement, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command).ConfigureAwait(false);

        return Result.Created(movement);
    }

    public async Task<Result<RegisteredPersonMovement>> GetMovementByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT Id, PersonId, X, Y, RegisteredAt
            FROM PersonMovements
            WHERE Id = @Id
            """;

        CommandDefinition command = new(sql, new { Id = id }, cancellationToken: cancellationToken);
        RegisteredPersonMovement? movement = await connection.QuerySingleOrDefaultAsync<RegisteredPersonMovement>(command)
            .ConfigureAwait(false);

        if (movement is null)
        {
            return Result<RegisteredPersonMovement>.NotFound();
        }

        return Result.Success(movement);
    }
}
