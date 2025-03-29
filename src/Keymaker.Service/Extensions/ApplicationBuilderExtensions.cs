using System;
using Keymaker.Service.Acme.Callback;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Keymaker.Service.Extensions;

public static class ApplicationBuilderExtensions
{
    private const string WellKnownAcmeChallengeUrl = "/.well-known/acme-challenge";
    private const string ResponseContentType = "plain/text";

    public static IApplicationBuilder UseKeymaker(this IApplicationBuilder app)
    {
        var callBack = app.ApplicationServices.GetRequiredService<IAcmeCallback>();

        if (callBack == null)
        {
            throw new InvalidOperationException();
        }

        app.Map(WellKnownAcmeChallengeUrl, sub =>
            sub.Run(async context =>
            {
                var path = context.Request.Path.ToUriComponent();

                if (!string.IsNullOrWhiteSpace(path) && path.Length > 1 && path.StartsWith("/", StringComparison.Ordinal))
                {
                    callBack.Hit = DateTime.Now;
                    context.Response.ContentType = ResponseContentType;

                    await context.Response.WriteAsync($"{path.Substring(1)}.{callBack.Thumbprint}");
                }
            }));

        return app;
    }
}