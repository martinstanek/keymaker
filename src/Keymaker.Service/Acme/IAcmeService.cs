using System;
using System.Threading;
using System.Threading.Tasks;
using Keymaker.Model;
using Keymaker.Service.Store;

namespace Keymaker.Service.Acme;

public interface IAcmeService
{
    Task RequestCertificateAsync(ChallengeMode challengeMode, CertificateParameters certificateParameters, CancellationToken cancellationToken);

    event EventHandler DnsValueSet;

    event EventHandler DnsValuePropagated;

    event EventHandler HttpChallengeTriggered;

    event EventHandler DnsChallengeTriggered;

    event EventHandler Failed;

    event EventHandler<CertificatePersistenceInfo> Succeeded;
}