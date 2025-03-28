using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Certes;
using Certes.Acme;
using Sucker.Api.Services.Callback;
using Sucker.Api.Services.Factories;
using Sucker.Api.Services.Model;

namespace Sucker.Api.Services;

public sealed class AcmeProvider : IAcmeProvider
{
    private const int OneSecond = 1000;

    private readonly IAcmeContextFactory _acmeContextFactory;
    private readonly IAcmeCallback _acmeCallback;

    public AcmeProvider(IAcmeContextFactory acmeContextFactory, IAcmeCallback acmeCallback)
    {
        _acmeContextFactory = acmeContextFactory;
        _acmeCallback = acmeCallback;
    }

    public async Task<string> GetCertificateAsync(CertificateParameters certificateParameters, uint waitForResponseSeconds, CancellationToken cancellationToken)
    {
        if (waitForResponseSeconds == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(waitForResponseSeconds));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return string.Empty;
        }

        var order = await PlaceOrderAsync(certificateParameters);

        if (cancellationToken.IsCancellationRequested)
        {
            return string.Empty;
        }

        await TriggerChallengeAsync(order, waitForResponseSeconds, cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            return string.Empty;
        }

        return await GetCertificateBase64StringAsync(order, certificateParameters);
    }

    private async Task<IOrderContext> PlaceOrderAsync(CertificateParameters certificateParameters)
    {
        var acme = _acmeContextFactory.GetAcmeContext();

        await acme.NewAccount(certificateParameters.Contact, true);

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

        await httpChallenge.Validate();

        var i = 0;

        while (!cancellationToken.IsCancellationRequested && !_acmeCallback.Hit.HasValue && i++ < waitForResponseSeconds)
        {
            await Task.Delay(OneSecond, cancellationToken);
        }
    }

    private async Task<string> GetCertificateBase64StringAsync(IOrderContext order, CertificateParameters certificateParameters)
    {
        var privateKey = KeyFactory.NewKey(KeyAlgorithm.RS256);
        var certInfo = certificateParameters.AsCsrInfo();
        var cert = await order.Generate(certInfo, privateKey);
        var pfxBuilder = cert.ToPfx(privateKey);
        var pfx = pfxBuilder.Build(certificateParameters.CertificateName, certificateParameters.Password);

        return Convert.ToBase64String(pfx);
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