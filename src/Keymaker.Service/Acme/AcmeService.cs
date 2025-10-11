using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Keymaker.Model;
using Keymaker.Service.Acme.Callback;
using Keymaker.Service.Acme.Factories;
using Keymaker.Service.Dns;
using Keymaker.Service.Store;
using Certes;
using Certes.Acme;
using Keymaker.Service.Acme.Certificates;
using Keymaker.Service.Acme.Dns;

namespace Keymaker.Service.Acme;

public sealed class AcmeService : IAcmeService
{
    private const int WaitForHttpCallbackSeconds = 60;
    private const int WaitForDnsPropagationSeconds = 3600;
    private const int WaitForDnsChallengeOrderFinalisationSeconds = 60;
    private const int CheckOrderEverySeconds = 10;
    private const int CheckDnsPropagationEverySeconds = 10;

    private readonly IAcmeContextFactory _acmeContextFactory;
    private readonly IAcmeCallback _acmeCallback;
    private readonly ICertStoreService _certStoreService;
    private readonly ICertProducer _certProducer;
    private readonly IDnsService _dnsService;
    private readonly IDnsProvider _dnsProvider;
    private readonly ILogger<AcmeService> _logger;

    public AcmeService(
        IAcmeContextFactory acmeContextFactory,
        IAcmeCallback acmeCallback,
        ICertStoreService certStoreService,
        ICertProducer certProducer,
        IDnsService dnsService,
        IDnsProvider dnsProvider,
        ILogger<AcmeService> logger)
    {
        _acmeContextFactory = acmeContextFactory;
        _acmeCallback = acmeCallback;
        _certStoreService = certStoreService;
        _certProducer = certProducer;
        _dnsService = dnsService;
        _dnsProvider = dnsProvider;
        _logger = logger;
    }

    public async Task RequestCertificateViaDnsChallengeAsync(CertificateParameters certificateParameters, DnsServiceConfiguration dnsConfig, CancellationToken cancellationToken)
    {
        _logger.LogDebug($"Getting the certificate for {certificateParameters.Domain}");

        var (acme, order) = await PlaceOrderAsync(certificateParameters);

        _logger.LogDebug($"Order negotiated {order.Location}");

        await PerformChallengeAsync(
            acme,
            order,
            isDnsChallenge: true,
            certificateParameters,
            dnsConfig,
            cancellationToken);

        await WaitForDnsOrderFinalisationAsync(order, certificateParameters, cancellationToken);
    }

    public async Task RequestCertificateViaHttpChallengeAsync(CertificateParameters certificateParameters, CancellationToken cancellationToken)
    {
        _logger.LogDebug($"Getting the certificate for {certificateParameters.Domain}");

        var (acme, order) = await PlaceOrderAsync(certificateParameters);

        _logger.LogDebug($"Order negotiated {order.Location}");

        await PerformChallengeAsync(
            acme,
            order,
            isDnsChallenge: false,
            certificateParameters,
            DnsServiceConfiguration.Empty,
            cancellationToken);
    }

    public async Task GetCertificateAsync(CertificateParameters certificateParameters, bool isWildCard, uint waitForResponseSeconds, CancellationToken cancellationToken)
    {
        _logger.LogDebug($"Getting the certificate for {certificateParameters.Domain}");

        ArgumentOutOfRangeException.ThrowIfZero(waitForResponseSeconds);

        var (acme, order) = await PlaceOrderAsync(certificateParameters);

        _logger.LogDebug($"Order negotiated {order.Location}");
    }

    private async Task<(IAcmeContext acmeContext, IOrderContext orderContext)> PlaceOrderAsync(CertificateParameters certificateParameters)
    {
        var acme = _acmeContextFactory.GetAcmeContext();

        await acme.NewAccount(certificateParameters.Contact, termsOfServiceAgreed: true);

        var order = await acme.NewOrder([certificateParameters.Domain]);

        return (acme, order);
    }

