using FluentValidation;

namespace Keymaker.Service.Configuration.CloudFlare;

public sealed record CloudFlareDnsServiceConfiguration
{
    public required string Key { get; init; } = string.Empty;

    public required string Zone { get; init; } = string.Empty;

    public required string Email { get; init; } = string.Empty;

    public required string DnsChallengeSetDomain { get; init; } = string.Empty;

    public required string DnsChallengeCheckDomain { get; init; } = string.Empty;

    public static CloudFlareDnsServiceConfiguration Empty => new()
    {
        Zone = string.Empty,
        Email = string.Empty,
        Key = string.Empty,
        DnsChallengeCheckDomain = string.Empty,
        DnsChallengeSetDomain = string.Empty
    };
}

internal sealed class CloudFlareDnsServiceConfigurationValidator : AbstractValidator<CloudFlareDnsServiceConfiguration>
{
    public CloudFlareDnsServiceConfigurationValidator()
    {
        RuleFor(r => r.Zone).NotEmpty();
        RuleFor(r => r.Email).NotEmpty();
        RuleFor(r => r.Key).NotEmpty();
        RuleFor(r => r.DnsChallengeCheckDomain).NotEmpty();
        RuleFor(r => r.DnsChallengeSetDomain).NotEmpty();
    }
}