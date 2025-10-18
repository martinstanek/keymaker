using System;
using System.Threading;
using System.Threading.Tasks;
using Keymaker.Model;

namespace Keymaker.Service.Acme;

public interface IAcmeService
{
    Task RequestCertificateViaDnsChallengeAsync(
        CertificateParameters certificateParameters,
        CloudFlareDnsServiceConfiguration cloudFlareDnsConfig,
        CancellationToken cancellationToken);

    Task RequestCertificateViaHttpChallengeAsync(
        CertificateParameters certificateParameters,
        CancellationToken cancellationToken);

    event EventHandler DnsValueSet;

    event EventHandler DnsValuePropagated;

    event EventHandler HttpChallengeTriggered;

    event EventHandler DnsChallengeTriggered;

    event EventHandler Failed;

    event EventHandler Succeeded;
}