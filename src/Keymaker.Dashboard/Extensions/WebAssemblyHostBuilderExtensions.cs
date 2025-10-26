using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Keymaker.Dashboard.Extensions;

public static class WebAssemblyHostBuilderExtensions
{
    public static WebAssemblyHostBuilder AddEdgeMqClient(this WebAssemblyHostBuilder builder)
    {
        return builder;
    }
}