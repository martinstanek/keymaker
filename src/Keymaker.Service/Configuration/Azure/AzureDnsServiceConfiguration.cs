using FluentValidation;

namespace Keymaker.Service.Configuration.Azure;

public sealed record AzureDnsServiceConfiguration
{
    public required string DnsZoneResourceId { get; init; }

    public required string Domain { get; init; }

    public static AzureDnsServiceConfiguration Empty => new()
    {
        Domain = string.Empty,
        DnsZoneResourceId = string.Empty
    };
}

internal sealed class AzureDnsServiceConfigurationValidator : AbstractValidator<AzureDnsServiceConfiguration>
{
    public AzureDnsServiceConfigurationValidator()
    {
        RuleFor(r => r.DnsZoneResourceId).NotEmpty();
        RuleFor(r => r.Domain).NotEmpty();
    }
}