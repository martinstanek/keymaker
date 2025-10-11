using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Certes;
using Certes.Acme;
using Certes.Acme.Resource;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Keymaker.Client;
using Keymaker.Model;
using Keymaker.Service.Acme.Certificates;
using Keymaker.Service.Acme.Dns;
using Keymaker.Service.Acme.Factories;
using Keymaker.Service.Dns;
using Keymaker.Service.Store;
using Moq;
using Shouldly;
using Xunit;

namespace Keymaker.Tests.Api;

public sealed class KeymakerApiTests
{
    [Fact]
    public async Task TriggerDnsChallenge_HappyPath_CertificateObtained()
    {
        var context = new KeymakerApiTestsContext();
        var client = context.GetClient();

        await client.TriggerDnsChallengeAsync();

        await context.WaitForStatus(client, CertificateRequestStatus.Success);

        var certs = await client.GetCertificatesAsync();

        certs.ShouldNotBeEmpty();
    }

    private sealed class KeymakerApiTestsContext
    {
        internal Mock<ICertStoreService> CertStore { get; init; } = new();

        internal Mock<IDnsService> DnsService { get; init; } = new();

        internal Mock<IAcmeContextFactory> AcmeContextFactory { get; init; } = new();

        internal Mock<IAcmeContext> AcmeContext { get; init; } = new();

        internal Mock<IOrderContext> AcmeOrderContext { get; init; } = new();

        internal Mock<IAuthorizationContext> AcmeAuthContext { get; init; } = new();

        internal Mock<IKey> AcmeAccountKey { get; init; } = new();

        internal Mock<IDnsProvider> DnsProvider { get; init; } = new();

        internal Mock<IChallengeContext> AcmeChallengeContext { get; init; } = new();

        internal Mock<ICertProducer> CertProducer { get; init; } = new();

        internal async Task WaitForStatus(IKeymakerClient client, CertificateRequestStatus status, int timeSpanSeconds = 30)
        {
            var span = TimeSpan.FromSeconds(timeSpanSeconds);
            var token = new CancellationTokenSource(span).Token;

            while (!token.IsCancellationRequested)
            {
                var challengeStatus = await client.GetChallengeStatusAsync();

                if (challengeStatus.Status == status)
                {
                    return;
                }
            }
        }

        internal IKeymakerClient GetClient()
        {
            var authContext = Task.FromResult<IEnumerable<IAuthorizationContext>>([AcmeAuthContext.Object]);

            var cert = new Certificate()
            {
                PemKey = "PemKey",
                Pem = "Pem",
                Base64 = "Base64"
            };

            var certInfo = new CertificateInfo
            {
                Base64Pfx = "base64",
                Domain = "domain.com",
                Expiry = DateTime.MaxValue,
                FullChainPem = "pem",
                Obtained = DateTime.MaxValue,
                PrivateKeyPem = "pemKey"
            };

            AcmeAuthContext.Setup(s => s.Location).Returns(new Uri("https://example.com"));
            AcmeContextFactory.Setup(s => s.GetAcmeContext()).Returns(AcmeContext.Object);
            AcmeContext.Setup(s => s.NewOrder(It.IsAny<IList<string>>(), null, null)).ReturnsAsync(AcmeOrderContext.Object);
            AcmeContext.Setup(s => s.AccountKey).Returns(AcmeAccountKey.Object);
            AcmeOrderContext.Setup(s => s.Authorizations()).Returns(authContext);
            AcmeChallengeContext.Setup(s => s.Validate()).ReturnsAsync(new Challenge { Type = "dns"});
            DnsProvider.Setup(s => s.GetDnsTxtValue(It.IsAny<IChallengeContext>(), It.IsAny<IAcmeContext>())).Returns("key");
            DnsProvider.Setup(s => s.GetDnsChallengeAsync(It.IsAny<IAuthorizationContext>())).ReturnsAsync(AcmeChallengeContext.Object);
            DnsService.Setup(s => s.GetTxtEntryAsync(It.IsAny<string>())).ReturnsAsync("key");
            CertProducer.Setup(s => s.BuildCertificateAsync(It.IsAny<IOrderContext>(), It.IsAny<CertificateParameters>())).ReturnsAsync(cert);
            CertStore.Setup(s => s.GetCertificatesAsync()).ReturnsAsync([certInfo]);

            var dnsConfig = GetTestDnsConfiguration();
            var crtConfig = GetTestCertificateParams();
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddSingleton(CertStore.Object);
                        services.AddSingleton(CertProducer.Object);
                        services.AddSingleton(DnsService.Object);
                        services.AddSingleton(AcmeContextFactory.Object);
                        services.AddSingleton(DnsProvider.Object);
                        services.AddSingleton(dnsConfig);
                        services.AddSingleton(crtConfig);
                    });
                });

            var httpClient = application.CreateClient();

            return new KeymakerClient(httpClient);
        }

        internal DnsServiceConfiguration GetTestDnsConfiguration()
        {
            return new DnsServiceConfiguration
            {
                DnsChallengeCheckDomain = "dns.challenge.check.domain.com",
                DnsChallengeSetDomain = "dns.challenge.set.domain.com",
                Email = "test@example.com",
                Key = "dns.challenge.key",
                Zone = "dns.zone"
            };
        }

        internal CertificateParameters GetTestCertificateParams()
        {
            return new CertificateParameters()
            {
                CertificateName = "certificate.name",
                Contact = "test@example.com",
                CountryName = "Switzerland",
                Domain = "test.domain.com",
                Locality = "Switzerland",
                Organization = "awitec",
                OrganizationUnit = "HQ",
                Password = "secret",
                State = "Zuerich"
            };
        }
    }
}