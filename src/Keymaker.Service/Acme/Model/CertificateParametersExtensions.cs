using Certes;

namespace Keymaker.Service.Acme.Model;

public static class CertificateParametersExtensions
{
    public static CertificateParameters MergeWithDefaults(this CertificateParameters? certificateParameters, CertificateParameters defaults)
    {
        if (certificateParameters is null)
        {
            return defaults;
        }

        return certificateParameters with
        {
            Contact = UseNotEmpty(certificateParameters.Contact, defaults.Contact),
            Domain = UseNotEmpty(certificateParameters.Domain, defaults.Domain),
            CertificateName = UseNotEmpty(certificateParameters.CertificateName, defaults.CertificateName),
            Password = UseNotEmpty(certificateParameters.Password, defaults.Password),
            CountryName = UseNotEmpty(certificateParameters.CountryName, defaults.CountryName),
            State = UseNotEmpty(certificateParameters.State, defaults.State),
            Locality = UseNotEmpty(certificateParameters.Locality, defaults.Locality),
            Organization = UseNotEmpty(certificateParameters.Organization, defaults.Organization),
            OrganizationUnit = UseNotEmpty(certificateParameters.OrganizationUnit, defaults.OrganizationUnit),
            DnsChallengeCheckDomain = UseNotEmpty(certificateParameters.DnsChallengeCheckDomain, defaults.DnsChallengeCheckDomain),
            DnsChallengeSetDomain = UseNotEmpty(certificateParameters.DnsChallengeSetDomain, defaults.DnsChallengeSetDomain),
        };
    }

    internal static CsrInfo AsCsrInfo(this CertificateParameters certificateParameters)
    {
        return new CsrInfo
        {
            CountryName = certificateParameters.CountryName,
            State = certificateParameters.State,
            Locality = certificateParameters.Locality,
            Organization = certificateParameters.Organization,
            OrganizationUnit = certificateParameters.OrganizationUnit,
            CommonName = certificateParameters.Domain
        };
    }

    private static string UseNotEmpty(string currentValue, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(currentValue)
            ? defaultValue
            : currentValue;
    }
}