using System.Threading;
using System.Threading.Tasks;
using Keymaker.Service.Acme;
using Keymaker.Service.Acme.Model;
using Microsoft.AspNetCore.Http;

namespace Keymaker.Api.Handlers;

public sealed class CertificateHandler
{
    private readonly CertificateParameters _parameters;
    private readonly IAcmeService _acmeService;

    public CertificateHandler(CertificateParameters parameters, IAcmeService acmeService)
    {
        _parameters = parameters;
        _acmeService = acmeService;
    }

    public Task<IResult> TriggerDnsChallengeAsync(CertificateParameters? parameters)
    {
        var mergedParameters = parameters.MergeWithDefaults(_parameters);

        Task.Factory.StartNew(
            () => _acmeService.RequestCertificateViaDnsChallengeAsync(mergedParameters, CancellationToken.None),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

        return Task.FromResult(Results.NoContent());
    }

    public Task<IResult> TriggerHttpChallenge(CertificateParameters? parameters)
    {
        var mergedParameters = parameters.MergeWithDefaults(_parameters);

        Task.Factory.StartNew(
            () => _acmeService.RequestCertificateViaHttpChallengeAsync(mergedParameters, CancellationToken.None),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

        return Task.FromResult(Results.NoContent());
    }
}