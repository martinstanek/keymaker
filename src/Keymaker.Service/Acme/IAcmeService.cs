using System.Threading;
using System.Threading.Tasks;
using Keymaker.Service.Acme.Model;

namespace Keymaker.Service.Acme;

public interface IAcmeService
{
    Task<string> GetCertificateAsync(CertificateParameters certificateParameters, uint waitForResponseSeconds, CancellationToken cancellationToken);
}