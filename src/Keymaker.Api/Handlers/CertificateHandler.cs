using System.Threading;
using System.Threading.Tasks;
using Keymaker.Service.Acme;
using Keymaker.Service.Acme.Model;

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

    public Task<string> GetCertificateAsync(CertificateParameters? parameters, CancellationToken cancellationToken)
    {
        var useParameters = string.IsNullOrWhiteSpace(parameters?.Contact)
            ? _parameters
            : parameters;

        return _acmeService.GetCertificateAsync(useParameters, WaitSeconds, cancellationToken);
    }
}