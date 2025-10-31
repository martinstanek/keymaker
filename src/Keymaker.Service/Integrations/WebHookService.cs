using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Keymaker.Service.Configuration;
using Microsoft.Extensions.Logging;

namespace Keymaker.Service.Integrations;

public sealed class WebHookService : IWebHookService
{
    private readonly KeyMakerConfiguration _configuration;
    private readonly ILogger<WebHookService> _logger;

    public WebHookService(KeyMakerConfiguration configuration, ILogger<WebHookService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task TriggerWebHookAsync(string fullChain, string privateKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullChain);
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKey);

        var payload = new WebHookPayload
        {
            FullChain = fullChain,
            PrivateKey = privateKey
        };

        using var httpClient = new HttpClient();

        try
        {
            httpClient.BaseAddress = new Uri(_configuration.WebHookUrl);

            await httpClient.PostAsJsonAsync(string.Empty, payload);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }
}