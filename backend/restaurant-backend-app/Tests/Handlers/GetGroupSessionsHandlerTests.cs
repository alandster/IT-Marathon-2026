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

public class GetGroupSessionsHandlerTests
{
    private readonly Mock<IGuestSessionService> _guestSessionServiceMock = new();
    private readonly Mock<ILambdaContext> _contextMock = new();
    private readonly Mock<ILambdaLogger> _loggerMock = new();
    private readonly GetGroupSessionsHandler _handler;

    public GetGroupSessionsHandlerTests()
    {
        _contextMock.Setup(c => c.Logger).Returns(_loggerMock.Object);
        _handler = new GetGroupSessionsHandler(_guestSessionServiceMock.Object);
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
        Assert.Contains("groupReservationId", response.Body);
    }

    [Fact]
    public async Task HandleRequest_MissingAuthorizerClaims_Returns401()
    {
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "groupReservationId", "grp-1" } },
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext()
        };

        var response = await _handler.HandleRequest(request, _contextMock.Object);

        Assert.Equal(401, response.StatusCode);
    }

    [Fact]
    public async Task HandleRequest_Success_Returns200WithSessions()
    {
        var expectedResponse = new GetAllSessionsResponse([], 0);
        _guestSessionServiceMock
            .Setup(s => s.GetGroupSessionsAsync("user-1", "grp-1"))
            .ReturnsAsync(expectedResponse);

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "groupReservationId", "grp-1" } },
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
    public async Task HandleRequest_ActionForbidden_Returns403()
    {
        _guestSessionServiceMock
            .Setup(s => s.GetGroupSessionsAsync("user-1", "grp-1"))
            .ThrowsAsync(new ActionForbiddenException("Only owner can view"));

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "groupReservationId", "grp-1" } },
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
