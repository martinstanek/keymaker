using System;
using System.Threading.Tasks;

namespace Keymaker.Service.Expiration;

public interface IRenewalChecker
{
    Task<NextChallenge> ShouldTriggerChallengeAsync();

    event EventHandler<NextChallenge> OnNextChallengeCheck;
}