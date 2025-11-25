using System.Threading.Tasks;
using Certes.Acme;
using Keymaker.Service.Configuration.Certificate;

namespace Keymaker.Service.Acme.Certificates;

public interface ICertProducer
{
    Task<Certificate> BuildCertificateAsync(IOrderContext order, CertificateConfiguration certificateConfiguration);
}