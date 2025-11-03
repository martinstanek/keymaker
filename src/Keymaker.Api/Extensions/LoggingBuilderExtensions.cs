using Microsoft.Extensions.Logging;

namespace Keymaker.Api.Extensions;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder SetDefaultLevels(this ILoggingBuilder builder)
    {
        builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = false;
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
        });

        return builder
            .SetMinimumLevel(LogLevel.Information)
            .AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning)
            .AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning)
            .AddFilter("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware", LogLevel.Warning)
            .AddFilter("Microsoft.AspNetCore.Cors", LogLevel.Warning)
            .AddFilter("Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult", LogLevel.Warning)
            .AddFilter("Microsoft.AspNetCore.Http.Result", LogLevel.Warning)
            .AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning)
            .AddFilter("Microsoft.AspNetCore.Routing.EndpointMiddleware", LogLevel.Warning)
            .AddFilter("Microsoft.AspNetCore.Mvc.StatusCodeResult", LogLevel.Warning)
            .AddFilter("Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker", LogLevel.Warning)
            .AddFilter("Microsoft.AspNetCore.Mvc.Infrastructure.ObjectResultExecutor", LogLevel.Warning)
            .AddFilter("System.Net.Http.HttpClient.Default.LogicalHandler", LogLevel.Warning)
            .AddFilter("System.Net.Http.HttpClient.Default.ClientHandler", LogLevel.Warning)
            .AddFilter("System.Net.Http.HttpClient.StateHolderApiClient.LogicalHandler", LogLevel.Warning)
            .AddFilter("System.Net.Http.HttpClient.health-checks.ClientHandler", LogLevel.Warning)
            .AddFilter("System.Net.Http.HttpClient.health-checks.LogicalHandler", LogLevel.Warning)
            .AddFilter("HealthChecks.UI.Core.HostedService.UIInitializationHostedService", LogLevel.Warning);
    }
}