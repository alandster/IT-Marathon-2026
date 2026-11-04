using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using APIHandler.Exceptions;
using APIHandler.Handlers.GuestSessions;
using APIHandler.Models.GuestSessions;
using APIHandler.Services.GuestSessions;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace APIHandler.Tests.Handlers;

public class GetOwnerGuestSessionIdHandlerTests
{
    private readonly Mock<IGuestSessionService> _guestSessionServiceMock = new();
    private readonly Mock<ILambdaContext> _contextMock = new();
    private readonly Mock<ILambdaLogger> _loggerMock = new();
    private readonly GetOwnerGuestSessionIdHandler _handler;

    public GetOwnerGuestSessionIdHandlerTests()
    {
        _contextMock.Setup(c => c.Logger).Returns(_loggerMock.Object);
        _handler = new GetOwnerGuestSessionIdHandler(_guestSessionServiceMock.Object);
    }

    [Fact]
    public async Task HandleRequest_MissingPathParameter_Returns400()
    {
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>()
        };

        var response = await _handler.HandleRequest(request, _contextMock.Object);

        Assert.Equal(400, response.StatusCode);
        Assert.Contains("reservationId", response.Body);
    }

    [Fact]
    public async Task HandleRequest_MissingAuthorizerClaims_Returns401()
    {
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "reservationId", "res-1" } },
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext()
        };

        var response = await _handler.HandleRequest(request, _contextMock.Object);

        Assert.Equal(401, response.StatusCode);
    }

    [Fact]
    public async Task HandleRequest_Success_Returns200WithGuestSessionId()
    {
        var expectedResponse = new GetOwnerSessionIdResponse("session-123");
        _guestSessionServiceMock
            .Setup(s => s.GetOwnerGuestSessionIdAsync("user-1", "res-1"))
            .ReturnsAsync(expectedResponse);

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "reservationId", "res-1" } },
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string> { { "sub", "user-1" } }
                }
            }
        };

        var response = await _handler.HandleRequest(request, _contextMock.Object);

        Assert.Equal(200, response.StatusCode);
        Assert.Contains("session-123", response.Body);
        Assert.Contains("guestSessionId", response.Body);
    }

    [Fact]
    public async Task HandleRequest_ReservationNotFound_Returns404()
    {
        _guestSessionServiceMock
            .Setup(s => s.GetOwnerGuestSessionIdAsync("user-1", "res-1"))
            .ThrowsAsync(new ReservationNotFoundException("Reservation not found"));

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "reservationId", "res-1" } },
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string> { { "sub", "user-1" } }
                }
            }
        };

        var response = await _handler.HandleRequest(request, _contextMock.Object);

        Assert.Equal(404, response.StatusCode);
    }

    [Fact]
    public async Task HandleRequest_ActionForbidden_Returns403()
    {
        _guestSessionServiceMock
            .Setup(s => s.GetOwnerGuestSessionIdAsync("user-1", "res-1"))
            .ThrowsAsync(new ActionForbiddenException("Only owner can access"));

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "reservationId", "res-1" } },
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string> { { "sub", "user-1" } }
                }
            }
        };

        var response = await _handler.HandleRequest(request, _contextMock.Object);

        Assert.Equal(403, response.StatusCode);
    }
}
