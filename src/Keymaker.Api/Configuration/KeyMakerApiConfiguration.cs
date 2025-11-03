using Keymaker.Service.Support;

namespace Keymaker.Api.Configuration;

public sealed record KeyMakerApiConfiguration
{
    public required bool IsUiEnabled { get; init; }

    public required bool IsOpenApiDocEnabled { get; init; }

    public required bool IsLogConsoleEnabled { get; init; }

    public required bool IsApiEnabled { get; init; }

    public static KeyMakerApiConfiguration ReadFromEnvironment()
    {
        return new KeyMakerApiConfiguration
        {
            IsUiEnabled = Env.ReadBool("KEYMAKER_ENABLEUI", true),
            IsApiEnabled = Env.ReadBool("KEYMAKER_ENABLEAPI", true),
            IsLogConsoleEnabled = Env.ReadBool("KEYMAKER_ENABLECONSOLE", true),
            IsOpenApiDocEnabled = Env.ReadBool("KEYMAKER_ENABLEOPENAPI", true)
        };
    }
}