using Keymaker.Model;
using Microsoft.AspNetCore.Components;

namespace Keymaker.Dashboard.Components;

public partial class CertificateRequestCard
{
    [Parameter]
    public ChallengeInfo ChallengeInfo { get; set; } = ChallengeInfo.Empty;

    public string Obtained => ChallengeInfo.Obtained?.ToShortDateString() ?? string.Empty;

    public string Expiry => ChallengeInfo.Expiry?.ToShortDateString() ?? string.Empty;

    public string NextRenewal => ChallengeInfo.NextRenewal?.ToShortDateString() ?? string.Empty;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Eventing.OnChallengeInfo += EventingOnOnChallengeInfo;
    }

    private async void EventingOnOnChallengeInfo(object? sender, ChallengeInfo e)
    {
        ChallengeInfo = e;

        await InvokeAsync(StateHasChanged);
    }
}