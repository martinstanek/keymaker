using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Keymaker.Api.Handlers;
using Keymaker.Service.Acme.Model;

namespace Keymaker.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseCustomEndpoints(this WebApplication webApplication)
    {
        var api = webApplication.MapGroup("/");

        api.MapPost("/certificate/dns", async (
                [FromServices] CertificateHandler handler,
                [FromBody] CertificateParameters? parameters)
            => await handler.TriggerDnsChallengeAsync(parameters))
            .Produces<NoContentResult>();

        api.MapPost("/certificate/http", async (
                    [FromServices] CertificateHandler handler,
                    [FromBody] CertificateParameters? parameters)
                => await handler.TriggerHttpChallenge(parameters))
            .Produces<NoContentResult>();

        api.MapPut("/dns", async (
                    [FromServices] DnsHandler handler,
                    [FromQuery] [Required] string domain,
                    [FromQuery] [Required] string value)
                => await handler.SetDnsTxtEntryAsync(domain, value))
            .Produces<NoContentResult>();

        api.MapGet("/dns", async (
                    [FromServices] DnsHandler handler,
                    [FromQuery] [Required] string domain)
                => await handler.GetDnsTxtEntryAsync(domain))
            .Produces<string>(contentType: "test/plain");

        return webApplication;
    }
}