using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Keymaker.Api.Handlers;
using Keymaker.Model;

namespace Keymaker.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseCustomEndpoints(this WebApplication webApplication)
    {
        var api = webApplication.MapGroup("/");

        api.MapDelete("/challenge", ([FromServices] RequestHandler handler) => handler.CancelCurrentChallenge())
           .Produces<NoContentResult>();

        api.MapPut("/challenge", ( [FromServices] RequestHandler handler) => handler.TriggerChallengeAsync())
           .Produces<NoContentResult>();

        api.MapGet("/challenge/status", ( [FromServices] RequestHandler handler) => handler.GetChallengeStatus())
           .Produces<ChallengeStatus>();

        api.MapGet("/challenge/info", ( [FromServices] RequestHandler handler) => handler.GetChallengeInfoAsync())
           .Produces<ChallengeInfo>();

        api.MapGet("/certificate", async ([FromServices] RequestHandler handler) => await handler.GetMostRecentCertificateInfoAsync())
           .Produces<CertificateInfo>();

        api.MapGet("/console", ([FromServices] RequestHandler handler) => handler.GetConsole())
           .Produces<string>(contentType: "text/plain");

        api.MapGet("/console/clear", ([FromServices] RequestHandler handler) => handler.ClearConsole())
           .Produces<NoContentResult>();

        return webApplication;
    }

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