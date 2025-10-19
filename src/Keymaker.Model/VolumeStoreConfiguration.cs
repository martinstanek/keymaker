namespace Keymaker.Model;

public sealed record VolumeStoreConfiguration
{
    public required string ToplevelFolder { get; init; }
}