using Certes;
using Keymaker.Service.Configuration.Certificate;
using Keymaker.Service.Model;

namespace Keymaker.Service.Extensions;

internal static class ModelExtensions
{
    internal static CsrInfo AsCsrInfo(this CertificateConfiguration certificateConfiguration)
    {
        return new CsrInfo
        {
            CountryName = certificateConfiguration.CountryName,
            State = certificateConfiguration.State,
            Locality = certificateConfiguration.Locality,
            Organization = certificateConfiguration.Organization,
            OrganizationUnit = certificateConfiguration.OrganizationUnit,
            CommonName = certificateConfiguration.Domain
        };
    }

    internal static string GetOrganisation(this CertificateConfiguration certificateConfiguration)
    {
        return $"{certificateConfiguration.OrganizationUnit}, {certificateConfiguration.Organization}, {certificateConfiguration.Locality}, {certificateConfiguration.State}, {certificateConfiguration.CountryName}";
    }
}