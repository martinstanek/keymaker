using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Keymaker.Model;
using Keymaker.Service.Acme;
using Keymaker.Service.Configuration;
using Keymaker.Service.Expiration;
using Keymaker.Service.Integrations;
using Keymaker.Service.Store;
using Microsoft.Extensions.Logging;

namespace Keymaker.Service;

// TODO lifecycle management, docker kill signal, docker health, fluent validation

public sealed class KeyMakerService : IKeymakerService
{
    private readonly IAcmeService _acmeService;
    private readonly ICertStoreService _storeService;
    private readonly IWebHookService _webHookService;
    private readonly IRenewalChecker _checker;
    private readonly CertificateParameters _certificateParameters;
    private readonly ILogger<KeyMakerService> _logger;
    private readonly KeyMakerConfiguration _keyMakerConfiguration;
    private readonly AzureKeyVaultStoreConfiguration _azureVaultConfiguration;
    private readonly VolumeStoreConfiguration _volumeStoreConfiguration;

    private ChallengeStatus _challengeStatus = ChallengeStatus.Empty;
    private NextChallenge _nextChallenge = NextChallenge.Empty;
    private string _version = string.Empty;

    public KeyMakerService(
        IAcmeService acmeService,
        ICertStoreService storeService,
        IWebHookService webHookService,
        IRenewalChecker checker,
        KeyMakerConfiguration keyMakerConfiguration,
        AzureKeyVaultStoreConfiguration azureVaultConfiguration,
        VolumeStoreConfiguration volumeStoreConfiguration,
        CertificateParameters certificateParameters,
        ILogger<KeyMakerService> logger)
    {
        _acmeService = acmeService;
        _storeService = storeService;
        _webHookService = webHookService;
        _checker = checker;
        _keyMakerConfiguration = keyMakerConfiguration;
        _azureVaultConfiguration = azureVaultConfiguration;
        _volumeStoreConfiguration = volumeStoreConfiguration;
        _certificateParameters = certificateParameters;
        _logger = logger;

        _checker.NextChallengeChecked += OnNextChallengeChecked;
        _acmeService.Succeeded += OnSuccess;
        _acmeService.Failed += (_, _) => { SetState(CertificateRequestStatus.Failed); };
        _acmeService.HttpChallengeTriggered += (_, _) => { SetState(CertificateRequestStatus.WaitingForHttpVerification); };
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
        CancelCurrentChallenge();

        return Task.CompletedTask;
    }

    public bool RequestCertificate(CancellationToken token)
    {
        if (!CanProcessRequest())
        {
            _logger.LogWarning($"Can not process the request, state is {_challengeStatus.Status}");
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

    public async Task<ChallengeInfo> GetChallengeInfoAsync()
    {
        var lastCert = await GetMostRecentCertificateInfoAsync();

        var info = new ChallengeInfo
        {
            Contact = _certificateParameters.Contact,
            CertificateName = _certificateParameters.CertificateName,
            Domain = _certificateParameters.Domain,
            DnsMode = _keyMakerConfiguration.DnsMode.ToString(),
            Status = _challengeStatus.Status.ToString(),
            ChallengeMode = _keyMakerConfiguration.ChallengeMode.ToString(),
            StoreMode = _keyMakerConfiguration.StorageMode.ToString(),
            IsAutoRenewalEnabled = _keyMakerConfiguration.IsAutoRenewalEnabled,
            RenewEveryHours = _keyMakerConfiguration.RenewEveryHours,
            NextRenewal = _nextChallenge.IsEmpty() ? null : _nextChallenge.NextNegotiation,
            Expiry = lastCert.IsEmpty() ? null : lastCert.Expiry,
            Obtained = lastCert.IsEmpty() ? null : lastCert.Obtained,
            Issuer = lastCert.Issuer,
            Server = GetVersion(),
            StoreTarget = GetStoreTarget(),
            Organization = GetOrganization()
        };

        return info;
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

        _logger.LogInformation($"Challenge started: {_keyMakerConfiguration.ChallengeMode}");
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

    private void OnNextChallengeChecked(object? sender, NextChallenge e)
    {
        _nextChallenge = e;
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
            CertificateRequestStatus.TimedOut,
            CertificateRequestStatus.Idle
        };

        return allowedStates.Contains(_challengeStatus.Status);
    }

    private string GetStoreTarget()
    {
        return _keyMakerConfiguration.StorageMode switch
        {
            StorageMode.KeyVault => _azureVaultConfiguration.CertificateName,
            StorageMode.Volume => _volumeStoreConfiguration.ToplevelFolder,
            _ => throw new NotSupportedException()
        };
    }

    private string GetOrganization()
    {
        return $"{_certificateParameters.OrganizationUnit}, {_certificateParameters.Organization}, {_certificateParameters.Locality}, {_certificateParameters.State}, {_certificateParameters.CountryName}";
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