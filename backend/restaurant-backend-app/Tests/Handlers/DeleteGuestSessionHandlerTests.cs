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

public class DeleteGuestSessionHandlerTests
{
    private readonly Mock<IGuestSessionService> _guestSessionServiceMock = new();
    private readonly Mock<ILambdaContext> _contextMock = new();
    private readonly Mock<ILambdaLogger> _loggerMock = new();
    private readonly DeleteGuestSessionHandler _handler;

    public DeleteGuestSessionHandlerTests()
    {
        _contextMock.Setup(c => c.Logger).Returns(_loggerMock.Object);
        _handler = new DeleteGuestSessionHandler(_guestSessionServiceMock.Object);
    }

    [Fact]
    public async Task HandleRequest_MissingGroupReservationId_Returns400()
    {
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "guestSessionId", "sess-1" } }
        };

        var response = await _handler.HandleRequest(request, _contextMock.Object);

        Assert.Equal(400, response.StatusCode);
        Assert.Contains("groupReservationId", response.Body);
    }

    [Fact]
    public async Task HandleRequest_MissingGuestSessionId_Returns400()
    {
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "groupReservationId", "grp-1" } }
        };

        var response = await _handler.HandleRequest(request, _contextMock.Object);

        Assert.Equal(400, response.StatusCode);
        Assert.Contains("guestSessionId", response.Body);
    }

    [Fact]
    public async Task HandleRequest_MissingAuthorizerClaims_Returns401()
    {
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                { "groupReservationId", "grp-1" },
                { "guestSessionId", "sess-1" }
            },
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext()
        };

        var response = await _handler.HandleRequest(request, _contextMock.Object);

        Assert.Equal(401, response.StatusCode);
    }

    [Fact]
    public async Task HandleRequest_Success_Returns200WithUpdatedSnapshot()
    {
        var updatedSnapshot = new GetAllSessionsResponse([], 0);
        _guestSessionServiceMock
            .Setup(s => s.DeleteGuestSessionAsync("user-1", "grp-1", "sess-1"))
            .ReturnsAsync(updatedSnapshot);

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                { "groupReservationId", "grp-1" },
                { "guestSessionId", "sess-1" }
            },
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
        Assert.Contains("participants", response.Body);
    }

    [Fact]
    public async Task HandleRequest_SessionNotFound_Returns404()
    {
        _guestSessionServiceMock
            .Setup(s => s.DeleteGuestSessionAsync("user-1", "grp-1", "sess-1"))
            .ThrowsAsync(new GuestSessionNotFoundException("Session not found"));

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                { "groupReservationId", "grp-1" },
                { "guestSessionId", "sess-1" }
            },
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
            .Setup(s => s.DeleteGuestSessionAsync("user-1", "grp-1", "sess-1"))
            .ThrowsAsync(new ActionForbiddenException("Only owner can delete"));

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string>
            {
                { "groupReservationId", "grp-1" },
                { "guestSessionId", "sess-1" }
            },
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
