using FluentResults;

namespace AdoExport.Api.Services.Notifications;

/// <summary>
/// Sends a mail that nobody waits for.
/// </summary>
/// <remarks>
/// Audit metrics and error reports are for the operators. The user's request must not get
/// slower or fail because one of them could not be sent, so they are not awaited. A task
/// that is merely discarded takes its failure with it, though: nothing reads the result
/// and an exception is never observed. This keeps the send off the request and still
/// puts every way it can go wrong into the log.
/// </remarks>
public static class BackgroundMail
{
    /// <summary>
    /// Starts <paramref name="send" /> without waiting for it and logs when it does not
    /// succeed.
    /// </summary>
    /// <param name="send">Starts the send. Taken as a delegate so that an exception thrown
    /// while starting it is caught here as well, instead of reaching the caller.</param>
    /// <param name="logger">Where a failure is reported.</param>
    /// <param name="description">Names the mail in the log, for example "audit metrics".</param>
    /// <returns>A task that completes once the send has been observed. It never fails.
    /// Callers discard it; tests await it.</returns>
    public static Task Send(Func<Task<Result>> send, ILogger logger, string description)
    {
        Guard.IsNotNull(send);
        Guard.IsNotNull(logger);
        Guard.IsNotNullOrWhiteSpace(description);

        return ObserveAsync(send, logger, description);
    }

    private static async Task ObserveAsync(Func<Task<Result>> send, ILogger logger, string description)
    {
        try
        {
            var result = await send().ConfigureAwait(false);

            if (result is { IsFailed: true })
            {
                logger.LogError("The {Description} mail was not sent: {Result}", description, result);
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "The {Description} mail was not sent", description);
        }
    }
}
