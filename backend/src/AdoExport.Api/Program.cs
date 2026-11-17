using AdoExport.Api.Configuration;
using AdoExport.Api.Middlewares;
using AdoExport.Api.RateLimiting;

using Scalar.AspNetCore;

namespace AdoExport.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // First, so that everything configured below already sees these settings.
        var branchSettings = builder.AddBranchSettings();

        builder.ConfigureServices();

        builder.Services.AddControllers();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Says in the log which limits an instance runs with, without anyone having to
        // work it out from the image tag.
        app.Logger.LogInformation(
            "Branch settings: built from '{Branch}', file '{FileName}', applied: {Applied}",
            branchSettings.Branch ?? "(not set)",
            branchSettings.FileName ?? "(none)",
            branchSettings.Applied);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        // Must run before anything that reads the client address: behind Azure App
        // Service the real caller is only in X-Forwarded-For.
        app.UseForwardedHeaders();

        app.UseMiddleware<ExceptionMiddleware>();

        app.UseHttpsRedirection();

        // UseAuthorization() used to stand here. Nothing registered an authentication or
        // authorization service and no endpoint carried [Authorize], so the call did
        // nothing except suggest the API was guarded. The service has no accounts; what
        // protects it is the unguessable audit key plus the limits below.
        app.UseConfiguredRateLimiting();

        app.MapControllers();

        app.Run();
    }
}