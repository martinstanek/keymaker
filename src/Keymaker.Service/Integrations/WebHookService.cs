using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Keymaker.Service.Configuration;
using Keymaker.Service.Configuration.Service;
using Keymaker.Service.Store;
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

    public async Task TriggerWebHookAsync(CertificatePersistenceInfo persistenceInfo)
    {
        using var httpClient = new HttpClient();

        try
        {
            httpClient.BaseAddress = new Uri(_configuration.WebHookUrl);

            await httpClient.PostAsJsonAsync(string.Empty, persistenceInfo);

            _logger.LogInformation($"WebHook executed on: {_configuration.WebHookUrl}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }
}