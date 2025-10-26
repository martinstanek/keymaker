using Keymaker.Model;
using Microsoft.AspNetCore.Components;

namespace Keymaker.Dashboard.Components;

public partial class CertificateRequestCard
{
    [Parameter]
    public ChallengeInfo ChallengeInfo { get; set; } = ChallengeInfo.Empty;
}