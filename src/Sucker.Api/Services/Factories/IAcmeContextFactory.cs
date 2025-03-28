using Certes;

namespace Sucker.Api.Services.Factories;

public interface IAcmeContextFactory
{
    IAcmeContext GetAcmeContext();
}