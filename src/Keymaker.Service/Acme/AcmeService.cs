using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Certes;
using Certes.Acme;
using Keymaker.Service.Acme.Callback;
using Keymaker.Service.Acme.Factories;
using Keymaker.Service.Acme.Model;
using Keymaker.Service.Dns;
using Keymaker.Service.Model;
using Keymaker.Service.Store;
using Microsoft.Extensions.Logging;

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
    private readonly IDnsService _dnsService;
    private readonly ILogger<AcmeService> _logger;

    public AcmeService(
        IAcmeContextFactory acmeContextFactory,
        IAcmeCallback acmeCallback,
        ICertStoreService certStoreService,
        IDnsService dnsService,
        ILogger<AcmeService> logger)
    {
        _acmeContextFactory = acmeContextFactory;
        _acmeCallback = acmeCallback;
        _certStoreService = certStoreService;
        _dnsService = dnsService;
        _logger = logger;
    }

    public async Task RequestCertificateViaDnsChallengeAsync(CertificateParameters certificateParameters, CancellationToken cancellationToken)
    {
        _logger.LogDebug($"Getting the certificate for {certificateParameters.Domain}");

        var (acme, order) = await PlaceOrderAsync(certificateParameters);

        _logger.LogDebug($"Order negotiated {order.Location}");

        await PerformChallengeAsync(
            acme,
            order,
            isDnsChallenge: true,
            certificateParameters,
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
        CancellationToken cancellationToken)
    {
        var authorize = (await order.Authorizations()).First();
        var challenge = isDnsChallenge
            ? await PrepareForDnsChallengeAsync(acme, authorize, certificateParameters, cancellationToken)
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

        var privateKey = KeyFactory.NewKey(KeyAlgorithm.RS256);
        var certInfo = certificateParameters.AsCsrInfo();
        var cert = await order.Generate(certInfo, privateKey);
        var pfxBuilder = cert.ToPfx(privateKey);
        var pfx = pfxBuilder.Build(certificateParameters.CertificateName, certificateParameters.Password);
        var pem = cert.ToPem(privateKey);
        var pemKey = privateKey.ToPem();
        var base64 = Convert.ToBase64String(pfx);

        await _certStoreService.PersistCertificatesAsync(certificateParameters.Domain, pem, pemKey, base64);
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

    private async Task<IChallengeContext> PrepareForDnsChallengeAsync(IAcmeContext acme, IAuthorizationContext authorize, CertificateParameters parameters, CancellationToken cancellationToken)
    {
        var dnsChallenge = await authorize.Dns();
        var dnsTxt = acme.AccountKey.DnsTxt(dnsChallenge.Token);
        var i = 0;

        await _dnsService.AddTxtEntryAsync(parameters.DnsChallengeSetDomain, dnsTxt);

        while (!cancellationToken.IsCancellationRequested && i++ < WaitForDnsPropagationSeconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            if (i % CheckDnsPropagationEverySeconds != 0)
            {
                continue;
            }

            var preparedKey = await _dnsService.GetTxtEntryAsync(parameters.DnsChallengeCheckDomain);

            _logger.LogDebug($"Waiting for the DNS propagation at {parameters.DnsChallengeCheckDomain}, expected value: {dnsTxt}, current value: {preparedKey}");

            if (preparedKey.Contains(dnsTxt))
            {
                return dnsChallenge;
            }
        }

        throw new InvalidOperationException("DNS not prepared");
    }
}