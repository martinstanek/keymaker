using System;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Keymaker.Model;
using Keymaker.Service.Acme;
using Keymaker.Service.Configuration.Certificate;
using Keymaker.Service.Configuration.Service;
using Keymaker.Service.Expiration;
using Keymaker.Service.Extensions;
using Keymaker.Service.Integrations;
using Keymaker.Service.Model;
using Keymaker.Service.Store;

namespace Keymaker.Service;

public sealed class KeyMakerService : IKeymakerService
{
    private readonly ICertStoreService _storeService;
    private readonly IWebHookService _webHookService;
    private readonly IAcmeService _acmeService;
    private readonly IRenewalChecker _checker;
    private readonly ILogger<KeyMakerService> _logger;
    private readonly CertificateConfiguration _certificateConfiguration;
    private readonly KeyMakerConfiguration _keyMakerConfiguration;

    private CertificateRequestStatus _challengeStatus = CertificateRequestStatus.Idle;
    private CancellationTokenSource _challengeTokenSource = new();
    private NextChallenge _nextChallenge = NextChallenge.Empty;
    private string _version = string.Empty;

    public KeyMakerService(
        IAcmeService acmeService,
        ICertStoreService storeService,
        IWebHookService webHookService,
        IRenewalChecker checker,
        KeyMakerConfiguration keyMakerConfiguration,
        CertificateConfiguration certificateConfiguration,
        ILogger<KeyMakerService> logger)
    {
        _acmeService = acmeService;
        _storeService = storeService;
        _webHookService = webHookService;
        _checker = checker;
        _keyMakerConfiguration = keyMakerConfiguration;
        _certificateConfiguration = certificateConfiguration;
        _logger = logger;

        _checker.NextChallengeChecked += OnNextChallengeChecked;
        _acmeService.HttpChallengeTriggered += (_, _) => { SetState(CertificateRequestStatus.WaitingForHttpVerification); };
        _acmeService.DnsValuePropagated += (_, _) => { SetState(CertificateRequestStatus.WaitingForDnsVerification); };
        _acmeService.DnsValueSet += (_, _) => { SetState(CertificateRequestStatus.WaitingForDnsPropagation); };
        _acmeService.Failed += (_, _) => { SetState(CertificateRequestStatus.Failed); };
        _acmeService.Succeeded += OnSuccess;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var challengeInfo = await GetChallengeInfoAsync();

        _logger.LogInformation($"Awitec Keymaker");
        _logger.LogInformation($"Server: {challengeInfo.Server}");
        _logger.LogInformation($"CertificateName: {challengeInfo.CertificateName}");
        _logger.LogInformation($"Domain: {challengeInfo.Domain}");
        _logger.LogInformation($"Organization: {challengeInfo.Organization}");
        _logger.LogInformation($"Challenge Mode: {challengeInfo.ChallengeMode}");
        _logger.LogInformation($"Store Mode: {challengeInfo.StoreMode}");
        _logger.LogInformation($"Store: {challengeInfo.StoreTarget}");
        _logger.LogInformation($"Autorenewal: {challengeInfo.IsAutoRenewalEnabled}");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Graceful shutdown requested.");

        CancelCurrentChallenge();

        return Task.CompletedTask;
    }

    public bool RequestCertificate(CancellationToken token)
    {
        return _keyMakerConfiguration.IsChallengeTriggerEnabled && RequestCertificateInternal(token);
    }

    public bool RequestCertificateInternal(CancellationToken token)
    {
        if (!CanProcessRequest())
        {
            _logger.LogWarning($"Can not process the request, state is {_challengeStatus}");
            return false;
        }

        StartChallenge(token);

        return true;
    }

    public void CancelCurrentChallenge()
    {
        _challengeTokenSource.Cancel();
    }

    public async Task<ChallengeInfo> GetChallengeInfoAsync()
    {
        var lastCert = await GetMostRecentCertificateInfoAsync();
        var info = new ChallengeInfo
        {
            Contact = _certificateConfiguration.Contact,
            CertificateName = _certificateConfiguration.CertificateName,
            DnsMode = _keyMakerConfiguration.DnsMode.ToString(),
            Status = _challengeStatus.ToString(),
            ChallengeMode = _keyMakerConfiguration.ChallengeMode.ToString(),
            StoreMode = _keyMakerConfiguration.StorageMode.ToString(),
            IsAutoRenewalEnabled = _keyMakerConfiguration.IsAutoRenewalEnabled,
            IsChallengeTriggerEnabled = _keyMakerConfiguration.IsChallengeTriggerEnabled,
            RenewEveryHours = _keyMakerConfiguration.RenewEveryHours,
            NextRenewal = _nextChallenge.IsEmpty() ? null : _nextChallenge.NextNegotiation,
            Expiry = lastCert.IsEmpty() ? null : lastCert.Expiry,
            Obtained = lastCert.IsEmpty() ? null : lastCert.Obtained,
            Domain = lastCert.IsEmpty() ? _certificateConfiguration.Domain : lastCert.Domain,
            Issuer = lastCert.Issuer,
            Server = GetVersion(),
            StoreTarget = _storeService.StoreName,
            Organization = _certificateConfiguration.GetOrganisation()
        };

        return info;
    }

    private Task<CertificateInfo> GetMostRecentCertificateInfoAsync()
    {
        return _storeService.GetMostRecentCertificateInfoAsync();
    }

    private async void OnSuccess(object? sender, CertificatePersistenceInfo e)
    {
        await _storeService.PersistCertificatesAsync(e);

        SetState(CertificateRequestStatus.Success);

        if (_keyMakerConfiguration.IsWebHookEnabled)
        {
            await _webHookService.TriggerWebHookAsync(e);
        }
    }

    private void StartChallenge(CancellationToken token)
    {
        _challengeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

        var linkedToken = _challengeTokenSource.Token;

        Task.Factory.StartNew(
            () => _acmeService.RequestCertificateAsync(_keyMakerConfiguration.ChallengeMode, _certificateConfiguration, linkedToken),
            linkedToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

        _challengeStatus = CertificateRequestStatus.Started;
        _logger.LogInformation($"Challenge started: {_keyMakerConfiguration.ChallengeMode}");
    }

    private void OnNextChallengeChecked(object? sender, NextChallenge e)
    {
        _nextChallenge = e;
    }

    private void SetState(CertificateRequestStatus status)
    {
        _challengeStatus = status;
    }

    private bool CanProcessRequest()
    {
        var allowedStates = new[]
        {
            CertificateRequestStatus.Failed,
            CertificateRequestStatus.Success,
            CertificateRequestStatus.TimedOut,
            CertificateRequestStatus.Idle
        };

        return allowedStates.Contains(_challengeStatus);
    }

    private string GetVersion()
    {
        if (string.IsNullOrWhiteSpace(_version))
        {
            _version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? string.Empty;
        }

        return _version;
    }
}