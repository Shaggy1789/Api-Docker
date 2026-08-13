using BuildingBlocks.CQRS;
using Orders_API.Data;

namespace Orders_API.Application.Users;

public record GetUsersQuery() : IQuery<GetUsersResult>;

public record GetUsersResult(List<string> UserIds);

public class GetUsersHandler(IOrdersRepository repository) : IQueryHandler<GetUsersQuery, GetUsersResult>
{
    private readonly IOrdersRepository _repository = repository;

    public async Task<GetUsersResult> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var userIds = await _repository.GetUserIdsAsync(cancellationToken);
        return new GetUsersResult(userIds);
    }
}