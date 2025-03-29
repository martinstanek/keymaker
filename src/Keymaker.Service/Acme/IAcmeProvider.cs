using System.Threading;
using System.Threading.Tasks;
using Keymaker.Api.Services.Model;

namespace Keymaker.Api.Services;

public interface IAcmeProvider
{
    Task<string> GetCertificateAsync(CertificateParameters certificateParameters, uint waitForResponseSeconds, CancellationToken cancellationToken);
}