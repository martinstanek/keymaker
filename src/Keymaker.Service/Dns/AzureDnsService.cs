using System;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager.Dns;
using Azure.ResourceManager.Dns.Models;
using Keymaker.Model;

namespace Keymaker.Service.Dns;

public sealed class AzureDnsService : IDnsService
{
    private const int TtlSeconds = 300;

    private readonly Lazy<DnsTxtRecordCollection> _dnsRecords;

    public AzureDnsService(AzureDnsServiceConfiguration configuration)
    {
        _dnsRecords = new Lazy<DnsTxtRecordCollection>(ResolveDnsRecords(configuration));
    }

    public async Task AddTxtEntryAsync(string domain, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var newData = new DnsTxtRecordData
        {
            TtlInSeconds = TtlSeconds,
            DnsTxtRecords =
            {

                new DnsTxtRecordInfo { Values = { value } }
            }
        };

        await _dnsRecords.Value.CreateOrUpdateAsync(WaitUntil.Started, domain, newData);
    }

    public async Task<string> GetTxtEntryAsync(string domain)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);

        var record = await _dnsRecords.Value.GetAsync(domain);
        var value = record.HasValue
            ? record.Value.Data.DnsTxtRecords.FirstOrDefault()?.Values.FirstOrDefault() ?? string.Empty
            : string.Empty;

        return value;
    }

    private static DnsTxtRecordCollection ResolveDnsRecords(AzureDnsServiceConfiguration configuration)
    {
        var credential = new ClientSecretCredential
        (
            tenantId: configuration.TenantId.ToString(),
            clientId: configuration.ClientId.ToString(),
            clientSecret: configuration.Secret
        );

        var armClient = new Azure.ResourceManager.ArmClient(credential);
        var resource = new ResourceIdentifier(configuration.DnsZoneResourceId);
        var dnsZone = armClient.GetDnsZoneResource(resource);
        var txtRecords = dnsZone.GetDnsTxtRecords();

        return txtRecords ?? throw new InvalidOperationException();
    }
}