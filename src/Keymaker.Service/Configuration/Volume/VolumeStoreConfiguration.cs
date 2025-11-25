using FluentValidation;

namespace Keymaker.Service.Configuration.Volume;

public sealed record VolumeStoreConfiguration
{
    public required string ToplevelFolder { get; init; }
}

internal sealed class VolumeStoreConfigurationValidator : AbstractValidator<VolumeStoreConfiguration>
{
    public VolumeStoreConfigurationValidator()
    {
        RuleFor(r => r.ToplevelFolder).NotEmpty();
    }
}