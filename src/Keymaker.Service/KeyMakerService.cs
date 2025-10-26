using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Keymaker.Model;
using Keymaker.Service.Acme;
using Keymaker.Service.Configuration;
using Keymaker.Service.Store;

namespace Keymaker.Service;

// TODO lifecycle management, docker kill signal, docker health, fluent validation
// TODO toggle api, ui, swagger ...

public sealed class KeyMakerService : IKeymakerService
{
    private readonly IAcmeService _acmeService;
    private readonly ICertStoreService _storeService;
    private readonly CertificateParameters _certificateParameters;
    private readonly KeyMakerConfiguration _keyMakerConfiguration;

    private ChallengeStatus _challengeStatus = ChallengeStatus.Empty;

    public KeyMakerService(
        IAcmeService acmeService,
        ICertStoreService storeService,
        KeyMakerConfiguration keyMakerConfiguration,
        CertificateParameters certificateParameters)
    {
        _acmeService = acmeService;
        _storeService = storeService;
        _keyMakerConfiguration = keyMakerConfiguration;
        _certificateParameters = certificateParameters;

        _acmeService.Succeeded += (_, _) => { SetState(CertificateRequestStatus.Success); };
        _acmeService.Failed += (_, _) => { SetState(CertificateRequestStatus.Failed); };
        _acmeService.HttpChallengeTriggered += (_, _) => { SetState(CertificateRequestStatus.WaitingForHttpVerification); };
    }

    public bool RequestCertificate(CancellationToken token)
    {
        if (!CanProcessRequest())
        {
            return false;
        }

        StartChallenge(token);

        return true;
    }

    public void CancelCurrentChallenge() { }

    public ChallengeStatus GetCurrentRequestStatus()
    {
        return _challengeStatus;
    }

    public Task<CertificateInfo> GetMostRecentCertificateInfoAsync()
    {
        return _storeService.GetMostRecentCertificateInfoAsync();
    }

    public Task<ChallengeInfo> GetChallengeInfoAsync()
    {
        return Task.FromResult(ChallengeInfo.Empty);
    }

    private void StartChallenge(CancellationToken token)
    {
        Task.Factory.StartNew(
            () => _acmeService.RequestCertificateAsync(_keyMakerConfiguration.ChallengeMode, _certificateParameters, token),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

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