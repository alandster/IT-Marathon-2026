using APIHandler.Constants;
using APIHandler.Entities;
using APIHandler.Exceptions;
using APIHandler.Models.GroupReservation;
using APIHandler.Repositories.GroupReservations;
using APIHandler.Repositories.GuestSessions;
using APIHandler.Repositories.Orders;
using APIHandler.Repositories.Reservations;
using APIHandler.Repositories.Tables;
using APIHandler.Services.Auth;
using APIHandler.Services.GroupReservations;
using APIHandler.Services.Locations;
using AutoFixture.Xunit2;
using Moq;

namespace APIHandler.Tests.Services;

public class GroupReservationServiceTests
{
    private readonly Mock<IGroupReservationRepository> _groupReservationRepositoryMock = new();
    private readonly Mock<ITableRepository> _tableRepositoryMock = new();
    private readonly Mock<IReservationRepository> _reservationRepositoryMock = new();
    private readonly Mock<IGuestSessionRepository> _guestSessionRepositoryMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<ILocationService> _locationServiceMock = new();
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();

    private readonly GroupReservationService _service;

    public GroupReservationServiceTests()
    {
        _service = new GroupReservationService(
            _groupReservationRepositoryMock.Object,
            _tableRepositoryMock.Object,
            _reservationRepositoryMock.Object,
            _guestSessionRepositoryMock.Object,
            _userServiceMock.Object,
            _locationServiceMock.Object,
            _orderRepositoryMock.Object);
    }

    [Theory]
    [AutoData]
    public async Task GetGroupReservationByTokenAsync_ValidInput_ReturnsGroupReservationInfo(
        GroupReservation groupReservation,
        Table table,
        Reservation reservation,
        Location location,
        string expectedUserName)
    {
        groupReservation.ReservationId = reservation.Id;
        groupReservation.Status = GroupReservationStatus.Open;
        reservation.TableId = table.Id;
        table.LocationId = location.Id;

        _groupReservationRepositoryMock
            .Setup(r => r.GetGroupReservationByTokenAsync(groupReservation.ReservationToken))
            .ReturnsAsync(groupReservation);

        _reservationRepositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _guestSessionRepositoryMock
            .Setup(r => r.CountByGroupReservationId(groupReservation.GroupReservationId))
            .ReturnsAsync(0);

        _tableRepositoryMock
            .Setup(r => r.GetTableAsync(table.Id))
            .ReturnsAsync(table);

        _locationServiceMock
            .Setup(s => s.GetLocationByIdAsync(table.LocationId))
            .ReturnsAsync(location);

        _userServiceMock
            .Setup(s => s.GetUserName(reservation.CustomerId!))
            .ReturnsAsync(expectedUserName);

        var result = await _service.GetGroupReservationByTokenAsync(groupReservation.ReservationToken);

        Assert.Equal(groupReservation.GroupReservationId, result.GroupReservationId);
        Assert.Equal(table.LocationId, result.LocationId);
        Assert.Equal(reservation.GuestCount, result.MaxGuestCount);
        Assert.Equal(reservation.StartTime, result.StartTime);
        Assert.Equal(reservation.EndTime, result.EndTime);
        Assert.Equal(table.TableNumber, result.TableNumber);
        Assert.Equal("Green & Tasty", result.RestaurantName);
        Assert.Equal(location.Address, result.LocationAddress);
        Assert.Equal(expectedUserName, result.InviterName);
    }

    [Theory]
    [AutoData]
    public async Task GetGroupReservationByTokenAsync_InvalidToken_ThrowsNotFound(string token)
    {
        _groupReservationRepositoryMock
            .Setup(r => r.GetGroupReservationByTokenAsync(token))
            .ReturnsAsync((GroupReservation?)null);

        await Assert.ThrowsAsync<GroupReservationNotFoundException>(() => _service.GetGroupReservationByTokenAsync(token));
    }

    [Theory]
    [InlineAutoData(GroupReservationStatus.Closed)]
    [InlineAutoData(GroupReservationStatus.Submitted)]
    public async Task GetGroupReservationByTokenAsync_GroupReservationIsNotOpen_ThrowsException(
        GroupReservationStatus status,
        GroupReservation groupReservation)
    {
        groupReservation.Status = status;

        _groupReservationRepositoryMock
            .Setup(r => r.GetGroupReservationByTokenAsync(groupReservation.ReservationToken))
            .ReturnsAsync(groupReservation);

        await Assert.ThrowsAsync<GroupReservationExpiredException>(() => _service.GetGroupReservationByTokenAsync(groupReservation.ReservationToken));
    }

