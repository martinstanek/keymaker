using System.Threading.Tasks;
using Certes;
using Certes.Acme;

namespace Keymaker.Service.Acme.Dns;

public sealed class DnsProvider : IDnsProvider
{
    public Task<IChallengeContext> GetDnsChallengeAsync(IAuthorizationContext authorizationContext)
    {
        return authorizationContext.Dns();
    }

    public string GetDnsTxtValue(IChallengeContext dnsChallenge, IAcmeContext acme)
    {
        return acme.AccountKey.DnsTxt(dnsChallenge.Token);
    }
}