using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Keymaker.Api.OpenApi.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseSwaggerApiDoc(this WebApplication app, string apiTitle)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", apiTitle);
            c.InjectStylesheet("/swagger-ui/SwaggerDark.css");
        });
        app.MapGet("/swagger-ui/SwaggerDark.css", async (CancellationToken cancellationToken) =>
        {
            var css = await File.ReadAllBytesAsync("OpenApi/Styles/SwaggerDark.css", cancellationToken);

            return Results.File(css, "text/css");
        }).ExcludeFromDescription();

        return app;
    }
}