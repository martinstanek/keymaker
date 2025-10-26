using Keymaker.Model;
using Microsoft.AspNetCore.Components;

namespace Keymaker.Dashboard.Components;

public partial class CertificateRequestCard
{
    [Parameter]
    public ChallengeInfo ChallengeInfo { get; set; } = ChallengeInfo.Empty;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Eventing.OnChallengeInfo += (_, info) => { ChallengeInfo = info; };
    }
}