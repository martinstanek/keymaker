using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Keymaker.Client;
using Keymaker.Model;
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

        internal async Task WaitForStatus(IKeymakerClient client, CertificateRequestStatus status, int timeSpanSeconds = 10)
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
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddSingleton(CertStore.Object);
                        services.AddSingleton(DnsService.Object);
                    });
                });

            var httpClient = application.CreateClient();

            return new KeymakerClient(httpClient);
        }
    }
}