using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using Keymaker.Model;

namespace Keymaker.Client;

public sealed class KeymakerClient : IKeymakerClient
{
    private readonly HttpClient _httpClient;

    public KeymakerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CertificateInfo> GetMostRecentCertificateInfoAsync()
    {
        var certificateInfo = await _httpClient.GetFromJsonAsync<CertificateInfo>("/certificate");

        return certificateInfo ?? CertificateInfo.Empty;
    }

    public async Task<ChallengeInfo> GetChallengeInfoInfoAsync()
    {
        var status = await _httpClient.GetFromJsonAsync<ChallengeInfo>("/challenge/info");

        return status ?? ChallengeInfo.Empty;
    }

    public async Task<ChallengeStatus> GetChallengeStatusAsync()
    {
        var status = await _httpClient.GetFromJsonAsync<ChallengeStatus>("/challenge/status");

        return status ?? ChallengeStatus.Empty;
    }

    public Task CancelCurrentChallengeAsync()
    {
        return _httpClient.DeleteAsync("/challenge");
    }

    public Task TriggerChallengeAsync()
    {
        return _httpClient.PutAsync("/challenge", new StringContent(string.Empty));
    }

    public Task<string> ConfirmHttpChallengeAsync(string challenge)
    {
        return _httpClient.GetStringAsync($"/.well-known/acme-challenge/{challenge}");
    }
}