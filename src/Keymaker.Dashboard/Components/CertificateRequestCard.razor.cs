using Keymaker.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Keymaker.Dashboard.Components;

public partial class CertificateRequestCard
{
    [Parameter]
    public ChallengeInfo ChallengeInfo { get; set; } = ChallengeInfo.Empty;

    private string Expiry => ChallengeInfo.Expiry?.ToString("yyyy.MM.dd HH:mm:ss") ?? "-";

    private string Obtained => ChallengeInfo.Obtained?.ToString("yyyy.MM.dd HH:mm:ss") ?? "-";

    private string NextRenewal => ChallengeInfo.NextRenewal?.ToString("yyyy.MM.dd HH:mm:ss") ?? "-";

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