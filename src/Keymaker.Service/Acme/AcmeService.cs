using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Certes;
using Certes.Acme;
using Keymaker.Service.Acme.Callback;
using Keymaker.Service.Acme.Factories;
using Keymaker.Service.Acme.Model;
using Keymaker.Service.Store;
using Microsoft.Extensions.Logging;

namespace Keymaker.Service.Acme;

public sealed class AcmeService : IAcmeService
{
    private readonly IAcmeContextFactory _acmeContextFactory;
    private readonly IAcmeCallback _acmeCallback;
    private readonly ICertStoreService _certStoreService;
    private readonly ILogger<AcmeService> _logger;

    public AcmeService(
        IAcmeContextFactory acmeContextFactory,
        IAcmeCallback acmeCallback,
        ICertStoreService certStoreService,
        ILogger<AcmeService> logger)
    {
        _acmeContextFactory = acmeContextFactory;
        _acmeCallback = acmeCallback;
        _certStoreService = certStoreService;
        _logger = logger;
    }

    public async Task<string> GetCertificateAsync(CertificateParameters certificateParameters, uint waitForResponseSeconds, CancellationToken cancellationToken)
    {
        _logger.LogDebug($"Getting the certificate for {certificateParameters.Domain}");

        ArgumentOutOfRangeException.ThrowIfZero(waitForResponseSeconds);

        var order = await PlaceOrderAsync(certificateParameters);

        _logger.LogDebug($"Order negotiated {order.Location}");

        await TriggerChallengeAsync(order, waitForResponseSeconds, cancellationToken);

        return await GetCertificateBase64StringAsync(order, certificateParameters);
    }

    private async Task<IOrderContext> PlaceOrderAsync(CertificateParameters certificateParameters)
    {
        var acme = _acmeContextFactory.GetAcmeContext();

        await acme.NewAccount(certificateParameters.Contact, termsOfServiceAgreed: true);

        return await acme.NewOrder([certificateParameters.Domain]);
    }

    private async Task TriggerChallengeAsync(IOrderContext order, uint waitForResponseSeconds, CancellationToken cancellationToken)
    {
        var authorize = (await order.Authorizations()).First();
        var httpChallenge = await authorize.Http();

        PrepareForChallenge(httpChallenge);

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        _logger.LogDebug($"Validating the challenge {httpChallenge.Type}");

        await httpChallenge.Validate();

        var i = 0;

        while (!cancellationToken.IsCancellationRequested && !_acmeCallback.Hit.HasValue && i++ < waitForResponseSeconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            _logger.LogDebug($"Waiting ... {i}/{waitForResponseSeconds}s");
        }
    }

    private async Task<string> GetCertificateBase64StringAsync(IOrderContext order, CertificateParameters certificateParameters)
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

        await _certStoreService.PersistCertificatesAsync(certificateParameters.Domain, pem, pemKey);

        return base64;
    }

    private void PrepareForChallenge(IChallengeContext challengeContext)
    {
        var keyAuthorize = challengeContext.KeyAuthz;
        var str = keyAuthorize.Split('.');

        _acmeCallback.Token = str[0];
        _acmeCallback.Thumbprint = str[1];
        _acmeCallback.Location = challengeContext.Location.ToString();
    }
}