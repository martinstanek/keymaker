using System.Threading;
using Keymaker.Api.Handlers;
using Keymaker.Api.Services.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Keymaker.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseCustomEndpoints(this WebApplication webApplication)
    {
        var api = webApplication.MapGroup("/");

        api.MapPost("/certificate", async (
                [FromServices] CertificateHandler handler,
                [FromBody] CertificateParameters? parameters,
                CancellationToken cancellationToken)
            => await handler.GetCertificateAsync(parameters, cancellationToken))
            .Produces<string>(contentType: "text/plain");

        return webApplication;
    }
}