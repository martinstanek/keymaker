namespace Keymaker.Service.Configuration;

public sealed record AzureKeyVaultStoreConfiguration
{
    public required string KeyVaultUrl { get; init; }

    public required string CertificateName { get; init; }
}