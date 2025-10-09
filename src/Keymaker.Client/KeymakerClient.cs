using System.Threading.Tasks;
using System.Collections.Immutable;
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

    public Task<ImmutableArray<CertificateInfo>> GetCertificatesAsync()
    {
        return _httpClient.GetFromJsonAsync<ImmutableArray<CertificateInfo>>("certificates");
    }

    public Task CancelCurrentChallengeAsync()
    {
        return _httpClient.DeleteAsync("/challenge");
    }

    public Task TriggerDnsChallengeAsync()
    {
        return _httpClient.PutAsync("/challenge/dns", new StringContent(string.Empty));
    }

    public Task TriggerHttpChallengeAsync()
    {
        return _httpClient.PutAsync("/challenge/http", new StringContent(string.Empty));
    }
}