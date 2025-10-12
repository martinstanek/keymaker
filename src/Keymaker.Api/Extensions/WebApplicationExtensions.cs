using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
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

        api.MapGet("/", () => "keymaker")
           .Produces<string>(contentType: "test/plain");

        api.MapDelete("/challenge", ([FromServices] RequestHandler handler) => handler.CancelCurrentChallenge())
           .Produces<NoContentResult>();

        api.MapPut("/challenge/dns", ( [FromServices] RequestHandler handler) => handler.TriggerDnsChallengeAsync())
           .Produces<NoContentResult>();

        api.MapPut("/challenge/http", ( [FromServices] RequestHandler handler) => handler.TriggerHttpChallenge())
           .Produces<NoContentResult>();

        api.MapGet("/challenge/status", ( [FromServices] RequestHandler handler) => handler.GetChallengeStatus())
           .Produces<ChallengeStatus>();

        api.MapGet("/certificates", async ([FromServices] RequestHandler handler) => await handler.GetCertificatesAsync())
           .Produces<ImmutableArray<CertificateInfo>>();

        api.MapGet("/dns/txt", async ( [FromServices] RequestHandler handler, [FromQuery] [Required] string domain)
              => await handler.GetDnsTxtEntryAsync(domain))
           .Produces<string>(contentType: "test/plain");

        return webApplication;
    }
}