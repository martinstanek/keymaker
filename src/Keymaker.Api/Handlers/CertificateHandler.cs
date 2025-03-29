using System.Threading;
using System.Threading.Tasks;
using Keymaker.Service.Acme;
using Keymaker.Service.Acme.Model;
using Microsoft.AspNetCore.Http;

namespace Keymaker.Api.Handlers;

public sealed class CertificateHandler
{
    private const byte WaitSeconds = 60;

    private readonly CertificateParameters _parameters;
    private readonly IAcmeService _acmeService;

    public CertificateHandler(CertificateParameters parameters, IAcmeService acmeService)
    {
        _parameters = parameters;
        _acmeService = acmeService;
    }

    public async Task<IResult> GetCertificateAsync(CertificateParameters? parameters, CancellationToken cancellationToken)
    {
        var useParameters = string.IsNullOrWhiteSpace(parameters?.Contact)
            ? _parameters
            : parameters;

        await _acmeService.GetCertificateAsync(useParameters, isWildCard: true, WaitSeconds, cancellationToken);

        return Results.NoContent();
    }
}