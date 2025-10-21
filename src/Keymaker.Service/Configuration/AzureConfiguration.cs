using System;

namespace Keymaker.Service.Configuration;

public sealed record AzureConfiguration
{
    public required Guid TenantId { get; init; }

    public required Guid ClientId { get; init; }

    public required string Secret { get; init; }
}