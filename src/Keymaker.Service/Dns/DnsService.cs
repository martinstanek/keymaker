using System;
using System.Threading;
using System.Threading.Tasks;
using CloudFlareDns;
using CloudFlareDns.Objects.Record;
using Microsoft.Extensions.Logging;

namespace Keymaker.Service.Dns;

public sealed class DnsService : IDnsService
{
    private const int RecordTimeToLiveSeconds = 3600;
    private const string RecordComment = "Added by the Keymaker.";
    private const string RecordPrefix = "_acme-challenge";

    private readonly ILogger<DnsService> _logger;
    private readonly Lazy<CloudFlareDnsClient> _client;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public DnsService(DnsServiceConfiguration configuration, ILogger<DnsService> logger)
    {
        _client = new Lazy<CloudFlareDnsClient>(() => new CloudFlareDnsClient(
            xAuthKey: configuration.Key,
            xAuthEmail: configuration.Email,
            zoneIdentifier: configuration.Zone));
        _logger = logger;
    }

    public async Task AddTxtEntryAsync(string domain, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(domain);
        ArgumentException.ThrowIfNullOrEmpty(value);

        var recordName = $"{RecordPrefix}.{domain}";

        await _semaphore.WaitAsync();

        _logger.LogDebug($"Setting a TXT record with {value} for the domain: {domain}");

        try
        {
            await _client.Value.Record.Create(
                name: recordName,
                content: value,
                proxied: true,
                RecordType.TXT,
                ttl: RecordTimeToLiveSeconds,
                RecordComment);
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

    public async Task RemoveTxtEntryAsync(string domain)
    {
        ArgumentException.ThrowIfNullOrEmpty(domain);

        var recordName = $"{RecordPrefix}.{domain}";

        await _semaphore.WaitAsync();

        _logger.LogDebug($"Removing a TXT record for the domain: {domain}");

        try
        {
            await _client.Value.Record.Delete(recordName);
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