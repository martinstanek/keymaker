using Certes;
using Keymaker.Service.Configuration.Certificate;

namespace Keymaker.Service.Extensions;

internal static class ModelExtensions
{
    extension(CertificateConfiguration certificateConfiguration)
    {
        internal CsrInfo AsCsrInfo()
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

        internal string GetOrganisation()
        {
            return $"{certificateConfiguration.OrganizationUnit}, {certificateConfiguration.Organization}, {certificateConfiguration.Locality}, {certificateConfiguration.State}, {certificateConfiguration.CountryName}";
        }
    }
}