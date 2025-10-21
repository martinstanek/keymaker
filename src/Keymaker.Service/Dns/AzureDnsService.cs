using System;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager.Dns;
using Azure.ResourceManager.Dns.Models;
using Keymaker.Service.Configuration;

namespace Keymaker.Service.Dns;

public sealed class AzureDnsService : IDnsService
{
    private const int TtlSeconds = 300;

    private readonly AzureConfiguration _azConfig;
    private readonly AzureDnsServiceConfiguration _azDnsConfig;
    private readonly Lazy<DnsTxtRecordCollection> _dnsRecords;

    public AzureDnsService(AzureConfiguration azConfig, AzureDnsServiceConfiguration azDnsConfig)
    {
        _azConfig = azConfig;
        _azDnsConfig = azDnsConfig;
        _dnsRecords = new Lazy<DnsTxtRecordCollection>(ResolveDnsRecords());
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

        await _dnsRecords.Value.CreateOrUpdateAsync(WaitUntil.Started, _azDnsConfig.SetDomain, newData);
    }

    public async Task<string> GetTxtEntryAsync()
    {
        // TODO: use dns client we are waiting for the propagation as seen by the external systems
        var record = await _dnsRecords.Value.GetAsync(_azDnsConfig.CheckDomain);
        var value = record?.HasValue ?? false
            ? record.Value.Data.DnsTxtRecords.FirstOrDefault()?.Values.FirstOrDefault() ?? string.Empty
            : string.Empty;

        return value;
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