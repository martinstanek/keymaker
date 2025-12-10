using FluentValidation;

namespace Keymaker.Service.Configuration.CloudFlare;

public sealed record CloudFlareDnsServiceConfiguration
{
    public required string Key { get; init; } = string.Empty;

    public required string Zone { get; init; } = string.Empty;

    public required string Email { get; init; } = string.Empty;

    public required string Domain { get; init; } = string.Empty;
}

internal sealed class CloudFlareDnsServiceConfigurationValidator : AbstractValidator<CloudFlareDnsServiceConfiguration>
{
    public CloudFlareDnsServiceConfigurationValidator()
    {
        RuleFor(r => r.Zone).NotEmpty();
        RuleFor(r => r.Email).NotEmpty();
        RuleFor(r => r.Key).NotEmpty();
        RuleFor(r => r.Domain).NotEmpty();
    }
}