using Keymaker.Model;

namespace Keymaker.Dashboard.Eventing;

public interface IEventingService
{
    void SignalChallengeInfo(ChallengeInfo challengeInfo);

    event EventHandler<ChallengeInfo> OnChallengeInfo;
}