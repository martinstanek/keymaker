using System.Threading.Tasks;
using Certes.Acme;

namespace Keymaker.Service.Acme.Http;

public interface IHttpProvider
{
    Task<IChallengeContext> GetHttpChallenge(IAuthorizationContext authorizationContext);

    string GetHttpAuthz(IChallengeContext challengeContext);
}