using System;
using System.Collections.Generic;
using System.Linq;

namespace Keymaker.Service.Dns;

public class DnsService
{
    private const string KnownAcmeDomain = "_acme-challenge";
    
    protected readonly IDnsLookupService DnsLookupService;
    private readonly List<string> _domainSegments;
    private readonly string _trimmedDomain;

    public DnsService(string domain, IDnsLookupService dnsLookupService)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);
        
        _trimmedDomain = domain.Trim('*').Trim('.').Trim(' ');
        _domainSegments = _trimmedDomain.Split('.').ToList();

        if (_domainSegments.Count < 2)
        {
            throw new ArgumentException("Expected at least two top level domain elements.");
        }

        DnsLookupService = dnsLookupService;
        SetDomain = GetSetDomain();
        CheckDomain = GetCheckDomain();
    }
    
    private string GetSetDomain()
    {
        var segments = new string[_domainSegments.Count - 2];
        
        _domainSegments.CopyTo(0, segments, 0, _domainSegments.Count - 2);
        
        return segments.Length > 0
            ? $"{KnownAcmeDomain}.{string.Join(".", segments)}"
            : KnownAcmeDomain;
    }

    private string GetCheckDomain()
    {
        return $"{KnownAcmeDomain}.{_trimmedDomain}";
    }

    public string SetDomain { get; }
    
    public string CheckDomain { get; }
}