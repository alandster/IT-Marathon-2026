using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using APIHandler.Exceptions;
using APIHandler.Handlers.GroupReservations;
using APIHandler.Models.GroupReservation;
using APIHandler.Services.GroupReservations;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace APIHandler.Tests.Handlers;

public class SubmitGroupReservationHandlerTests
{
    private readonly Mock<IGroupReservationService> _groupReservationServiceMock = new();
    private readonly Mock<ILambdaContext> _contextMock = new();
    private readonly Mock<ILambdaLogger> _loggerMock = new();
    private readonly SubmitGroupReservationHandler _handler;

    public SubmitGroupReservationHandlerTests()
    {
        _contextMock.Setup(c => c.Logger).Returns(_loggerMock.Object);
        _handler = new SubmitGroupReservationHandler(_groupReservationServiceMock.Object);
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
    public async Task HandleRequest_EmptyBody_Returns400()
    {
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "groupReservationId", "grp-1" } },
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string> { { "sub", "user-1" } }
                }
            },
            Body = ""
        };

        var response = await _handler.HandleRequest(request, _contextMock.Object);

        Assert.Equal(400, response.StatusCode);
        Assert.Contains("empty", response.Body);
    }

    [Fact]
    public async Task HandleRequest_MissingReservationIdInBody_Returns400()
    {
        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "groupReservationId", "grp-1" } },
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string> { { "sub", "user-1" } }
                }
            },
            Body = "{}"
        };

        var response = await _handler.HandleRequest(request, _contextMock.Object);

        Assert.Equal(400, response.StatusCode);
        Assert.Contains("reservationId", response.Body);
    }

    [Fact]
    public async Task HandleRequest_Success_Returns200WithConsolidatedDishes()
    {
        var expectedResult = new List<ConsolidatedDishResponse>
        {
            new("d1", 3),
            new("d2", 2)
        };

        _groupReservationServiceMock
            .Setup(s => s.SubmitGroupReservationAsync("user-1", "grp-1", "res-1"))
            .ReturnsAsync(expectedResult);

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "groupReservationId", "grp-1" } },
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string> { { "sub", "user-1" } }
                }
            },
            Body = "{\"reservationId\": \"res-1\"}"
        };

        var response = await _handler.HandleRequest(request, _contextMock.Object);

        Assert.Equal(200, response.StatusCode);
        Assert.Contains("d1", response.Body);
        Assert.Contains("d2", response.Body);
    }

    [Fact]
    public async Task HandleRequest_NoSubmittedSessions_Returns400()
    {
        _groupReservationServiceMock
            .Setup(s => s.SubmitGroupReservationAsync("user-1", "grp-1", "res-1"))
            .ThrowsAsync(new ValidationException("No submitted guest sessions found."));

        var request = new APIGatewayProxyRequest
        {
            PathParameters = new Dictionary<string, string> { { "groupReservationId", "grp-1" } },
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string> { { "sub", "user-1" } }
                }
            },
            Body = "{\"reservationId\": \"res-1\"}"
        };

        var response = await _handler.HandleRequest(request, _contextMock.Object);

        Assert.Equal(400, response.StatusCode);
    }
}
