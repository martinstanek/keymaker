using System.Threading;
using System.Threading.Tasks;
using Sucker.Api.Services.Model;

namespace Sucker.Api.Services;

public interface IAcmeProvider
{
    Task<string> GetCertificateAsync(CertificateParameters certificateParameters, uint waitForResponseSeconds, CancellationToken cancellationToken);
}