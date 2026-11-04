using Restaurant_Aqa_Framework.Api.Dtos.GroupOrders;
using Restaurant_Aqa_Framework.Screenplay.Tasks.GroupOrders;

namespace Restaurant_Aqa_Framework.Screenplay.Questions.GroupOrders;

public sealed class TheGroupSessions : IQuestion<GetAllSessionsResponse>
{
    public Task<GetAllSessionsResponse> AnsweredByAsync(Actor actor)
    {
        var response = actor.TheLastResponse();
        return Task.FromResult(response.Json<GetAllSessionsResponse>());
    }
}

public sealed class TheConsolidatedOrder : IQuestion<IReadOnlyList<ConsolidatedDishResponse>>
{
    public Task<IReadOnlyList<ConsolidatedDishResponse>> AnsweredByAsync(Actor actor)
    {
        var response = actor.TheLastResponse();
        return Task.FromResult(response.Json<IReadOnlyList<ConsolidatedDishResponse>>());
    }
}

