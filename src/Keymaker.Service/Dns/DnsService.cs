using System;
using System.Threading.Tasks;

namespace Keymaker.Service.Dns;

public sealed class DnsService : IDnsService
{
    public Task AddTxtEntryAsync(string domain, string prefix, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(domain);
        ArgumentException.ThrowIfNullOrEmpty(prefix);
        ArgumentException.ThrowIfNullOrEmpty(value);

        return Task.CompletedTask;
    }

    public Task RemoveTxtEntryAsync(string domain, string prefix)
    {
        ArgumentException.ThrowIfNullOrEmpty(domain);
        ArgumentException.ThrowIfNullOrEmpty(prefix);

        return Task.CompletedTask;
    }
}