using FluentValidation;

namespace Keymaker.Service.Configuration.Azure;

public sealed record AzureDnsServiceConfiguration
{
    public required string DnsZoneResourceId { get; init; }

    public required string SetDomain { get; init; }

    public required string CheckDomain { get; init; }
}

internal sealed class AzureDnsServiceConfigurationValidator : AbstractValidator<AzureDnsServiceConfiguration>
{
    public AzureDnsServiceConfigurationValidator()
    {
        RuleFor(r => r.DnsZoneResourceId).NotEmpty();
        RuleFor(r => r.SetDomain).NotEmpty();
        RuleFor(r => r.CheckDomain).NotEmpty();
    }
}