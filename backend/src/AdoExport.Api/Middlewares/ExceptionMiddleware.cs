using System.Net;

using AdoExport.Api.Configuration;
using AdoExport.Api.Services.Notifications;

using Microsoft.Extensions.Options;

namespace AdoExport.Api.Middlewares;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        Guard.IsNotNull(next);
        Guard.IsNotNull(logger);

        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
        {
            // do nothing when request was canceled
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{TraceId}] Got an unhandled error", httpContext.TraceIdentifier);

            await HandleExceptionAsync(httpContext, ex, _logger).ConfigureAwait(false);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger logger)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        await context.Response.WriteAsync(new ErrorDetails()
        {
            TraceId = context.TraceIdentifier,
            StatusCode = context.Response.StatusCode,
            Message = "Internal Server Error"
        }.ToString()).ConfigureAwait(false);

        var sendMailOptions = context.RequestServices.GetRequiredService<IOptions<ErrorReportsMailConfiguration>>();
        var sendMailService = context.RequestServices.GetRequiredService<ISendMailService>();

        // Explicitly fire and forget. No need to block execution for this. A report that
        // cannot be sent is logged, so the error it was about is not lost twice.
        _ = BackgroundMail.Send(
            () => sendMailService.SendErrorReportMailAsync(
                sendMailOptions.Value.Sender,
                sendMailOptions.Value.Receiver,
                new ErrorReport()
                {
                    TraceId = context.TraceIdentifier,
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                }),
            logger,
            "error report");
    }
}