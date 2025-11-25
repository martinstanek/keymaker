using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Keymaker.Client;
using Keymaker.Service.Dns;
using Keymaker.Service.Model;
using Keymaker.Service.Store;
using Keymaker.Service.Acme.Dns;
using Keymaker.Service.Acme.Certificates;
using Keymaker.Service.Acme.Factories;
using Keymaker.Service.Acme.Http;
using Keymaker.Service.Configuration.Certificate;
using Keymaker.Service.Configuration.CloudFlare;
using Keymaker.Service.Configuration.Service;
using Certes;
using Certes.Acme;
using Certes.Acme.Resource;
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
        var client = context.GetClient(challengeMode: ChallengeMode.Dns);

        await client.TriggerChallengeAsync();
        await KeymakerApiTestsContext.WaitForStatus(client, CertificateRequestStatus.Success);

        var challengeInfo = await client.GetChallengeInfoInfoAsync();

        challengeInfo.Domain.ShouldBe("example.com");
    }

    [Fact]
    public async Task TriggerHttpChallenge_HappyPath_CertificateObtained()
    {
        var context = new KeymakerApiTestsContext();
        var client = context.GetClient(challengeMode: ChallengeMode.Http);

        await client.TriggerChallengeAsync();
        await KeymakerApiTestsContext.WaitForStatus(client, CertificateRequestStatus.WaitingForHttpVerification);
        await client.ConfirmHttpChallengeAsync("test");
        await KeymakerApiTestsContext.WaitForStatus(client, CertificateRequestStatus.Success);

        var challengeInfoAsync = await client.GetChallengeInfoInfoAsync();

        challengeInfoAsync.Domain.ShouldBe("example.com");
    }

    private sealed class KeymakerApiTestsContext
    {
        internal IKeymakerClient GetClient(ChallengeMode challengeMode)
        {
            var authContext = Task.FromResult<IEnumerable<IAuthorizationContext>>([AcmeAuthContext.Object]);

            var cert = new Certificate
            {
                PemKey = "PemKey",
                Pem = "Pem",
                Base64 = "Base64",
                Issuer = "issuer",
                Domain = "example.com",
                Expiry = DateTime.MinValue
            };

            var certInfo = new CertificateInfo
            {
                Domain = "example.com",
                Expiry = DateTime.MaxValue,
                Obtained = DateTime.MinValue,
                Issuer = "Let's Encrypt"
            };

            var keyMakerConf = new KeyMakerConfiguration
            {
                ChallengeMode = challengeMode,
                DnsMode = DnsMode.CloudFlare,
                StorageMode = StorageMode.Volume,
                IsAutoRenewalEnabled = false,
                IsWebHookEnabled = false,
                IsChallengeTriggerEnabled = true,
                WebHookUrl = string.Empty,
                RenewEveryHours = 0,
                CheckForExpirationEveryMinutes = 0
            };

            AcmeAuthContext.Setup(s => s.Location).Returns(new Uri("https://example.com"));
            AcmeContextFactory.Setup(s => s.GetAcmeContext()).Returns(AcmeContext.Object);
            AcmeContext.Setup(s => s.NewOrder(It.IsAny<IList<string>>(), null, null)).ReturnsAsync(AcmeOrderContext.Object);
            AcmeContext.Setup(s => s.AccountKey).Returns(AcmeAccountKey.Object);
            AcmeOrderContext.Setup(s => s.Authorizations()).Returns(authContext);
            AcmeChallengeContext.Setup(s => s.Validate()).ReturnsAsync(new Challenge { Type = "dns"});
            AcmeChallengeContext.Setup(s => s.Location).Returns(new Uri("https://example.com"));
            DnsProvider.Setup(s => s.GetDnsTxtValue(It.IsAny<IChallengeContext>(), It.IsAny<IAcmeContext>())).Returns("key");
            DnsProvider.Setup(s => s.GetDnsChallengeAsync(It.IsAny<IAuthorizationContext>())).ReturnsAsync(AcmeChallengeContext.Object);
            DnsService.Setup(s => s.GetTxtEntryAsync()).ReturnsAsync("key");
            CertProducer.Setup(s => s.BuildCertificateAsync(It.IsAny<IOrderContext>(), It.IsAny<CertificateConfiguration>())).ReturnsAsync(cert);
            CertStore.Setup(s => s.GetMostRecentCertificateInfoAsync()).ReturnsAsync(certInfo);
            HttpProvider.Setup(s => s.GetHttpChallenge(It.IsAny<IAuthorizationContext>())).ReturnsAsync(AcmeChallengeContext.Object);
            HttpProvider.Setup(s => s.GetHttpAuthz(It.IsAny<IChallengeContext>())).Returns("token.key");

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
                        services.AddSingleton(HttpProvider.Object);
                        services.AddSingleton(dnsConfig);
                        services.AddSingleton(crtConfig);
                        services.AddSingleton(keyMakerConf);
                    });
                });

            var httpClient = application.CreateClient();

            return new KeymakerClient(httpClient);
        }

        internal static async Task WaitForStatus(IKeymakerClient client, CertificateRequestStatus status, int timeSpanSeconds = 30)
        {
            var span = TimeSpan.FromSeconds(timeSpanSeconds);
            var token = new CancellationTokenSource(span).Token;

            while (!token.IsCancellationRequested)
            {
                var challengeStatus = await client.GetChallengeInfoInfoAsync();

                if (challengeStatus.Status == status.ToString())
                {
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(2), token);
            }
        }

        private static CloudFlareDnsServiceConfiguration GetTestDnsConfiguration()
        {
            return new CloudFlareDnsServiceConfiguration
            {
                DnsChallengeCheckDomain = "dns.challenge.check.domain.com",
                DnsChallengeSetDomain = "dns.challenge.set.domain.com",
                Email = "test@example.com",
                Key = "dns.challenge.key",
                Zone = "dns.zone"
            };
        }

        private static CertificateConfiguration GetTestCertificateParams()
        {
            return new CertificateConfiguration
            {
                CertificateName = "certificate.name",
                Contact = "test@example.com",
                CountryName = "Switzerland",
                Domain = "test.domain.com",
                Locality = "Switzerland",
                Organization = "awitec",
                OrganizationUnit = "HQ",
                Password = "secret",
                State = "Zürich"
            };
        }

        private Mock<IAcmeContextFactory> AcmeContextFactory { get; init; } = new();

        private Mock<IChallengeContext> AcmeChallengeContext { get; init; } = new();

        private Mock<IAuthorizationContext> AcmeAuthContext { get; init; } = new();

        private Mock<IOrderContext> AcmeOrderContext { get; init; } = new();

        private Mock<ICertStoreService> CertStore { get; init; } = new(); // TODO: try to use the volume store

        private Mock<IHttpProvider> HttpProvider { get; init; } = new();

        private Mock<ICertProducer> CertProducer { get; init; } = new();

        private Mock<IAcmeContext> AcmeContext { get; init; } = new();

        private Mock<IDnsProvider> DnsProvider { get; init; } = new();

        private Mock<IDnsService> DnsService { get; init; } = new();

        private Mock<IKey> AcmeAccountKey { get; init; } = new();
    }
}