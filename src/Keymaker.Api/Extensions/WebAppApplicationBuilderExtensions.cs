using Keymaker.Api.Logging;
using Keymaker.Api.Logging.Store;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Keymaker.Api.Extensions;

public static class WebAppApplicationBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public void AddLogging()
        {
            var store = new InMemoryLoggerStore();
            var provider = new InMemoryLoggerProvider(store);

            builder.Logging.SetDefaultLevels();
            builder.Logging.AddProvider(provider);
            builder.Services.AddSingleton<IInMemoryLoggerStore>(store);
        }
    }
}