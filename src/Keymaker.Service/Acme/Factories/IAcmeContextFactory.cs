using Certes;

namespace Keymaker.Api.Services.Factories;

public interface IAcmeContextFactory
{
    IAcmeContext GetAcmeContext();
}