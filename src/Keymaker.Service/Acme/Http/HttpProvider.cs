using System.Threading.Tasks;
using Certes;
using Certes.Acme;

namespace Keymaker.Service.Acme.Http;

public sealed class HttpProvider : IHttpProvider
{
    public Task<IChallengeContext> GetHttpChallenge(IAuthorizationContext authorizationContext)
    {
        return authorizationContext.Http();
    }

    public string GetHttpAuthz(IChallengeContext challengeContext)
    {
        return challengeContext.KeyAuthz;
    }
}