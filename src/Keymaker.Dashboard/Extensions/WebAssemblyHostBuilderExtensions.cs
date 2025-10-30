using Keymaker.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Keymaker.Dashboard.Extensions;

public static class WebAssemblyHostBuilderExtensions
{
    public static WebAssemblyHostBuilder AddKeymakerClient(this WebAssemblyHostBuilder builder)
    {
        // var uri = new Uri(builder.HostEnvironment.BaseAddress);
        var uri = new Uri("http://10.0.1.243:6001");
        var httpClient = new HttpClient { BaseAddress = uri };
        var client = new KeymakerClient(httpClient);

        builder.Services.AddSingleton<IKeymakerClient>(_ =>  client);

        return builder;
    }
}