using System;
using System.Threading.Tasks;
using Certes;
using Certes.Acme;
using Keymaker.Model;
using Keymaker.Service.Extensions;

namespace Keymaker.Service.Acme.Certificates;

public sealed class CertProducer : ICertProducer
{
    public async Task<Certificate> BuildCertificateAsync(IOrderContext order, CertificateParameters certificateParameters)
    {
        var privateKey = KeyFactory.NewKey(KeyAlgorithm.RS256);
        var certInfo = certificateParameters.AsCsrInfo();
        var cert = await order.Generate(certInfo, privateKey);
        var pfxBuilder = cert.ToPfx(privateKey);
        var pfx = pfxBuilder.Build(certificateParameters.CertificateName, certificateParameters.Password);
        var pem = cert.ToPem(privateKey);
        var pemKey = privateKey.ToPem();
        var base64 = Convert.ToBase64String(pfx);

        return new Certificate
        {
            Base64 = base64,
            Pem = pem,
            PemKey = pemKey
        };
    }
}