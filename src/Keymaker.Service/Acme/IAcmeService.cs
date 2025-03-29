using System.Threading;
using System.Threading.Tasks;
using Keymaker.Service.Acme.Model;

namespace Keymaker.Service.Acme;

public interface IAcmeService
{
    Task GetCertificateAsync(
        CertificateParameters certificateParameters,
        bool isWildCard,
        uint waitForResponseSeconds,
        CancellationToken cancellationToken);
}