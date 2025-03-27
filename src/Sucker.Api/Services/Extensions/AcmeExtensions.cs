using System;
using Awitec.Framework.Acme.Callback;
using Awitec.Framework.Acme.Factories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Awitec.Framework.Acme.Extensions
{
    public static class AcmeExtensions
    {
        private const string WellKnownAcmeChallengeUrl = "/.well-known/acme-challenge";
        private const string ResponseContentType = "plain/text";

        public static IApplicationBuilder UseAcmeHandler(this IApplicationBuilder app)
        {
            var callBack = app.ApplicationServices?.GetService<IAcmeCallback>();

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
                        await context.Response.WriteAsync($"{path.Substring(1)}.{callBack.Thumbprint}").ConfigureAwait(false);
                    }
                }));

            return app;
        }

        public static IServiceCollection AddAcme(this IServiceCollection services)
        {
            services
                .AddSingleton<IAcmeContextFactory, AcmeContextFactory>()
                .AddSingleton<IAcmeCallback, AcmeCallback>()
                .AddTransient<IAcmeProvider, AcmeProvider>();

            return services;
        }
    }
}