using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DnsClient;

namespace Keymaker.Service.Dns;

public class DnsLookupService : IDnsLookupService
{
    private readonly ILogger<DnsLookupService> _logger;
    private readonly Lazy<LookupClient> _lookupClient;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public DnsLookupService(ILogger<DnsLookupService> logger)
    {
        _logger = logger;
        _lookupClient = new Lazy<LookupClient>(() => new LookupClient());
    }

    public async Task<string> GetTxtEntryAsync(string checkDomain)
    {
        await _semaphore.WaitAsync();

        try
        {
            var result = await _lookupClient.Value.QueryAsync(checkDomain, QueryType.TXT);

            return result.Answers
                .TxtRecords()
                .FirstOrDefault()?.Text
                .FirstOrDefault() ?? string.Empty;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}