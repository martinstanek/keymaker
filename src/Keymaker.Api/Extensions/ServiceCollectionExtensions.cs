using System;
using Keymaker.Api.Handlers;
using Keymaker.Service.Dns;
using Keymaker.Service.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Keymaker.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureHandlers(this IServiceCollection services)
    {
        var requestConfig = new CertificateParameters
        {
            Contact = Environment.GetEnvironmentVariable("KEYMAKER_CONTACT") ?? string.Empty,
            Domain = Environment.GetEnvironmentVariable("KEYMAKER_DOMAIN") ?? string.Empty,
            CertificateName = Environment.GetEnvironmentVariable("KEYMAKER_CERTNAME") ?? string.Empty,
            Password = Environment.GetEnvironmentVariable("KEYMAKER_PASSWORD") ?? string.Empty,
            CountryName = Environment.GetEnvironmentVariable("KEYMAKER_COUNTRY") ?? string.Empty,
            State = Environment.GetEnvironmentVariable("KEYMAKER_STATE") ?? string.Empty,
            Locality = Environment.GetEnvironmentVariable("KEYMAKER_LOCALITY") ?? string.Empty,
            Organization = Environment.GetEnvironmentVariable("KEYMAKER_ORG") ?? string.Empty,
            OrganizationUnit = Environment.GetEnvironmentVariable("KEYMAKER_UNIT") ?? string.Empty,
            DnsChallengeCheckDomain = Environment.GetEnvironmentVariable("KEYMAKER_DNSCHECKDOMAIN") ?? string.Empty,
            DnsChallengeSetDomain = Environment.GetEnvironmentVariable("KEYMAKER_DNSSETDOMAIN") ?? string.Empty
        };

        var dnsConfig = new DnsServiceConfiguration
        {
            Email = Environment.GetEnvironmentVariable("KEYMAKER_DNSAPIEMAIL") ?? string.Empty,
            Key = Environment.GetEnvironmentVariable("KEYMAKER_DNSAPIKEY") ?? string.Empty,
            Zone = Environment.GetEnvironmentVariable("KEYMAKER_DNSAPIZONE") ?? string.Empty
        };

        return services
            .AddSingleton(dnsConfig)
            .AddSingleton(requestConfig)
            .AddSingleton<DnsHandler>()
            .AddSingleton<CertificateHandler>();
    }
}