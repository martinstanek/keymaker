using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Certes;
using Certes.Acme;
using Awitec.Framework.Acme.Callback;
using Awitec.Framework.Acme.Factories;
using Awitec.Framework.Acme.Model;

namespace Awitec.Framework.Acme
{
    public sealed class AcmeProvider : IAcmeProvider, IDisposable
    {
        private const int OneSecond = 1000;

        private readonly IAcmeContextFactory _acmeContextFactory;
        private readonly IAcmeCallback _acmeCallback;
        private bool _isDisposed;

        public AcmeProvider(IAcmeContextFactory acmeContextFactory, IAcmeCallback acmeCallback, ILoggerFactory loggerFactory)
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

            var order = await PlaceOrderAsync(certificateParameters).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
            {
                return string.Empty;
            }

            await TriggerChallengeAsync(order, waitForResponseSeconds, cancellationToken).ConfigureAwait(false);

            if (cancellationToken.IsCancellationRequested)
            {
                return string.Empty;
            }

            return await GetCertificateBase64StringAsync(order, certificateParameters).ConfigureAwait(false);
        }

        private async Task<IOrderContext> PlaceOrderAsync(CertificateParameters certificateParameters)
        {
            var acme = _acmeContextFactory.GetAcmeContext();

            await acme.NewAccount(certificateParameters.Contact, true).ConfigureAwait(false);

            return await acme.NewOrder(new[] { certificateParameters.Domain }).ConfigureAwait(false);
        }

        private async Task TriggerChallengeAsync(IOrderContext order, uint waitForResponseSeconds, CancellationToken cancellationToken)
        {
            var authorize = (await order.Authorizations().ConfigureAwait(false)).First();
            var httpChallenge = await authorize.Http().ConfigureAwait(false);

            PrepareForChallenge(httpChallenge);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await httpChallenge.Validate().ConfigureAwait(false);

            var i = 0;

            while (!cancellationToken.IsCancellationRequested && !_acmeCallback.Hit.HasValue && i++ < waitForResponseSeconds)
            {
                await Task.Delay(OneSecond, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<string> GetCertificateBase64StringAsync(IOrderContext order, CertificateParameters certificateParameters)
        {
            var privateKey = KeyFactory.NewKey(KeyAlgorithm.RS256);
            var certInfo = certificateParameters.AsCsrInfo();
            var cert = await order.Generate(certInfo, privateKey).ConfigureAwait(false);
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

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
        }
    }
}