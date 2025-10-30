using Keymaker.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Keymaker.Dashboard.Components;

public partial class CertificateRequestCard
{
    [Parameter]
    public ChallengeInfo ChallengeInfo { get; set; } = ChallengeInfo.Empty;

    private string Obtained => ChallengeInfo.Obtained?.ToShortDateString() ?? "-";

    private string Expiry => ChallengeInfo.Expiry?.ToShortDateString() ?? "-";

    private string NextRenewal => ChallengeInfo.NextRenewal?.ToShortDateString() ?? "-";

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

    private async void OnTriggerClick(MouseEventArgs obj)
    {
        await Client.TriggerChallengeAsync();
    }
}