using System.Threading;
using System.Threading.Tasks;
using Awitec.Framework.Acme.Model;

namespace Awitec.Framework.Acme
{
    public interface IAcmeProvider
    {
        Task<string> GetCertificateAsync(CertificateParameters certificateParameters, uint waitForResponseSeconds, CancellationToken cancellationToken);
    }
}