using Keymaker.Model;

namespace Keymaker.Dashboard.Eventing;

public sealed class EventingService : IEventingService
{
    public void SignalChallengeInfo(ChallengeInfo challengeInfo)
    {
        OnChallengeInfo.Invoke(this, challengeInfo);
    }

    public event EventHandler<ChallengeInfo> OnChallengeInfo = (_, _) => { };
}