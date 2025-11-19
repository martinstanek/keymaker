namespace Keymaker.Service.Configuration;

public sealed record AzureDnsServiceConfiguration
{
    public required string DnsZoneResourceId { get; init; }

    public required string SetDomain { get; init; }

    public required string CheckDomain { get; init; }
}