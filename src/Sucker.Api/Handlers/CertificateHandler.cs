using System.Threading;
using System.Threading.Tasks;
using Sucker.Api.Services;
using Sucker.Api.Services.Model;

namespace Sucker.Api.Handlers;

public sealed class CertificateHandler
{
    private const byte WaitSeconds = 15;

    private readonly CertificateParameters _parameters;
    private readonly IAcmeProvider _acmeProvider;

    public CertificateHandler(CertificateParameters parameters, IAcmeProvider acmeProvider)
    {
        _parameters = parameters;
        _acmeProvider = acmeProvider;
    }

    public Task<string> GetCertificateAsync(CertificateParameters? parameters, CancellationToken cancellationToken)
    {
        var useParameters = string.IsNullOrWhiteSpace(parameters?.Contact)
            ? _parameters
            : parameters;

        return _acmeProvider.GetCertificateAsync(useParameters, WaitSeconds, cancellationToken);
    }
}