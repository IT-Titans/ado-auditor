using System.Globalization;
using System.Threading.RateLimiting;

using AdoExport.Api.Configuration;

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace AdoExport.Api.RateLimiting;

public static class RateLimitingExtensions
{
    public static WebApplicationBuilder AddRateLimiting(this WebApplicationBuilder builder)
    {
        builder
            .Services
            .AddOptionsWithFluentValidation<RateLimitingConfiguration>(RateLimitingConfiguration.ConfigurationSectionName);

        // Azure App Service terminates the connection at its front end and forwards the
        // caller's address in X-Forwarded-For. Without this, RemoteIpAddress is the load
        // balancer for every request, every client lands in the same partition, and the
        // limiter would throttle all users together - worse than having none at all.
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            // The defaults trust loopback only, which excludes the App Service front end.
            // Clearing them accepts the header from the platform in front of us; nothing
            // else can reach the container.
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        builder.Services.AddRateLimiter(limiterOptions =>
        {
            var configuration = builder
                .Configuration
                .GetSection(RateLimitingConfiguration.ConfigurationSectionName)
                .Get<RateLimitingConfiguration>() ?? new RateLimitingConfiguration();

            limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Tells a well-behaved client when to come back instead of leaving it to
            // retry immediately and burn the next window as well.
            limiterOptions.OnRejected = (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                }

                return ValueTask.CompletedTask;
            };

            // Applies on top of the named policies rather than instead of them, so an
            // audit request counts against both its own budget and this one.
            limiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                httpContext => CreatePartition(
                    httpContext,
                    "global",
                    configuration.GlobalPermitLimit,
                    configuration));

            limiterOptions.AddPolicy(
                RateLimitPolicies.Mail,
                httpContext => CreatePartition(
                    httpContext,
                    RateLimitPolicies.Mail,
                    configuration.MailPermitLimit,
                    configuration));

            limiterOptions.AddPolicy(
                RateLimitPolicies.Audit,
                httpContext => CreatePartition(
                    httpContext,
                    RateLimitPolicies.Audit,
                    configuration.AuditPermitLimit,
                    configuration));
        });

        return builder;
    }

    /// <summary>
    /// Adds the rate limiting middleware unless it has been switched off in configuration.
    /// </summary>
    public static WebApplication UseConfiguredRateLimiting(this WebApplication app)
    {
        var configuration = app.Services.GetRequiredService<IOptions<RateLimitingConfiguration>>().Value;

        if (configuration.Enabled == false)
        {
            app.Logger.LogWarning("Rate limiting is disabled by configuration.");
            return app;
        }

        app.UseRateLimiter();

        return app;
    }

    private static RateLimitPartition<string> CreatePartition(
        HttpContext httpContext,
        string policyName,
        int permitLimit,
        RateLimitingConfiguration configuration)
    {
        // The policy name is part of the key so the same client keeps a separate budget
        // per policy. Without it, a single bucket would be shared and the tightest limit
        // would silently govern everything.
        //
        // The address is the visitor's when the frontend server vouches for it with the
        // shared secret, and the socket address otherwise.
        var partitionKey = $"{policyName}:{ClientAddressResolver.Resolve(httpContext, configuration.TrustedProxyKey)}";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = configuration.Window,
                QueueLimit = configuration.QueueLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true,
            });
    }
}
