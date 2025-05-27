using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Keymaker.Api.Handlers;

namespace Keymaker.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseCustomEndpoints(this WebApplication webApplication)
    {
        var api = webApplication.MapGroup("/");

        api.MapGet("/certificate/dns", (
                [FromServices] CertificateHandler handler)
            => handler.TriggerDnsChallengeAsync())
            .Produces<NoContentResult>();

        api.MapGet("/certificate/http", (
                    [FromServices] CertificateHandler handler)
                => handler.TriggerHttpChallenge())
            .Produces<NoContentResult>();

        api.MapGet("/dns/txt", async (
                    [FromServices] DnsHandler handler,
                    [FromQuery] [Required] string domain)
                => await handler.GetDnsTxtEntryAsync(domain))
            .Produces<string>(contentType: "test/plain");

        return webApplication;
    }
}