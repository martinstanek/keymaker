using System;
using System.Threading.Tasks;
using Keymaker.Service.Dns;
using Microsoft.AspNetCore.Http;

namespace Keymaker.Api.Handlers;

public sealed class DnsHandler
{
    private readonly IDnsService _dnsService;

    public DnsHandler(IDnsService dnsService)
    {
        _dnsService = dnsService;
    }

    public async Task<IResult> SetDnsTxtEntryAsync(string domain, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        await _dnsService.AddTxtEntryAsync(domain, value);

        return Results.NoContent();
    }

    public async Task<IResult> GetDnsTxtEntryAsync(string domain)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);

        var value = await _dnsService.GetTxtEntryAsync(domain);

        return Results.Ok(value);
    }
}