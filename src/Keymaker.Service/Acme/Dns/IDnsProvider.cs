using System.Threading.Tasks;
using Certes;
using Certes.Acme;

namespace Keymaker.Service.Acme.Dns;

public interface IDnsProvider
{
    Task<IChallengeContext> GetDnsChallengeAsync(IAuthorizationContext authorizationContext);

    string GetDnsTxtValue(IChallengeContext dnsChallenge, IAcmeContext acme);
}