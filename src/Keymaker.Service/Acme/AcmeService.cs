using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Keymaker.Service.Dns;
using Keymaker.Service.Store;
using Keymaker.Service.Acme.Dns;
using Keymaker.Service.Acme.Callback;
using Keymaker.Service.Acme.Factories;
using Keymaker.Service.Acme.Certificates;
using Keymaker.Service.Acme.Http;
using Certes;
using Certes.Acme;
using Keymaker.Service.Model;

namespace Keymaker.Service.Acme;

public sealed class AcmeService : IAcmeService
{
    private const int CheckDnsPropagationEverySeconds = 10;
    private const int WaitForDnsPropagationSeconds = 3600;
    private const int WaitForHttpCallbackSeconds = 60;

    private readonly IAcmeContextFactory _acmeContextFactory;
    private readonly IAcmeCallback _acmeCallback;
    private readonly ICertProducer _certProducer;
    private readonly IDnsService _dnsService;
    private readonly IDnsProvider _dnsProvider;
    private readonly IHttpProvider _httpProvider;
    private readonly ILogger<AcmeService> _logger;

    public AcmeService(
        IAcmeContextFactory acmeContextFactory,
        IAcmeCallback acmeCallback,
        ICertProducer certProducer,
        IDnsService dnsService,
        IDnsProvider dnsProvider,
        IHttpProvider httpProvider,
        ILogger<AcmeService> logger)
    {
        _acmeContextFactory = acmeContextFactory;
        _acmeCallback = acmeCallback;
        _certProducer = certProducer;
        _dnsService = dnsService;
        _dnsProvider = dnsProvider;
        _httpProvider = httpProvider;
        _logger = logger;
    }

    public Task RequestCertificateAsync(ChallengeMode challengeMode, CertificateParameters certificateParameters, CancellationToken cancellationToken)
    {
        return challengeMode switch
        {
            ChallengeMode.Dns => RequestCertificateViaDnsChallengeAsync(certificateParameters, cancellationToken),
            ChallengeMode.Http => RequestCertificateViaHttpChallengeAsync(certificateParameters, cancellationToken),
            _ => throw new NotSupportedException()
        };
    }

    private async Task RequestCertificateViaDnsChallengeAsync(CertificateParameters certificateParameters, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Getting the certificate for {certificateParameters.Domain}");

        var (acme, order) = await PlaceOrderAsync(certificateParameters);

        _logger.LogInformation($"Order negotiated {order.Location}");

        DnsChallengeTriggered.Invoke(this, EventArgs.Empty);

        await PerformChallengeAsync(acme, order, isDnsChallenge: true, cancellationToken);
        await FinaliseOrderAsync(order, certificateParameters);
    }

    private async Task RequestCertificateViaHttpChallengeAsync(CertificateParameters certificateParameters, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Getting the certificate for {certificateParameters.Domain}");

        var (acme, order) = await PlaceOrderAsync(certificateParameters);

        _logger.LogInformation($"Order negotiated {order.Location}");

        await PerformChallengeAsync(acme, order, isDnsChallenge: false, cancellationToken);

        HttpChallengeTriggered.Invoke(this, EventArgs.Empty);

        await WaitForHttpCallbackAsync(cancellationToken);
        await FinaliseOrderAsync(order, certificateParameters);
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
        CancellationToken cancellationToken)
    {
        var authorize = (await order.Authorizations()).First();
        var challenge = isDnsChallenge
            ? await PrepareForDnsChallengeAsync(acme, authorize, cancellationToken)
            : await PrepareForHttpChallengeAsync(authorize);

        var validatedChallenge = await challenge.Validate();

        _logger.LogInformation($"Validating challenge: {validatedChallenge.Type}");
    }

    private async Task WaitForHttpCallbackAsync(CancellationToken cancellationToken)
    {
        var i = 0;

        while (!cancellationToken.IsCancellationRequested && !_acmeCallback.Hit.HasValue && i++ < WaitForHttpCallbackSeconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }

    private async Task FinaliseOrderAsync(IOrderContext order, CertificateParameters certificateParameters)
    {
        _logger.LogInformation("Generating the certificate");

        var cert = await _certProducer.BuildCertificateAsync(order, certificateParameters);
        var certPersistenceInfo = new CertificatePersistenceInfo
        {
            Issuer = cert.Issuer,
            Obtained = DateTime.Now,
            Domain = cert.Domain,
            Expiry = cert.Expiry,
            FullChainPem = cert.Pem,
            PrivateKeyPem = cert.PemKey,
            Base64Pfx = cert.Base64,
            Base64FullChainPem = Convert.ToBase64String(Encoding.ASCII.GetBytes(cert.Pem)),
            Base64PrivateKeyPem = Convert.ToBase64String(Encoding.ASCII.GetBytes(cert.PemKey)),
            Password = certificateParameters.Password
        };

        Succeeded.Invoke(this, certPersistenceInfo);

        _logger.LogInformation($"The certificate generated, with expiry: {certPersistenceInfo.Expiry}");
    }

    private async Task<IChallengeContext> PrepareForHttpChallengeAsync(IAuthorizationContext authorize)
    {
        var httpChallenge = await _httpProvider.GetHttpChallenge(authorize);
        var keyAuthorize = _httpProvider.GetHttpAuthz(httpChallenge);
        var str = keyAuthorize.Split('.');

        _acmeCallback.Token = str[0];
        _acmeCallback.Thumbprint = str[1];
        _acmeCallback.Location = httpChallenge.Location.ToString();

        return httpChallenge;
    }

    private async Task<IChallengeContext> PrepareForDnsChallengeAsync(
        IAcmeContext acme,
        IAuthorizationContext authorize,
        CancellationToken cancellationToken)
    {
        var dnsChallenge = await _dnsProvider.GetDnsChallengeAsync(authorize);
        var dnsTxt = _dnsProvider.GetDnsTxtValue(dnsChallenge, acme);
        var i = 0;

        await _dnsService.AddTxtEntryAsync(dnsTxt);

        DnsValueSet.Invoke(this, EventArgs.Empty);

        while (!cancellationToken.IsCancellationRequested && i++ < WaitForDnsPropagationSeconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            if (i % CheckDnsPropagationEverySeconds != 0)
            {
                continue;
            }

            var preparedKey = await _dnsService.GetTxtEntryAsync();

            _logger.LogInformation($"Waiting for the DNS propagation. Expected value: {dnsTxt}, current value: {preparedKey}");

            if (preparedKey.Contains(dnsTxt))
            {
                DnsValuePropagated.Invoke(this, EventArgs.Empty);

                return dnsChallenge;
            }
        }

        throw new InvalidOperationException("DNS entry not prepared");
    }

    public event EventHandler DnsValueSet = (_, _) => { };

    public event EventHandler DnsValuePropagated = (_, _) => { };

    public event EventHandler HttpChallengeTriggered = (_, _) => { };

    public event EventHandler DnsChallengeTriggered = (_, _) => { };

    public event EventHandler Failed = (_, _) => { };

    public event EventHandler<CertificatePersistenceInfo> Succeeded = (_, _) => { };
}