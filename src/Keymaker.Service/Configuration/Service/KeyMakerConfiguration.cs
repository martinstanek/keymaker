using FluentValidation;
using Keymaker.Service.Model;

namespace Keymaker.Service.Configuration.Service;

public sealed record KeyMakerConfiguration
{
    public required DnsMode DnsMode { get; init; }

    public required StorageMode StorageMode { get; init; }

    public required ChallengeMode ChallengeMode { get; init; }

    public required bool IsAutoRenewalEnabled { get; init; }

    public required bool IsWebHookEnabled { get; init; }

    public required bool IsChallengeTriggerEnabled { get; init; }

    public required string WebHookUrl { get; init; }

    public required int RenewEveryHours { get; init; }

    public required int CheckForExpirationEveryMinutes { get; init; }

    public static KeyMakerConfiguration Empty => new()
    {
        DnsMode = DnsMode.CloudFlare,
        StorageMode = StorageMode.Volume,
        ChallengeMode = ChallengeMode.Http,
        IsAutoRenewalEnabled = false,
        IsWebHookEnabled = false,
        IsChallengeTriggerEnabled = false,
        WebHookUrl = string.Empty,
        RenewEveryHours = 0,
        CheckForExpirationEveryMinutes = 0
    };
}

internal sealed class KeyMakerConfigurationValidator : AbstractValidator<KeyMakerConfiguration>
{
    public KeyMakerConfigurationValidator()
    {
        RuleFor(r => r.WebHookUrl).NotEmpty().When(w => w.IsWebHookEnabled);
        RuleFor(r => r.RenewEveryHours).GreaterThan(0).When(w => w.IsAutoRenewalEnabled);
        RuleFor(r => r.CheckForExpirationEveryMinutes).GreaterThan(0).When(w => w.IsAutoRenewalEnabled);
    }
}