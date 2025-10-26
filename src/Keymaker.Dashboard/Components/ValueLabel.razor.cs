using Microsoft.AspNetCore.Components;

namespace Keymaker.Dashboard.Components;

public partial class ValueLabel
{
    [Parameter]
    public string Label { get; set; } = "Label:";

    [Parameter]
    public string Value { get; set; } = "Value";
}