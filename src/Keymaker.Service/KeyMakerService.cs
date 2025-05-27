using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Keymaker.Model;
using Keymaker.Service.Acme;

namespace Keymaker.Service;

// TODO lifecycle management, docker kill signal, docker health, fluent validation

public sealed class KeyMakerService : IKeymakerService
{
    private readonly DnsServiceConfiguration _dnsServiceConfiguration;
    private readonly CertificateParameters _certificateParameters;
    private readonly IAcmeService _acmeService;

    public KeyMakerService(IAcmeService acmeService, CertificateParameters certificateParameters, DnsServiceConfiguration dnsServiceConfiguration)
    {
        _acmeService = acmeService;
        _certificateParameters = certificateParameters;
        _dnsServiceConfiguration = dnsServiceConfiguration;
    }

    public bool RequestCertificate(CertificateRequestChallengeType challengeType, CancellationToken token)
    {
        switch (challengeType)
        {
            case CertificateRequestChallengeType.Dns:
                Task.Factory.StartNew(
                    () => _acmeService.RequestCertificateViaDnsChallengeAsync(_certificateParameters, _dnsServiceConfiguration, token),
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
                break;
            case CertificateRequestChallengeType.Http:
                Task.Factory.StartNew(
                    () => _acmeService.RequestCertificateViaHttpChallengeAsync(_certificateParameters, token),
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(challengeType), challengeType, null);
        }

        return true;
    }

    public void CancelCurrentChallenge() { }

    public ChallengeParameters GetChallengeParameters()
    {
        return new ChallengeParameters
        {
            CertificateName = _certificateParameters.CertificateName,
            Contact = _certificateParameters.Contact,
            CountryName = _certificateParameters.CountryName,
            Domain = _certificateParameters.Domain,
            Locality = _certificateParameters.Locality,
            Organization = _certificateParameters.Organization,
            OrganizationUnit = _certificateParameters.OrganizationUnit,
            State = _certificateParameters.State,
            DnsChallengeCheckDomain = _dnsServiceConfiguration.DnsChallengeCheckDomain,
            DnsChallengeSetDomain = _dnsServiceConfiguration.DnsChallengeSetDomain
        };
    }

    public ChallengeStatus GetCurrentRequestStatus()
    {
        return ChallengeStatus.Empty;
    }

    public Task<ImmutableArray<CertificateInfo>> GetPersistedCertificatesAsync()
    {
        return Task.FromResult(ImmutableArray.Create(CertificateInfo.Empty));
    }
}