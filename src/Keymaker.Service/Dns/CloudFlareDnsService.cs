using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CloudFlareDns;
using CloudFlareDns.Objects.Record;
using Keymaker.Service.Configuration.CloudFlare;

namespace Keymaker.Service.Dns;

public sealed class CloudFlareDnsService : DnsService, IDnsService
{
    private const string RecordComment = "Added by the Keymaker.";
    private const int RecordTimeToLiveSeconds = 300;
    
    private readonly Lazy<CloudFlareDnsClient> _dnsClient;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ILogger<CloudFlareDnsService> _logger;

    public CloudFlareDnsService(
        CloudFlareDnsServiceConfiguration configuration, 
        IDnsLookupService dnsLookupService, 
        ILogger<CloudFlareDnsService> logger) : base(configuration.Domain, dnsLookupService)
    {
        _logger = logger;
        _dnsClient = new Lazy<CloudFlareDnsClient>(() => new CloudFlareDnsClient(
            xAuthKey: configuration.Key,
            xAuthEmail: configuration.Email,
            zoneIdentifier: configuration.Zone));
    }

    public async Task AddTxtEntryAsync(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);

        await _semaphore.WaitAsync();

        _logger.LogDebug($"Setting a TXT record with {value} for the domain: {CheckDomain}");

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
        return await DnsLookupService.GetTxtEntryAsync(CheckDomain);
    }

    private async Task SetTxtEntryAsync(string value)
    {
        await _dnsClient.Value.Record.Create(
            name: SetDomain,
            content: value,
            proxied: false,
            RecordType.TXT,
            ttl: RecordTimeToLiveSeconds,
            RecordComment);
    }

    private async Task DeleteTxtEntriesAsync()
    {
        var records = await _dnsClient.Value.Record.Get();

        foreach (var record in records.Where(record => record.Name.Equals(CheckDomain)))
        {
            await _dnsClient.Value.Record.Delete(record.Id);
        }
    }
}