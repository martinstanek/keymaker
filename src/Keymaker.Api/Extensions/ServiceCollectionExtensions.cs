using System;
using Keymaker.Api.Handlers;
using Keymaker.Service.Acme.Model;
using Keymaker.Service.Dns;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Keymaker.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        var defaults = configuration.GetSection(nameof(CertificateParameters)).Get<CertificateParameters>()
                       ?? CertificateParameters.Empty;

        var dnsConfig = new DnsServiceConfiguration
        {
            Email = Environment.GetEnvironmentVariable("KEYMAKER_DNSAPIEMAIL") ?? string.Empty,
            Key = Environment.GetEnvironmentVariable("KEYMAKER_DNSAPIKEY") ?? string.Empty,
            Zone = Environment.GetEnvironmentVariable("KEYMAKER_DNSAPIZONE") ?? string.Empty
        };

        return services
            .AddSingleton(defaults)
            .AddSingleton(dnsConfig)
            .AddSingleton<CertificateHandler>();
    }
}