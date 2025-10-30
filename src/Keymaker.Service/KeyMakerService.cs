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
    private readonly AzureKeyVaultStoreConfiguration _azureVaultConfiguration;
    private readonly VolumeStoreConfiguration _volumeStoreConfiguration;

    private ChallengeStatus _challengeStatus = ChallengeStatus.Empty;

    public KeyMakerService(
        IAcmeService acmeService,
        ICertStoreService storeService,
        KeyMakerConfiguration keyMakerConfiguration,
        AzureKeyVaultStoreConfiguration azureVaultConfiguration,
        VolumeStoreConfiguration volumeStoreConfiguration,
        CertificateParameters certificateParameters)
    {
        _acmeService = acmeService;
        _storeService = storeService;
        _keyMakerConfiguration = keyMakerConfiguration;
        _azureVaultConfiguration = azureVaultConfiguration;
        _volumeStoreConfiguration = volumeStoreConfiguration;
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
            RenewEveryDay = _keyMakerConfiguration.RenewEveryDays,
            NextRenewal = lastCert.IsEmpty() ? null : lastCert.Obtained.AddDays(_keyMakerConfiguration.RenewEveryDays),
            Expiry = lastCert.IsEmpty() ? null : lastCert.Expiry,
            Obtained = lastCert.IsEmpty() ? null : lastCert.Obtained,
            Issuer = lastCert.Issuer,
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
}