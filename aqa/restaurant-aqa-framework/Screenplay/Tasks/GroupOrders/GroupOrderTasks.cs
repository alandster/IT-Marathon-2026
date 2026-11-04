using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.GroupOrders;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks.GroupOrders;

public sealed record GroupReservationContext(
    string GroupReservationId,
    string ReservationId,
    string OwnerToken,
    List<GuestSessionContext> Guests
);

public sealed record GuestSessionContext(
    string GuestSessionId,
    string GuestName,
    string SessionToken
);

public sealed class SetUpGroupReservation(int initialGuests = 2) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var api = new GroupOrdersApi(actor.UsingTheApi());
        var response = actor.TheLastResponse();

        var groupReservationId = $"AQA-Sample-Grp-{Guid.NewGuid():N}";
        var reservationId = $"AQA-Sample-Res-{Guid.NewGuid():N}";
        var ownerToken = "owner-simulated-token";

        var guests = new List<GuestSessionContext>();
        for (var i = 1; i <= initialGuests; i++)
        {
            var guestName = $"Guest {i}";
            var sessionToken = $"token-guest-{Guid.NewGuid():N}";
            var sessionId = $"session-{Guid.NewGuid():N}";
            guests.Add(new GuestSessionContext(sessionId, guestName, sessionToken));
        }

        var context = new GroupReservationContext(groupReservationId, reservationId, ownerToken, guests);
        actor.Remember(context);
    }
}

public sealed class AddGuestDishesAndSubmit(bool submitCart = true) : ITask
{
    public bool SubmitCart => submitCart;

    public Task PerformAsAsync(Actor actor)
    {
        _ = SubmitCart;
        return Task.CompletedTask;
    }
}

public sealed class ViewOwnerGroupSessions(string? groupReservationId = null, string? token = null) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var api = new GroupOrdersApi(actor.UsingTheApi());
        var context = actor.TryRecall<GroupReservationContext>(out var c) ? c : null;
        var targetGroupId = groupReservationId ?? context?.GroupReservationId ?? "nonexistent-group-id";
        var authToken = token ?? context?.OwnerToken;

        var rawResponse = await api.GetGroupSessionsAsync(targetGroupId, authToken);
        await actor.TheLastResponse().CaptureAsync(rawResponse);
    }
}

public sealed class ConfirmConsolidatedPreOrder(string? groupReservationId = null, string? token = null) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var api = new GroupOrdersApi(actor.UsingTheApi());
        var context = actor.TryRecall<GroupReservationContext>(out var c) ? c : null;
        var targetGroupId = groupReservationId ?? context?.GroupReservationId ?? "nonexistent-group-id";
        var resId = context?.ReservationId ?? "res-id";
        var authToken = token ?? context?.OwnerToken;

        var rawResponse = await api.SubmitGroupReservationAsync(
            targetGroupId,
            new SubmitGroupReservationRequest(resId),
            authToken);
        await actor.TheLastResponse().CaptureAsync(rawResponse);
    }
}

public sealed class RemoveGuestFromGroup(string? sessionId = null, string? groupReservationId = null, string? token = null) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var api = new GroupOrdersApi(actor.UsingTheApi());
        var context = actor.TryRecall<GroupReservationContext>(out var c) ? c : null;
        var targetGroupId = groupReservationId ?? context?.GroupReservationId ?? "nonexistent-group-id";
        var targetSessionId = sessionId ?? context?.Guests.FirstOrDefault()?.GuestSessionId ?? "nonexistent-session";
        var authToken = token ?? context?.OwnerToken;

        var rawResponse = await api.DeleteGuestSessionAsync(targetGroupId, targetSessionId, authToken);
        await actor.TheLastResponse().CaptureAsync(rawResponse);
    }
}
