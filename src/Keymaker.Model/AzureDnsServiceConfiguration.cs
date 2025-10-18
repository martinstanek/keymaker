using System;

namespace Keymaker.Model;

public sealed record AzureDnsServiceConfiguration
{
    public required Guid TenantId { get; init; }

    public required Guid ClientId { get; init; }

    public required string DnsZoneResourceId { get; init; }

    public required string Secret { get; init; }

    public required string SetDomain { get; init; }

    public required string CheckDomain { get; init; }
}