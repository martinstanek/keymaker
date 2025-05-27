using Microsoft.AspNetCore.Builder;

namespace Keymaker.Infra.OpenApi.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseSwaggerApiDoc(this WebApplication app, string apiTitle)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", apiTitle);
        });

        return app;
    }
}