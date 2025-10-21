namespace Keymaker.Service.Configuration;

public sealed record VolumeStoreConfiguration
{
    public required string ToplevelFolder { get; init; }
}