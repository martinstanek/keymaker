namespace Awitec.Framework.Acme.Model
{
    public class CertificateParameters
    {
        public string Contact { get; set; } = string.Empty;

        public string Domain { get; set; } = string.Empty;

        public string CertificateName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string CountryName { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string Locality { get; set; } = string.Empty;

        public string Organization { get; set; } = string.Empty;

        public string OrganizationUnit { get; set; } = string.Empty;
    }
}