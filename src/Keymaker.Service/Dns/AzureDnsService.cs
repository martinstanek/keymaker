using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager.Dns;
using Azure.ResourceManager.Dns.Models;
using Keymaker.Service.Configuration.Azure;

namespace Keymaker.Service.Dns;

public sealed class AzureDnsService : DnsService, IDnsService
{
    private const int TtlSeconds = 300;

    private readonly AzureDnsServiceConfiguration _azDnsConfig;
    private readonly AzureConfiguration _azConfig;
    private readonly Lazy<DnsTxtRecordCollection> _dnsRecords;
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private readonly ILogger<AzureDnsService> _logger;

    public AzureDnsService(
        AzureConfiguration azConfig,
        AzureDnsServiceConfiguration azDnsConfig,
        IDnsLookupService dnsLookupService,
        ILogger<AzureDnsService> logger) : base(azDnsConfig.Domain, dnsLookupService)
    {
        _azConfig = azConfig;
        _azDnsConfig = azDnsConfig;
        _logger = logger;
        _dnsRecords = new Lazy<DnsTxtRecordCollection>(ResolveDnsRecords);
    }

    public async Task AddTxtEntryAsync(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var newData = new DnsTxtRecordData
        {
            TtlInSeconds = TtlSeconds,
            DnsTxtRecords =
            {
                new DnsTxtRecordInfo { Values = { value } }
            }
        };

        await _semaphoreSlim.WaitAsync();

        _logger.LogDebug($"Setting a TXT record with {value} for the domain: {CheckDomain}");

        try
        {
            await _dnsRecords.Value.CreateOrUpdateAsync(WaitUntil.Started, SetDomain, newData);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            throw;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task<string> GetTxtEntryAsync()
    {
        return await DnsLookupService.GetTxtEntryAsync(CheckDomain);
    }

    private DnsTxtRecordCollection ResolveDnsRecords()
    {
        var credential = new ClientSecretCredential
        (
            tenantId: _azConfig.TenantId.ToString(),
            clientId: _azConfig.ClientId.ToString(),
            clientSecret: _azConfig.Secret
        );

        var armClient = new Azure.ResourceManager.ArmClient(credential);
        var resource = new ResourceIdentifier(_azDnsConfig.DnsZoneResourceId);
        var dnsZone = armClient.GetDnsZoneResource(resource);
        var txtRecords = dnsZone.GetDnsTxtRecords();

        return txtRecords ?? throw new InvalidOperationException();
    }
}