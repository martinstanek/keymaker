using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Keymaker.Model;
using Keymaker.Service.Acme;
using Keymaker.Service.Store;

namespace Keymaker.Service;

// TODO lifecycle management, docker kill signal, docker health, fluent validation

public sealed class KeyMakerService : IKeymakerService
{
    private readonly DnsServiceConfiguration _dnsServiceConfiguration;
    private readonly CertificateParameters _certificateParameters;
    private readonly ICertStoreService _storeService;
    private readonly IAcmeService _acmeService;
    private ChallengeStatus _challengeStatus = ChallengeStatus.Empty;

    public KeyMakerService(
        IAcmeService acmeService,
        ICertStoreService storeService,
        CertificateParameters certificateParameters,
        DnsServiceConfiguration dnsServiceConfiguration)
    {
        _acmeService = acmeService;
        _storeService = storeService;
        _certificateParameters = certificateParameters;
        _dnsServiceConfiguration = dnsServiceConfiguration;

        _acmeService.Succeeded += (_, _) => { SetState(CertificateRequestStatus.Success); };
        _acmeService.Failed += (_, _) => { SetState(CertificateRequestStatus.Failed); };
        _acmeService.HttpChallengeTriggered += (_, _) => { SetState(CertificateRequestStatus.WaitingForHttpVerification); };
    }

    public bool RequestCertificate(CertificateRequestChallengeType challengeType, CancellationToken token)
    {
        if (!CanProcessRequest())
        {
            return false;
        }

        StartChallenge(challengeType, token);

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
        return _challengeStatus;
    }

    public Task<ImmutableArray<CertificateInfo>> GetPersistedCertificatesAsync()
    {
        return _storeService.GetCertificatesAsync();
    }

    private void StartChallenge(CertificateRequestChallengeType challengeType, CancellationToken token)
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
                throw new NotSupportedException();
        }

        _challengeStatus = _challengeStatus with
        {
            Status = CertificateRequestStatus.Started,
            Requested = DateTime.UtcNow
        };
    }

    private void SetState(CertificateRequestStatus status)
    {
        _challengeStatus = _challengeStatus with
        {
            Status = status
        };
    }

    private bool CanProcessRequest()
    {
        var allowedStates = new[]
        {
            CertificateRequestStatus.Failed,
            CertificateRequestStatus.Success,
            CertificateRequestStatus.TimeOut,
            CertificateRequestStatus.Idle
        };

        return allowedStates.Contains(_challengeStatus.Status);
    }
}