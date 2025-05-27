using System;
using Keymaker.Model;

namespace Keymaker.Service.Configuration;

public static class EnvironmentReader
{
    public static CertificateParameters GetCertificateParametersFromEnvironment()
    {
        return new CertificateParameters
        {
            Contact = Environment.GetEnvironmentVariable("KEYMAKER_CONTACT") ?? string.Empty,
            Domain = Environment.GetEnvironmentVariable("KEYMAKER_DOMAIN") ?? string.Empty,
            CertificateName = Environment.GetEnvironmentVariable("KEYMAKER_CERTNAME") ?? string.Empty,
            Password = Environment.GetEnvironmentVariable("KEYMAKER_PASSWORD") ?? string.Empty,
            CountryName = Environment.GetEnvironmentVariable("KEYMAKER_COUNTRY") ?? string.Empty,
            State = Environment.GetEnvironmentVariable("KEYMAKER_STATE") ?? string.Empty,
            Locality = Environment.GetEnvironmentVariable("KEYMAKER_LOCALITY") ?? string.Empty,
            Organization = Environment.GetEnvironmentVariable("KEYMAKER_ORG") ?? string.Empty,
            OrganizationUnit = Environment.GetEnvironmentVariable("KEYMAKER_UNIT") ?? string.Empty
        };
    }

    public static DnsServiceConfiguration GetDnsServiceConfigurationFromEnvironment()
    {
        return new DnsServiceConfiguration
        {
            Email = Environment.GetEnvironmentVariable("KEYMAKER_DNSAPIEMAIL") ?? string.Empty,
            Key = Environment.GetEnvironmentVariable("KEYMAKER_DNSAPIKEY") ?? string.Empty,
            Zone = Environment.GetEnvironmentVariable("KEYMAKER_DNSAPIZONE") ?? string.Empty,
            DnsChallengeCheckDomain = Environment.GetEnvironmentVariable("KEYMAKER_DNSCHECKDOMAIN") ?? string.Empty,
            DnsChallengeSetDomain = Environment.GetEnvironmentVariable("KEYMAKER_DNSSETDOMAIN") ?? string.Empty
        };
    }
}