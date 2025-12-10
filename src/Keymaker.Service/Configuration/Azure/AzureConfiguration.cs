using System;
using FluentValidation;

namespace Keymaker.Service.Configuration.Azure;

public sealed record AzureConfiguration
{
    public required Guid TenantId { get; init; }

    public required Guid ClientId { get; init; }

    public required string Secret { get; init; }

    public static AzureConfiguration Empty => new()
    {
        ClientId = Guid.Empty,
        TenantId = Guid.Empty,
        Secret = string.Empty
    };
}

internal sealed class AzureConfigurationValidator : AbstractValidator<AzureConfiguration>
{
    public AzureConfigurationValidator()
    {
        RuleFor(r => r.ClientId).NotEmpty();
        RuleFor(r => r.Secret).NotEmpty();
        RuleFor(r => r.TenantId).NotEmpty();
    }
}