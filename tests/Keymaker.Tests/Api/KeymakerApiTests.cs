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
using Keymaker.Service.Configuration;
using Keymaker.Service.Configuration.Volume;
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
        using var context = new KeymakerApiTestsContext();
        var client = context.GetClient(challengeMode: ChallengeMode.Dns);

        await client.TriggerChallengeAsync();
        await KeymakerApiTestsContext.WaitForStatus(client, CertificateRequestStatus.Success);

        var challengeInfo = await client.GetChallengeInfoInfoAsync();

        challengeInfo.Domain.ShouldBe("example.com");
    }

    [Fact]
    public async Task TriggerHttpChallenge_HappyPath_CertificateObtained()
    {
        using var context = new KeymakerApiTestsContext();
        var client = context.GetClient(challengeMode: ChallengeMode.Http);

        await client.TriggerChallengeAsync();
        await KeymakerApiTestsContext.WaitForStatus(client, CertificateRequestStatus.WaitingForHttpVerification);
        await client.ConfirmHttpChallengeAsync("test");
        await KeymakerApiTestsContext.WaitForStatus(client, CertificateRequestStatus.Success);

        var challengeInfoAsync = await client.GetChallengeInfoInfoAsync();

        challengeInfoAsync.Domain.ShouldBe("example.com");
    }

    private sealed class KeymakerApiTestsContext : IDisposable
    {
        public IKeymakerClient GetClient(ChallengeMode challengeMode)
        {
            var acmeContextFactory = new Mock<IAcmeContextFactory>();
            var acmeChallengeContext = new Mock<IChallengeContext>();
            var acmeAuthContext = new Mock<IAuthorizationContext>();
            var acmeOrderContext = new Mock<IOrderContext>();
            var acmeContext = new Mock<IAcmeContext>();
            var acmeAccountKey = new Mock<IKey>();
            var certStore = new Mock<ICertStoreService>();
            var httpProvider = new Mock<IHttpProvider>();
            var certProducer = new Mock<ICertProducer>();
            var dnsProvider = new Mock<IDnsProvider>();
            var dnsService = new Mock<IDnsService>();
            var authContext = Task.FromResult<IEnumerable<IAuthorizationContext>>([acmeAuthContext.Object]);
            var keyMakerConfig = GetKeyMakerConfiguration(challengeMode);
            var storeConfig = GetTestVolumeStoreConfiguration();
            var dnsConfig = GetTestDnsConfiguration();
            var crtConfig = GetTestCertificateConfiguration();
            var certInfo = GetCertificateInfo();
            var cert = GetCertificate();

            EnvironmentConfiguration.WriteConfiguration(EnvironmentConfiguration.ConstructCertificateConfiguration(crtConfig));
            EnvironmentConfiguration.WriteConfiguration(EnvironmentConfiguration.ConstructCloudFlareDnsServiceConfiguration(dnsConfig));
            EnvironmentConfiguration.WriteConfiguration(EnvironmentConfiguration.ConstructVolumeStoreConfiguration(storeConfig));
            EnvironmentConfiguration.WriteConfiguration(EnvironmentConfiguration.ConstructKeyMakerConfiguration(keyMakerConfig));

            acmeAuthContext.Setup(s => s.Location).Returns(new Uri("https://example.com"));
            acmeContextFactory.Setup(s => s.GetAcmeContext()).Returns(acmeContext.Object);
            acmeContext.Setup(s => s.NewOrder(It.IsAny<IList<string>>(), null, null)).ReturnsAsync(acmeOrderContext.Object);
            acmeContext.Setup(s => s.AccountKey).Returns(acmeAccountKey.Object);
            acmeOrderContext.Setup(s => s.Authorizations()).Returns(authContext);
            acmeChallengeContext.Setup(s => s.Validate()).ReturnsAsync(new Challenge { Type = "dns"});
            acmeChallengeContext.Setup(s => s.Location).Returns(new Uri("https://example.com"));
            dnsProvider.Setup(s => s.GetDnsTxtValue(It.IsAny<IChallengeContext>(), It.IsAny<IAcmeContext>())).Returns("key");
            dnsProvider.Setup(s => s.GetDnsChallengeAsync(It.IsAny<IAuthorizationContext>())).ReturnsAsync(acmeChallengeContext.Object);
            dnsService.Setup(s => s.GetTxtEntryAsync()).ReturnsAsync("key");
            certProducer.Setup(s => s.BuildCertificateAsync(It.IsAny<IOrderContext>(), It.IsAny<CertificateConfiguration>())).ReturnsAsync(cert);
            certStore.Setup(s => s.GetMostRecentCertificateInfoAsync()).ReturnsAsync(certInfo);
            httpProvider.Setup(s => s.GetHttpChallenge(It.IsAny<IAuthorizationContext>())).ReturnsAsync(acmeChallengeContext.Object);
            httpProvider.Setup(s => s.GetHttpAuthz(It.IsAny<IChallengeContext>())).Returns("token.key");

            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddSingleton(certStore.Object);
                        services.AddSingleton(certProducer.Object);
                        services.AddSingleton(dnsService.Object);
                        services.AddSingleton(acmeContextFactory.Object);
                        services.AddSingleton(dnsProvider.Object);
                        services.AddSingleton(httpProvider.Object);
                        services.AddSingleton(dnsConfig);
                        services.AddSingleton(crtConfig);
                        services.AddSingleton(keyMakerConfig);
                    });
                });

            var httpClient = application.CreateClient();

            return new KeymakerClient(httpClient);
        }

        public static async Task WaitForStatus(IKeymakerClient client, CertificateRequestStatus status, int timeSpanSeconds = 60)
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

        public void Dispose()
        {
            var keyMakerConfig = GetKeyMakerConfiguration(ChallengeMode.Http);
            var storeConfig = GetTestVolumeStoreConfiguration();
            var dnsConfig = GetTestDnsConfiguration();
            var crtConfig = GetTestCertificateConfiguration();

            EnvironmentConfiguration.RemoveConfiguration(EnvironmentConfiguration.ConstructCertificateConfiguration(crtConfig));
            EnvironmentConfiguration.RemoveConfiguration(EnvironmentConfiguration.ConstructCloudFlareDnsServiceConfiguration(dnsConfig));
            EnvironmentConfiguration.RemoveConfiguration(EnvironmentConfiguration.ConstructVolumeStoreConfiguration(storeConfig));
            EnvironmentConfiguration.RemoveConfiguration(EnvironmentConfiguration.ConstructKeyMakerConfiguration(keyMakerConfig));
        }

        private static CloudFlareDnsServiceConfiguration GetTestDnsConfiguration()
        {
            return new CloudFlareDnsServiceConfiguration
            {
                Domain = "example.com",
                Email = "test@example.com",
                Key = "dns.challenge.key",
                Zone = "dns.zone"
            };
        }

        private static CertificateConfiguration GetTestCertificateConfiguration()
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

        private static VolumeStoreConfiguration GetTestVolumeStoreConfiguration()
        {
            return new VolumeStoreConfiguration
            {
                ToplevelFolder = "./test"
            };
        }

        private static KeyMakerConfiguration GetKeyMakerConfiguration(ChallengeMode challengeMode)
        {
            return new KeyMakerConfiguration
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
        }

        private static CertificateInfo GetCertificateInfo()
        {
            return new CertificateInfo
            {
                Domain = "example.com",
                Expiry = DateTime.MaxValue,
                Obtained = DateTime.MinValue,
                Issuer = "Let's Encrypt"
            };
        }

        private static Certificate GetCertificate()
        {
            return new Certificate
            {
                PemKey = "PemKey",
                Pem = "Pem",
                Base64 = "Base64",
                Issuer = "issuer",
                Domain = "example.com",
                Expiry = DateTime.MinValue
            };
        }
    }
}