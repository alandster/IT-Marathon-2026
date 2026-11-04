namespace Restaurant_Aqa_Framework.Api.Dtos.GroupOrders;

public sealed record CreateGuestSessionRequest(
    string GroupReservationId,
    string GuestName
);

public sealed record CreateGuestSessionResponse(
    string SessionToken
);

public sealed record UpdateGuestSessionDishRequest(
    string DishId,
    int Quantity
);

public sealed record GetOwnerSessionIdResponse(
    string? GuestSessionId
);

public sealed record GroupReservationStatusResponse(
    int CurrentParticipants,
    int MaxCapacity,
    string Status
);

public sealed record GroupInviteTokenResponse(
    string Token
);

public sealed record ParticipantItemResponse(
    string DishId,
    string DishName,
    int Quantity,
    decimal Price,
    decimal Total
);

public sealed record ParticipantResponse(
    string? Id,
    string Type,
    string Name,
    IReadOnlyList<ParticipantItemResponse> Items,
    decimal Total,
    bool IsSubmitted
);

public sealed record GetAllSessionsResponse(
    IReadOnlyList<ParticipantResponse> Participants,
    decimal Total
);

public sealed record SubmitGroupReservationRequest(
    string ReservationId
);

public sealed record ConsolidatedDishResponse(
    string DishId,
    string DishName,
    int Quantity,
    decimal Price
);

