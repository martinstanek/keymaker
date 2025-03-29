using Certes;
using Certes.Acme;

namespace Keymaker.Service.Acme.Factories;

public class AcmeContextFactory : IAcmeContextFactory
{
    private readonly bool _isProduction;

    public AcmeContextFactory() : this(isProduction: true) { }

    public AcmeContextFactory(bool isProduction)
    {
        _isProduction = isProduction;
    }

    public IAcmeContext GetAcmeContext()
    {
        var knownServer = _isProduction
            ? WellKnownServers.LetsEncryptV2
            : WellKnownServers.LetsEncryptStagingV2;

        return new AcmeContext(knownServer);
    }
}