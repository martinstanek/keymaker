using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Keymaker.Model;
using Keymaker.Service.Acme;

namespace Keymaker.Service;

// TODO lifecycle management, docker kill signal, docker health

public sealed class KeyMakerService : IKeymakerService
{
    private readonly IAcmeService _acmeService;
    private readonly CertificateParameters _certificateParameters;
    private readonly DnsServiceConfiguration _dnsServiceConfiguration;

    public KeyMakerService(IAcmeService acmeService, CertificateParameters certificateParameters, DnsServiceConfiguration dnsServiceConfiguration)
    {
        _acmeService = acmeService;
        _certificateParameters = certificateParameters;
        _dnsServiceConfiguration = dnsServiceConfiguration;
    }

    public void RequestCertificate(CertificateRequestChallengeType challengeType, CancellationToken token)
    {
        /*
        Task.Factory.StartNew(
            () => _acmeService.RequestCertificateViaDnsChallengeAsync(_parameters, CancellationToken.None),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        */

        throw new System.NotImplementedException();
    }

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