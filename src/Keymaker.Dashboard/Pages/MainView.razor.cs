namespace Keymaker.Dashboard.Pages;

public partial class MainView
{
    private Timer? _timer;
    private string _serverVersion = string.Empty;
    private bool _autoRefresh = true;
    private bool _rendered;

    protected override Task OnInitializedAsync()
    {
        _timer = new Timer(
            callback: async void (_) => await OnTimerAsync(),
            state: null,
            dueTime: TimeSpan.FromSeconds(5),
            period: TimeSpan.FromSeconds(7));

        return Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_rendered)
        {
            return;
        }

        await ReloadAndSignalAsync();

        _rendered = true;
    }

    private async Task OnTimerAsync()
    {
        if (!_autoRefresh && _timer != null)
        {
            return;
        }

        await ReloadAndSignalAsync();
    }

    private async Task ReloadAndSignalAsync()
    {
        await ReloadAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task ReloadAsync()
    {
        var info = await Client.GetChallengeInfoInfoAsync();

        _serverVersion = info.Server;

        Eventing.SignalChallengeInfo(info);
    }
}