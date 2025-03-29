using Certes;

namespace Keymaker.Service.Acme.Factories;

public interface IAcmeContextFactory
{
    IAcmeContext GetAcmeContext();
}