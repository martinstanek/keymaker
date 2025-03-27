using Certes;

namespace Awitec.Framework.Acme.Factories
{
    public interface IAcmeContextFactory
    {
        IAcmeContext GetAcmeContext();
    }
}