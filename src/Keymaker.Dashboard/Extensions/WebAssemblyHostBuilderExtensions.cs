using Keymaker.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Keymaker.Dashboard.Extensions;

public static class WebAssemblyHostBuilderExtensions
{
    public static WebAssemblyHostBuilder AddKeymakerClient(this WebAssemblyHostBuilder builder)
    {
        // var uri = new Uri(builder.HostEnvironment.BaseAddress);
        var uri = new Uri("http://localhost:5001");
        var httpClient = new HttpClient { BaseAddress = uri };
        var client = new KeymakerClient(httpClient);

        builder.Services.AddSingleton<IKeymakerClient>(_ =>  client);

        return builder;
    }
}