using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using APIHandler.Exceptions;
using APIHandler.Helpers;
using APIHandler.Models.GroupReservation;
using APIHandler.Services.GroupReservations;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace APIHandler.Handlers.GroupReservations;

public class SubmitGroupReservationHandler : IApiRequestHandler
{
    private readonly IGroupReservationService _groupReservationService;

    public SubmitGroupReservationHandler(IGroupReservationService groupReservationService)
    {
        _groupReservationService = groupReservationService ?? throw new ArgumentNullException(nameof(groupReservationService));
    }

    public async Task<APIGatewayProxyResponse> HandleRequest(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (!request.TryGetPathValue("groupReservationId", out var groupReservationId))
        {
            return ResponseBuilder.CreateErrorResponse(400, "Missing 'groupReservationId' in path parameters.", context.Logger);
        }

        if (request.RequestContext?.Authorizer?.Claims == null ||
            !request.RequestContext.Authorizer.Claims.TryGetValue("sub", out var userId))
        {
            return ResponseBuilder.CreateErrorResponse(401, "Unauthorized.", context.Logger);
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return ResponseBuilder.CreateErrorResponse(400, "Request body is empty.", context.Logger);
        }

        SubmitGroupReservationRequest? submitRequest;
        try
        {
            submitRequest = JsonSerializer.Deserialize<SubmitGroupReservationRequest>(request.Body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            return ResponseBuilder.CreateErrorResponse(400, "Invalid JSON in request body.", context.Logger, ex);
        }

        if (string.IsNullOrWhiteSpace(submitRequest?.ReservationId))
        {
            return ResponseBuilder.CreateErrorResponse(400, "Missing 'reservationId' in request body.", context.Logger);
        }

        try
        {
            var result = await _groupReservationService.SubmitGroupReservationAsync(userId, groupReservationId, submitRequest.ReservationId);
            return ResponseBuilder.CreateJsonResponse(200, result);
        }
        catch (GroupReservationNotFoundException ex)
        {
            return ResponseBuilder.CreateErrorResponse(404, ex.Message, context.Logger);
        }
        catch (ReservationNotFoundException ex)
        {
            return ResponseBuilder.CreateErrorResponse(404, ex.Message, context.Logger);
        }
        catch (ActionForbiddenException ex)
        {
            return ResponseBuilder.CreateErrorResponse(403, ex.Message, context.Logger);
        }
        catch (ValidationException ex)
        {
            return ResponseBuilder.CreateErrorResponse(400, ex.Message, context.Logger);
        }
    }
}
