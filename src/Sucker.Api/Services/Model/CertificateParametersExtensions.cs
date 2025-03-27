using Certes;

namespace Awitec.Framework.Acme.Model
{
    internal static class CertificateParametersExtensions
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
}
