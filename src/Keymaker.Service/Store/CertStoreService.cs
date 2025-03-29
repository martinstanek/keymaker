using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
}