    [Theory]
    [AutoData]
    public async Task GetGroupReservationByTokenAsync_WhenGuestCountIsMax_ThrowsGuestLimitExceeded(GroupReservation groupReservation, Reservation reservation)
    {
        groupReservation.Status = GroupReservationStatus.Open;
        groupReservation.ReservationId = reservation.Id;

        _groupReservationRepositoryMock
            .Setup(r => r.GetGroupReservationByTokenAsync(groupReservation.ReservationToken))
            .ReturnsAsync(groupReservation);

        _reservationRepositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _guestSessionRepositoryMock
            .Setup(r => r.CountByGroupReservationId(groupReservation.GroupReservationId))
            .ReturnsAsync(reservation.GuestCount);

        await Assert.ThrowsAsync<GuestLimitExceededException>(() => _service.GetGroupReservationByTokenAsync(groupReservation.ReservationToken));
    }

    [Theory]
    [AutoData]
    public async Task CloseGroupReservationAsync_ValidInput_UpdatesGroupReservationStatus(GroupReservation groupReservation)
    {
        _groupReservationRepositoryMock
            .Setup(r => r.GetByReservationIdAsync(groupReservation.ReservationId))
            .ReturnsAsync(groupReservation);

        GroupReservation? savedGroupReservation = null;
        _groupReservationRepositoryMock
            .Setup(r => r.SaveGroupReservationAsync(It.IsAny<GroupReservation>()))
            .Callback<GroupReservation>(r => savedGroupReservation = r);

        await _service.CloseGroupReservationAsync(groupReservation.ReservationId);

        Assert.NotNull(savedGroupReservation);
        Assert.Equal(GroupReservationStatus.Closed, savedGroupReservation.Status);
    }

    [Theory]
    [AutoData]
    public async Task CloseGroupReservationAsync_GroupReservationNotFound_DoesNotThrowException(string reservationId)
    {
        _groupReservationRepositoryMock
            .Setup(r => r.GetByReservationIdAsync(reservationId))
            .ReturnsAsync((GroupReservation?)null);

        await _service.CloseGroupReservationAsync(reservationId);
    }

    [Theory]
    [AutoData]
    public async Task GetGroupReservationInviteTokenAsync_ValidInput_ReturnsToken(Reservation reservation, GroupReservation groupReservation)
    {
        reservation.Status = ReservationStatus.Reserved;
        reservation.GuestCount = 3;
        groupReservation.ReservationId = reservation.Id;
        groupReservation.Status = GroupReservationStatus.Open;

        _reservationRepositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _groupReservationRepositoryMock
            .Setup(r => r.GetByReservationIdAsync(reservation.Id))
            .ReturnsAsync(groupReservation);

        GetGroupInviteTokenResponse result = await _service.GetGroupReservationInviteTokenAsync(reservation.Id);

        Assert.Equal(groupReservation.ReservationToken, result.Token);
    }

