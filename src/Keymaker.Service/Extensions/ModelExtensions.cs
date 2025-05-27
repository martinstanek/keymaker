using Certes;
using Keymaker.Model;

namespace Keymaker.Service.Extensions;

internal static class ModelExtensions
{
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
}