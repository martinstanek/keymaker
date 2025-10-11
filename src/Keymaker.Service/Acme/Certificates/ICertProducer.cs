using System.Threading.Tasks;
using Certes.Acme;
using Keymaker.Model;

namespace Keymaker.Service.Acme.Certificates;

public interface ICertProducer
{
    Task<Certificate> BuildCertificateAsync(IOrderContext order, CertificateParameters certificateParameters);
}