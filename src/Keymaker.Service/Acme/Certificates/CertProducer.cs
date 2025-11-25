using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Keymaker.Service.Extensions;
using Certes;
using Certes.Acme;
using Keymaker.Service.Configuration.Certificate;

namespace Keymaker.Service.Acme.Certificates;

public sealed class CertProducer : ICertProducer
{
    public async Task<Certificate> BuildCertificateAsync(IOrderContext order, CertificateConfiguration certificateConfiguration)
    {
        var privateKey = KeyFactory.NewKey(KeyAlgorithm.RS256);
        var certInfo = certificateConfiguration.AsCsrInfo();
        var cert = await order.Generate(certInfo, privateKey);
        var pfxBuilder = cert.ToPfx(privateKey);
        var pfx = pfxBuilder.Build(certificateConfiguration.CertificateName, certificateConfiguration.Password);
        var pem = cert.ToPem(privateKey);
        var pemKey = privateKey.ToPem();
        var base64 = Convert.ToBase64String(pfx);
        var pfxCertificate = X509CertificateLoader.LoadPkcs12(pfx, certificateConfiguration.Password);

        return new Certificate
        {
            Base64 = base64,
            Pem = pem,
            PemKey = pemKey,
            Domain = certificateConfiguration.Domain,
            Issuer = pfxCertificate.Issuer,
            Expiry = pfxCertificate.NotAfter
        };
    }
}