    private async Task PerformChallengeAsync(
        IAcmeContext acme,
        IOrderContext order,
        bool isDnsChallenge,
        CertificateParameters certificateParameters,
        DnsServiceConfiguration dnsConfig,
        CancellationToken cancellationToken)
    {
        var authorize = (await order.Authorizations()).First();
        var challenge = isDnsChallenge
            ? await PrepareForDnsChallengeAsync(acme, authorize, certificateParameters, dnsConfig, cancellationToken)
            : await PrepareForHttpChallengeAsync(authorize);

        var validatedChallenge = await challenge.Validate();

        _logger.LogDebug($"Validating challenge: {validatedChallenge.Type}");
    }

    private async Task WaitForHttpCallbackAsync(CancellationToken cancellationToken)
    {
        var i = 0;

        while (!cancellationToken.IsCancellationRequested && !_acmeCallback.Hit.HasValue && i++ < WaitForHttpCallbackSeconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }

    private async Task WaitForDnsOrderFinalisationAsync(IOrderContext order, CertificateParameters certificateParameters, CancellationToken cancellationToken)
    {
        var i = 0;

        while (!cancellationToken.IsCancellationRequested && i++ < WaitForDnsChallengeOrderFinalisationSeconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            if (i % CheckOrderEverySeconds != 0)
            {
                continue;
            }

            try
            {
                await FinaliseOrderAsync(order, certificateParameters);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }

    private async Task FinaliseOrderAsync(IOrderContext order, CertificateParameters certificateParameters)
    {
        _logger.LogDebug("Generating the certificate");

        var cert = await _certProducer.BuildCertificateAsync(order, certificateParameters);

        await _certStoreService.PersistCertificatesAsync(certificateParameters.Domain, cert.Pem, cert.PemKey, cert.Base64);
    }

    private async Task<IChallengeContext> PrepareForHttpChallengeAsync(IAuthorizationContext authorize)
    {
        var httpChallenge = await authorize.Http();
        var keyAuthorize = httpChallenge.KeyAuthz;
        var str = keyAuthorize.Split('.');

        _acmeCallback.Token = str[0];
        _acmeCallback.Thumbprint = str[1];
        _acmeCallback.Location = httpChallenge.Location.ToString();

        return httpChallenge;
    }

    private async Task<IChallengeContext> PrepareForDnsChallengeAsync(
        IAcmeContext acme,
        IAuthorizationContext authorize,
        CertificateParameters parameters,
        DnsServiceConfiguration dnsConfig,
        CancellationToken cancellationToken)
    {
        var dnsChallenge = await _dnsProvider.GetDnsChallengeAsync(authorize);
        var dnsTxt = _dnsProvider.GetDnsTxtValue(dnsChallenge, acme);
        var i = 0;

        await _dnsService.AddTxtEntryAsync(dnsConfig.DnsChallengeSetDomain, dnsTxt);

        while (!cancellationToken.IsCancellationRequested && i++ < WaitForDnsPropagationSeconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            if (i % CheckDnsPropagationEverySeconds != 0)
            {
                continue;
            }

            var preparedKey = await _dnsService.GetTxtEntryAsync(dnsConfig.DnsChallengeCheckDomain);

            _logger.LogDebug($"Waiting for the DNS propagation at {dnsConfig.DnsChallengeCheckDomain}, expected value: {dnsTxt}, current value: {preparedKey}");

            if (preparedKey.Contains(dnsTxt))
            {
                return dnsChallenge;
            }
        }

        throw new InvalidOperationException("DNS not prepared");
    }

    public event EventHandler DnsValueSet = (_, _) => { };

    public event EventHandler DnsValuePropagated = (_, _) => { };

    public event EventHandler HttpChallengeTriggered = (_, _) => { };

    public event EventHandler DnsChallengeTriggered = (_, _) => { };

    public event EventHandler Failed = (_, _) => { };

    public event EventHandler Succeeded = (_, _) => { };
}