    [Theory]
    [AutoData]
    public async Task GetGroupReservationInviteTokenAsync_InvalidReservationId_ThrowsNotFound(string reservationId)
    {
        _reservationRepositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservationId))
            .ReturnsAsync((Reservation?)null);

        await Assert.ThrowsAsync<ReservationNotFoundException>(() => _service.GetGroupReservationInviteTokenAsync(reservationId));
    }

    [Theory]
    [AutoData]
    public async Task GetGroupReservationInviteTokenAsync_OneGuest_ThrowsValidationException(Reservation reservation)
    {
        reservation.Status = ReservationStatus.Reserved;
        reservation.GuestCount = 1;

        _reservationRepositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        await Assert.ThrowsAsync<ValidationException>(() => _service.GetGroupReservationInviteTokenAsync(reservation.Id));
    }

    [Theory]
    [InlineAutoData(ReservationStatus.Completed)]
    [InlineAutoData(ReservationStatus.Cancelled)]
    [InlineAutoData(ReservationStatus.InProgress)]
    public async Task GetGroupReservationInviteTokenAsync_StatusIsNotReserved_ThrowsValidationException(ReservationStatus reservationStatus, Reservation reservation)
    {
        reservation.Status = reservationStatus;
        reservation.GuestCount = 1;

        _reservationRepositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        await Assert.ThrowsAsync<ValidationException>(() => _service.GetGroupReservationInviteTokenAsync(reservation.Id));
    }

    [Theory]
    [InlineAutoData(GroupReservationStatus.Closed, typeof(GroupReservationExpiredException))]
    [InlineAutoData(GroupReservationStatus.Submitted, typeof(GroupReservationSubmittedException))]
    public async Task GetGroupReservationInviteTokenAsync_GroupStatusIsNotOpen_ThrowsException(GroupReservationStatus status, Type type, Reservation reservation, GroupReservation groupReservation)
    {
        reservation.Status = ReservationStatus.Reserved;
        reservation.GuestCount = 3;
        groupReservation.ReservationId = reservation.Id;
        groupReservation.Status = status;

        _reservationRepositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _groupReservationRepositoryMock
            .Setup(r => r.GetByReservationIdAsync(reservation.Id))
            .ReturnsAsync(groupReservation);

        await Assert.ThrowsAsync(type, () => _service.GetGroupReservationInviteTokenAsync(reservation.Id));
    }

    [Theory]
    [AutoData]
    public async Task SubmitGroupReservationAsync_ValidInput_ConsolidatesDishesAndUpdatesStatuses(
        string userId,
        GroupReservation groupReservation,
        Reservation reservation,
        Order order,
        GuestSession submittedSession1,
        GuestSession submittedSession2,
        GuestSession unsubmittedSession)
    {
        reservation.CustomerId = userId;
        groupReservation.ReservationId = reservation.Id;
        order.ReservationId = reservation.Id;

        submittedSession1.IsSubmitted = true;
        submittedSession1.Dishes =
        [
            new OrderItem { DishId = "dish-1", Quantity = 2 },
            new OrderItem { DishId = "dish-2", Quantity = 1 }
        ];

        submittedSession2.IsSubmitted = true;
        submittedSession2.Dishes =
        [
            new OrderItem { DishId = "dish-1", Quantity = 1 },
            new OrderItem { DishId = "dish-3", Quantity = 4 }
        ];

        unsubmittedSession.IsSubmitted = false;
        unsubmittedSession.Dishes =
        [
            new OrderItem { DishId = "dish-1", Quantity = 99 }
        ];

        _groupReservationRepositoryMock
            .Setup(r => r.GetGroupReservationByIdAsync(groupReservation.GroupReservationId))
            .ReturnsAsync(groupReservation);

        _reservationRepositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _guestSessionRepositoryMock
            .Setup(r => r.GetAllByGroupReservationIdAsync(groupReservation.GroupReservationId))
            .ReturnsAsync([submittedSession1, submittedSession2, unsubmittedSession]);

        _orderRepositoryMock
            .Setup(r => r.GetOrderByReservationIdAsync(reservation.Id))
            .ReturnsAsync(order);

        Order? savedOrder = null;
        _orderRepositoryMock
            .Setup(r => r.SaveOrderAsync(It.IsAny<Order>()))
            .Callback<Order>(o => savedOrder = o)
            .Returns(Task.CompletedTask);

        GroupReservation? savedGroup = null;
        _groupReservationRepositoryMock
            .Setup(r => r.SaveGroupReservationAsync(It.IsAny<GroupReservation>()))
            .Callback<GroupReservation>(g => savedGroup = g)
            .Returns(Task.CompletedTask);

        var result = await _service.SubmitGroupReservationAsync(userId, groupReservation.GroupReservationId, reservation.Id);

        Assert.Equal(3, result.Count);
        Assert.Equal(3, result.First(d => d.DishId == "dish-1").Quantity); // 2 + 1
        Assert.Equal(1, result.First(d => d.DishId == "dish-2").Quantity);
        Assert.Equal(4, result.First(d => d.DishId == "dish-3").Quantity);

        Assert.NotNull(savedOrder);
        Assert.Equal(OrderStatus.SubmittedByCustomer, savedOrder.Status);
        Assert.Equal(3, savedOrder.Dishes.Count);

        Assert.NotNull(savedGroup);
        Assert.Equal(GroupReservationStatus.Submitted, savedGroup.Status);
    }

    [Theory]
    [AutoData]
    public async Task SubmitGroupReservationAsync_GroupNotFound_ThrowsGroupReservationNotFoundException(
        string userId,
        string groupReservationId,
        string reservationId)
    {
        _groupReservationRepositoryMock
            .Setup(r => r.GetGroupReservationByIdAsync(groupReservationId))
            .ReturnsAsync((GroupReservation?)null);

        await Assert.ThrowsAsync<GroupReservationNotFoundException>(() =>
            _service.SubmitGroupReservationAsync(userId, groupReservationId, reservationId));
    }

    [Theory]
    [AutoData]
    public async Task SubmitGroupReservationAsync_ReservationNotFound_ThrowsReservationNotFoundException(
        string userId,
        GroupReservation groupReservation,
        string reservationId)
    {
        _groupReservationRepositoryMock
            .Setup(r => r.GetGroupReservationByIdAsync(groupReservation.GroupReservationId))
            .ReturnsAsync(groupReservation);

        _reservationRepositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservationId))
            .ReturnsAsync((Reservation?)null);

        await Assert.ThrowsAsync<ReservationNotFoundException>(() =>
            _service.SubmitGroupReservationAsync(userId, groupReservation.GroupReservationId, reservationId));
    }

    [Theory]
    [AutoData]
    public async Task SubmitGroupReservationAsync_ReservationMismatch_ThrowsReservationNotFoundException(
        string userId,
        GroupReservation groupReservation,
        Reservation reservation,
        string differentReservationId)
    {
        groupReservation.ReservationId = reservation.Id;

        _groupReservationRepositoryMock
            .Setup(r => r.GetGroupReservationByIdAsync(groupReservation.GroupReservationId))
            .ReturnsAsync(groupReservation);

        _reservationRepositoryMock
            .Setup(r => r.GetReservationByIdAsync(differentReservationId))
            .ReturnsAsync(reservation);

        await Assert.ThrowsAsync<ReservationNotFoundException>(() =>
            _service.SubmitGroupReservationAsync(userId, groupReservation.GroupReservationId, differentReservationId));
    }

    [Theory]
    [AutoData]
    public async Task SubmitGroupReservationAsync_NonOwner_ThrowsActionForbiddenException(
        string userId,
        string otherUserId,
        GroupReservation groupReservation,
        Reservation reservation)
    {
        reservation.CustomerId = otherUserId;
        groupReservation.ReservationId = reservation.Id;

        _groupReservationRepositoryMock
            .Setup(r => r.GetGroupReservationByIdAsync(groupReservation.GroupReservationId))
            .ReturnsAsync(groupReservation);

        _reservationRepositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        await Assert.ThrowsAsync<ActionForbiddenException>(() =>
            _service.SubmitGroupReservationAsync(userId, groupReservation.GroupReservationId, reservation.Id));
    }

    [Theory]
    [AutoData]
    public async Task SubmitGroupReservationAsync_NoSubmittedSessions_ThrowsValidationException(
        string userId,
        GroupReservation groupReservation,
        Reservation reservation,
        GuestSession unsubmittedSession)
    {
        reservation.CustomerId = userId;
        groupReservation.ReservationId = reservation.Id;
        unsubmittedSession.IsSubmitted = false;

        _groupReservationRepositoryMock
            .Setup(r => r.GetGroupReservationByIdAsync(groupReservation.GroupReservationId))
            .ReturnsAsync(groupReservation);

        _reservationRepositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _guestSessionRepositoryMock
            .Setup(r => r.GetAllByGroupReservationIdAsync(groupReservation.GroupReservationId))
            .ReturnsAsync([unsubmittedSession]);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.SubmitGroupReservationAsync(userId, groupReservation.GroupReservationId, reservation.Id));
    }

    [Theory]
    [AutoData]
    public async Task SubmitGroupReservationAsync_NoDishesInSubmittedSessions_ThrowsValidationException(
        string userId,
        GroupReservation groupReservation,
        Reservation reservation,
        GuestSession submittedSession)
    {
        reservation.CustomerId = userId;
        groupReservation.ReservationId = reservation.Id;
        submittedSession.IsSubmitted = true;
        submittedSession.Dishes = [];

        _groupReservationRepositoryMock
            .Setup(r => r.GetGroupReservationByIdAsync(groupReservation.GroupReservationId))
            .ReturnsAsync(groupReservation);

        _reservationRepositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _guestSessionRepositoryMock
            .Setup(r => r.GetAllByGroupReservationIdAsync(groupReservation.GroupReservationId))
            .ReturnsAsync([submittedSession]);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.SubmitGroupReservationAsync(userId, groupReservation.GroupReservationId, reservation.Id));
    }

    [Theory]
    [AutoData]
    public async Task SubmitGroupReservationAsync_OrderNotFound_ThrowsValidationException(
        string userId,
        GroupReservation groupReservation,
        Reservation reservation,
        GuestSession submittedSession)
    {
        reservation.CustomerId = userId;
        groupReservation.ReservationId = reservation.Id;
        submittedSession.IsSubmitted = true;
        submittedSession.Dishes = [new OrderItem { DishId = "d1", Quantity = 1 }];

        _groupReservationRepositoryMock
            .Setup(r => r.GetGroupReservationByIdAsync(groupReservation.GroupReservationId))
            .ReturnsAsync(groupReservation);

        _reservationRepositoryMock
            .Setup(r => r.GetReservationByIdAsync(reservation.Id))
            .ReturnsAsync(reservation);

        _guestSessionRepositoryMock
            .Setup(r => r.GetAllByGroupReservationIdAsync(groupReservation.GroupReservationId))
            .ReturnsAsync([submittedSession]);

        _orderRepositoryMock
            .Setup(r => r.GetOrderByReservationIdAsync(reservation.Id))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.SubmitGroupReservationAsync(userId, groupReservation.GroupReservationId, reservation.Id));
    }
}
