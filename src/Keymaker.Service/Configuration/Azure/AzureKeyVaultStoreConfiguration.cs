using FluentValidation;

namespace Keymaker.Service.Configuration.Azure;

public sealed record AzureKeyVaultStoreConfiguration
{
    public required string KeyVaultUrl { get; init; }

    public required string CertificateName { get; init; }
}

internal sealed class AzureKeyVaultStoreConfigurationValidator : AbstractValidator<AzureKeyVaultStoreConfiguration>
{
    public AzureKeyVaultStoreConfigurationValidator()
    {
        RuleFor(r => r.KeyVaultUrl).NotEmpty();
        RuleFor(r => r.CertificateName).NotEmpty();
    }
}