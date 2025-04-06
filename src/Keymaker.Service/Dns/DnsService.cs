using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CloudFlareDns;
using CloudFlareDns.Objects.Record;
using DnsClient;

namespace Keymaker.Service.Dns;

public sealed class DnsService : IDnsService
{
    private const int RecordTimeToLiveSeconds = 15 * 60;
    private const string RecordComment = "Added by the Keymaker.";
    private const string RecordPrefix = "_acme-challenge";

    private readonly ILogger<DnsService> _logger;
    private readonly Lazy<CloudFlareDnsClient> _dnsClient;
    private readonly Lazy<LookupClient> _lookupClient;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public DnsService(DnsServiceConfiguration configuration, ILogger<DnsService> logger)
    {
        _lookupClient = new Lazy<LookupClient>(() => new LookupClient());
        _dnsClient = new Lazy<CloudFlareDnsClient>(() => new CloudFlareDnsClient(
            xAuthKey: configuration.Key,
            xAuthEmail: configuration.Email,
            zoneIdentifier: configuration.Zone));
        _logger = logger;
    }

    public async Task AddTxtEntryAsync(string domain, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(domain);
        ArgumentException.ThrowIfNullOrEmpty(value);

        await _semaphore.WaitAsync();

        _logger.LogDebug($"Setting a TXT record with {value} for the domain: {domain}");

        try
        {
            await _dnsClient.Value.Record.Create(
                name: domain,
                content: value,
                proxied: false,
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

    public async Task<string> GetTxtEntryAsync(string domain)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);

        await _semaphore.WaitAsync();

        try
        {
            var result = await _lookupClient.Value.QueryAsync(domain, QueryType.TXT);

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