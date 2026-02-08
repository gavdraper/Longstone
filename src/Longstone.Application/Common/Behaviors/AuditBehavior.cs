using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Longstone.Application.Common.Behaviors;

public sealed class AuditBehavior<TRequest, TResponse>(
    ILogger<AuditBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly ActivitySource ActivitySource = new("Longstone");

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        using var activity = ActivitySource.StartActivity($"MediatR:{requestName}");
        activity?.SetTag("mediatr.request_type", requestName);

        logger.LogInformation("Executing {RequestName}", requestName);

        try
        {
            var response = await next();

            logger.LogInformation("Completed {RequestName}", requestName);
            activity?.SetStatus(ActivityStatusCode.Ok);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed {RequestName}", requestName);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
