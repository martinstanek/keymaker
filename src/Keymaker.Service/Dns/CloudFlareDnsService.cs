using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CloudFlareDns;
using CloudFlareDns.Objects.Record;
using Keymaker.Service.Configuration.CloudFlare;

namespace Keymaker.Service.Dns;

public sealed class CloudFlareDnsService : IDnsService
{
    private const int RecordTimeToLiveSeconds = 300;
    private const string RecordComment = "Added by the Keymaker.";

    private readonly CloudFlareDnsServiceConfiguration _configuration;
    private readonly IDnsLookupService _lookupService;
    private readonly ILogger<CloudFlareDnsService> _logger;
    private readonly Lazy<CloudFlareDnsClient> _dnsClient;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public CloudFlareDnsService(CloudFlareDnsServiceConfiguration configuration, IDnsLookupService lookupService, ILogger<CloudFlareDnsService> logger)
    {
        _dnsClient = new Lazy<CloudFlareDnsClient>(() => new CloudFlareDnsClient(
            xAuthKey: configuration.Key,
            xAuthEmail: configuration.Email,
            zoneIdentifier: configuration.Zone));
        _configuration = configuration;
        _lookupService = lookupService;
        _logger = logger;
    }

    public async Task AddTxtEntryAsync(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);

        await _semaphore.WaitAsync();

        _logger.LogDebug($"Setting a TXT record with {value} for the domain: {_configuration.DnsChallengeSetDomain}");

        try
        {
            await DeleteTxtEntriesAsync();
            await SetTxtEntryAsync(value);
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

    public async Task<string> GetTxtEntryAsync()
    {
        return await _lookupService.GetTxtEntryAsync(_configuration.DnsChallengeCheckDomain);
    }

    private async Task SetTxtEntryAsync(string value)
    {
        await _dnsClient.Value.Record.Create(
            name: _configuration.DnsChallengeSetDomain,
            content: value,
            proxied: false,
            RecordType.TXT,
            ttl: RecordTimeToLiveSeconds,
            RecordComment);
    }

    private async Task DeleteTxtEntriesAsync()
    {
        var records = await _dnsClient.Value.Record.Get();

        foreach (var record in records.Where(record => record.Name.Equals(_configuration.DnsChallengeCheckDomain)))
        {
            await _dnsClient.Value.Record.Delete(record.Id);
        }
    }
}