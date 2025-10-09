using System.Threading.Tasks;
using Keymaker.Client;
using Keymaker.Service.Acme;
using Keymaker.Service.Dns;
using Keymaker.Service.Store;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
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

        var certs = await client.GetCertificatesAsync();

        certs.ShouldNotBeEmpty();
    }

    private sealed class KeymakerApiTestsContext
    {
        internal Mock<ICertStoreService> CertStore { get; init; } = new();

        internal Mock<IAcmeService> AcmeService { get; init; } = new();

        internal Mock<IDnsService> DnsService { get; init; } = new();

        internal IKeymakerClient GetClient()
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddSingleton(CertStore.Object);
                        services.AddSingleton(AcmeService.Object);
                        services.AddSingleton(DnsService.Object);
                    });
                });

            var httpClient = application.CreateClient();

            return new KeymakerClient(httpClient);
        }
    }
}