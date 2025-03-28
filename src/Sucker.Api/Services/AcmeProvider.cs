using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sucker.Api.Services.Callback;
using Sucker.Api.Services.Factories;
using Sucker.Api.Services.Model;
using Certes;
using Certes.Acme;

namespace Sucker.Api.Services;

public sealed class AcmeProvider : IAcmeProvider
{
    private const int OneSecond = 1000;

    private readonly IAcmeContextFactory _acmeContextFactory;
    private readonly IAcmeCallback _acmeCallback;
    private readonly ILogger<AcmeProvider> _logger;

    public AcmeProvider(IAcmeContextFactory acmeContextFactory, IAcmeCallback acmeCallback, ILogger<AcmeProvider> logger)
    {
        _acmeContextFactory = acmeContextFactory;
        _acmeCallback = acmeCallback;
        _logger = logger;
    }

    public async Task<string> GetCertificateAsync(CertificateParameters certificateParameters, uint waitForResponseSeconds, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Getting the certificate for {certificateParameters.Domain}");

        await PersistPemsAsync("test", "test");

        /*

        ArgumentOutOfRangeException.ThrowIfZero(waitForResponseSeconds);

        var order = await PlaceOrderAsync(certificateParameters);

        _logger.LogInformation($"Order negotiated {order.Location}");

        await TriggerChallengeAsync(order, waitForResponseSeconds, cancellationToken);

        return await GetCertificateBase64StringAsync(order, certificateParameters);

        */

        return "";
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

        _logger.LogInformation($"Validating the challenge {httpChallenge.Type}");

        await httpChallenge.Validate();

        var i = 0;

        while (!cancellationToken.IsCancellationRequested && !_acmeCallback.Hit.HasValue && i++ < waitForResponseSeconds)
        {
            await Task.Delay(OneSecond, cancellationToken);

            _logger.LogInformation($"Waiting ... {i}/{waitForResponseSeconds}s");
        }
    }

    private async Task<string> GetCertificateBase64StringAsync(IOrderContext order, CertificateParameters certificateParameters)
    {
        _logger.LogInformation("Generating the certificate");

        var privateKey = KeyFactory.NewKey(KeyAlgorithm.RS256);
        var certInfo = certificateParameters.AsCsrInfo();
        var cert = await order.Generate(certInfo, privateKey);
        var pfxBuilder = cert.ToPfx(privateKey);
        var pfx = pfxBuilder.Build(certificateParameters.CertificateName, certificateParameters.Password);
        var pem = cert.ToPem(privateKey);
        var pemKey = privateKey.ToPem();
        var base64 = Convert.ToBase64String(pfx);

        await PersistPemsAsync(pem, pemKey);

        return base64;
    }

    private async Task PersistPemsAsync(string fullChain, string key)
    {
        var folder = DateTime.Now.ToString("yyyyMMddHHddss");
        var path = Path.Combine("./data", folder);

        _logger.LogInformation($"Persisting certificates: {folder}");

        try
        {
            Directory.CreateDirectory(path);
            await File.WriteAllTextAsync(Path.Combine(path, "fullchain.pem"), fullChain);
            await File.WriteAllTextAsync(Path.Combine(path, "privkey.pem"), key);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
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