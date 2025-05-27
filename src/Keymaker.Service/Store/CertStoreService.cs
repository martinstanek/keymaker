using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Keymaker.Model;

namespace Keymaker.Service.Store;

public sealed class CertStoreService : ICertStoreService
{
    private const string DefaultFolderTimeFormat = "yyyyMMddHHddss";
    private const string TopLevelFolderName = "/data";
    private const string PrivateKeyFileName = "privkey.pem";
    private const string FullChainFileName = "fullchain.pem";
    private const string PfxFileName = "base64.pfx.txt";

    private readonly ILogger<CertStoreService> _logger;

    public CertStoreService(ILogger<CertStoreService> logger)
    {
        _logger = logger;
    }

    public async Task PersistCertificatesAsync(string domain, string fullChainPem, string privateKeyPem, string base64Pfx)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullChainPem);
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKeyPem);
        ArgumentException.ThrowIfNullOrWhiteSpace(base64Pfx);

        var folder = DateTime.Now.ToString(DefaultFolderTimeFormat);
        var path = Path.Combine(TopLevelFolderName, domain, folder);

        _logger.LogDebug($"Persisting certificates into: {path}");

        try
        {
            Directory.CreateDirectory(path);

            await File.WriteAllTextAsync(Path.Combine(path, FullChainFileName), fullChainPem);
            await File.WriteAllTextAsync(Path.Combine(path, PrivateKeyFileName), privateKeyPem);
            await File.WriteAllTextAsync(Path.Combine(path, PfxFileName), base64Pfx);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }

    public async Task<ImmutableArray<CertificateInfo>> GetCertificatesAsync()
    {
        if (!Directory.Exists(TopLevelFolderName))
        {
            return ImmutableArray<CertificateInfo>.Empty;
        }

        var result = new List<CertificateInfo>();
        var domains = Directory.GetDirectories(TopLevelFolderName);

        foreach (var domain in domains)
        {
            var path = Path.Combine(TopLevelFolderName, domain);
            var times = Directory.GetDirectories(path);

            foreach (var time in times)
            {
                var info = new CertificateInfo
                {
                    Domain = domain,
                    Obtained = DateTime.MinValue,
                    Expiry = DateTime.MinValue,
                    Base64Pfx = await File.ReadAllTextAsync(Path.Combine(path, time, PfxFileName)),
                    FullChainPem = await File.ReadAllTextAsync(Path.Combine(path, time, FullChainFileName)),
                    PrivateKeyPem = await File.ReadAllTextAsync(Path.Combine(path, time, PrivateKeyFileName))
                };

                result.Add(info);
            }
        }

        return result.ToImmutableArray();
    }
}